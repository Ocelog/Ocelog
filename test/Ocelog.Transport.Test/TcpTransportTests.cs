using System;
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
        public async void should_send_content_to_tcp_endpoint()
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

            await Task.Delay(100);

            using (var receiver = await TcpReceiver.Receive(_port))
            {
                _sender.OnNext(new FormattedLogEvent { Content = "Hello2" });
                await Task.Delay(50);
                
                Assert.Equal("Hello2", receiver.Lines.Single());
            }
        }

        [Fact(Skip = "Not impl")]
        public async void Copes_with_connection_not_being_immediately_available()
        {
            //...
        }

        IObserver<FormattedLogEvent> CreateSender()
            => TcpTransport.Send("127.0.0.1", _port);

        static int RandomPort()
            => new Random().Next(10000, 20000);
        
    }        
}
