using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Ocelog.Transport
{
    public class TcpTransport : IObserver<FormattedLogEvent>
    {
        readonly string _host;
        readonly int _port;
        
        IPropagatorBlock<string, string> _bufferBlock;
        ITargetBlock<string> _senderBlock;

        private TcpTransport(string host, int port)
        {
            _host = host;
            _port = port;

            _bufferBlock = new BufferBlock<string>();
            _senderBlock = CreateSenderBlock();
            _bufferBlock.LinkTo(_senderBlock);
        }

        public static IObserver<FormattedLogEvent> Send(string host, int port)
            => new TcpTransport(host, port);

        ITargetBlock<string> CreateSenderBlock()
        {
            TcpClient tcp;
            StreamWriter writer;
            var delay = (1000D, 1000D);
            var options = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 };

            Setup();

            var block = new ActionBlock<string>(Handle, options);
            block.Completion.ContinueWith(t => CleanUp());
            return block;
                
            void Setup()
            {
                tcp = new TcpClient(_host, _port);
                writer = new StreamWriter(tcp.GetStream());
            }
                
            async Task Handle(string body)
            {
                try
                {
                    await writer.WriteLineAsync(body);
                    await writer.FlushAsync();
                    ResetDelay();
                }
                catch (Exception ex)
                {
                    await Delay();
                    await Recover(ex, () => Handle(body));
                }
            }

            Task Recover(Exception exception, Func<Task> retry)
            {
                switch (exception)
                {
                    case IOException ex:
                        CleanUp();
                        Setup();
                        return retry();

                    case Exception ex when ex.InnerException != null:
                        return Recover(ex.InnerException, retry);

                    default:
                        throw new Exception("Unrecoverable error", exception);
                }
            }

            Task Delay()
            {
                var (orig, next) = delay;
                delay = (orig, next * 1.3);
                return Task.Delay((int)next);
            }

            void ResetDelay() {
                var (orig, next) = delay;
                delay = (orig, orig);
            }

            void CleanUp()
            {
                try { writer?.Close(); }
                catch(Exception) { }
 
                try { tcp?.Close(); }
                catch(Exception) { }                
            }
        }
            
        void IObserver<FormattedLogEvent>.OnCompleted()
            => _bufferBlock.Complete();

        void IObserver<FormattedLogEvent>.OnError(Exception error) { }

        void IObserver<FormattedLogEvent>.OnNext(FormattedLogEvent value)
            => _bufferBlock.Post(value.Content);
    }
}
