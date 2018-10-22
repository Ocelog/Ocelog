using System;
using System.IO;
using System.Net.Sockets;
using System.Reactive;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Ocelog.Transport
{
    public static class TcpTransport
    {
        public static IObserver<FormattedLogEvent> Send(string host, int port)
            => new Sender(host, port);

        class Sender : IObserver<FormattedLogEvent>
        {
            readonly string _host;
            readonly int _port;

            IPropagatorBlock<string, string> _bufferBlock;
            ITargetBlock<string> _senderBlock;

            public Sender(string host, int port)
            {
                _host = host;
                _port = port;

                _bufferBlock = new BufferBlock<string>();
                _senderBlock = CreateSenderBlock();
                _bufferBlock.LinkTo(_senderBlock);
            }
            
            ITargetBlock<string> CreateSenderBlock()
            {
                TcpClient tcp;
                StreamWriter writer;

                Setup();

                var senderBlock = new ActionBlock<string>(Handle, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 });
                
                return senderBlock;
                
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
            
            public void OnCompleted()
            {
                _bufferBlock.Complete();
            }

            public void OnError(Exception error)
            {
                _bufferBlock.Complete();
            }

            public void OnNext(FormattedLogEvent value)
            {
                _bufferBlock.Post(value.Content);
            }
        }
    }
    
}
