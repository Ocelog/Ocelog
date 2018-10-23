using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Ocelog.Transport.Test
{
    public class TcpTransportTests
    {
        int _port;
        IObserver<FormattedLogEvent> _sender;
        
        public TcpTransportTests()
        {
            _port = RandomPort();
        }

        [Fact]
        public async void Sends_content_to_tcp_endpoint()
        {
            using (var receiver = await TcpReceiver.Receive(_port))
            {
                _sender = CreateSender();
                _sender.OnNext(new FormattedLogEvent { Content = "Hello1" });
                await Task.Delay(100);
                
                Assert.Equal("Hello1", receiver.Lines.Single());
            }
        }

        [Fact]
        public async void Reconnects_when_connection_closes()
        {
            using (var receiver = await TcpReceiver.Receive(_port))
            {
                _sender = CreateSender();
                _sender.OnNext(new FormattedLogEvent { Content = "Hello1" });
                await Task.Delay(50);

                Assert.Equal("Hello1", receiver.Lines.Single());

                await receiver.ResetSocket();
            }
            
            using (var receiver = await TcpReceiver.Receive(_port))
            {
                _sender.OnNext(new FormattedLogEvent { Content = "Hello2" });
                
                await Task.Delay(1500); //enough time for sender to retry
                
                Assert.Equal("Hello2", receiver.Lines.Single());
            }
        }

        [Fact]
        public void Fails_fast_if_no_connection_on_start()
        {
            Assert.ThrowsAny<Exception>(() => CreateSender());            
        }

        [Fact]
        public async Task Copes_under_parallel_load()
        {
            using (var receiver = await TcpReceiver.Receive(_port))
            {
                _sender = CreateSender();

                var messages = Enumerable.Range(0, 5000)
                                .Select(i => i.ToString())
                                .ToArray();

                Parallel.ForEach(messages, m =>
                {
                    _sender.OnNext(new FormattedLogEvent { Content = m });
                });
                                
                await Task.Delay(2000);

                var received = new HashSet<string>(receiver.Lines);
                Assert.All(messages, m => received.Contains(m));
            }
        }
        
        IObserver<FormattedLogEvent> CreateSender()
            => TcpTransport.Send("127.0.0.1", _port);

        static int RandomPort()
            => new Random().Next(10000, 20000);
        
    }        
}
