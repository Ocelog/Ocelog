using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Xunit;

namespace Ocelog.Transport.Test
{
    public class TcpTransportTests
    {
        ISubject<int> _ports;
        IObservable<TcpClient> _conns;
        IObservable<string> _lines;

        public TcpTransportTests()
        {
            _ports = new ReplaySubject<int>();
            _conns = _ports.ReceiveConnections();
            _lines = _conns.ReadLines();
        }
        
        [Fact]
        public async void should_send_content_to_tcp_endpoint()
        {
            var port = RandomPort();
            _ports.OnNext(port);
            
            TcpTransport.Send("127.0.0.1", port)
                .OnNext(new FormattedLogEvent { Content = "This is my content" });

            var received = await _lines.FirstAsync();
            Assert.Equal("This is my content", received);
        }

        [Fact]
        public async void Reconnects_when_necessary()
        {
            var port = RandomPort();
            _ports.OnNext(port);

            TcpTransport.Send("127.0.0.1", port)
                .OnNext(new FormattedLogEvent { Content = "This is my content" });

            _ports.OnNext(port); //reestablish connection (needs to trash existing conn)

            TcpTransport.Send("127.0.0.1", port)
                .OnNext(new FormattedLogEvent { Content = "This is my content" });

            var received = await _lines.ToArray().SingleAsync();
            Assert.Equal("This is my content", received[0]);
            Assert.Equal("This is my content", received[1]);
        }

        static int RandomPort()
            => new Random().Next(10000, 20000);

    }        
}
