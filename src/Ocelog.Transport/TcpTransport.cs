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
            var options = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 };

            Setup();

            return new ActionBlock<string>(Handle, options);
                
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
                }
                catch (Exception ex)
                {
                    await Recover(ex, () => Handle(body));
                }
            }

            Task Recover(Exception exception, Func<Task> retry)
            {
                switch (exception)
                {
                    case IOException ex:
                        Setup();
                        return retry();

                    case Exception ex when ex.InnerException != null:
                        return Recover(ex.InnerException, retry);

                    default:
                        throw new Exception("Unrecoverable error", exception);
                }
            }
        }
            
        void IObserver<FormattedLogEvent>.OnCompleted()
            => _bufferBlock.Complete();

        void IObserver<FormattedLogEvent>.OnError(Exception error) { }

        void IObserver<FormattedLogEvent>.OnNext(FormattedLogEvent value)
            => _bufferBlock.Post(value.Content);
    }
}
