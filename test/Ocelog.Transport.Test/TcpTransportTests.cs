using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Xunit;

namespace Ocelog.Transport.Test
{
    public class TcpTransportTests
    {
        int _port;
        Task<string> _receiving;

        public TcpTransportTests()
        {
            _port = new Random().Next(10000, 20000);
            _receiving = Receive(port: _port, timeout: 3000);      
        }
        
        [Fact]
        public async void should_send_content_to_tcp_endpoint()
        {
            TcpTransport.Send("127.0.0.1", _port)
                .OnNext(new FormattedLogEvent { Content = "This is my content" });

            var received = await _receiving;
            Assert.Equal("This is my content", received);
        }
        
        async Task<string> Receive(int port, int timeout)
        {
            var listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            var connecting = listener.AcceptTcpClientAsync();

            await Task.WhenAny(Task.Delay(timeout), connecting);
            if(!connecting.IsCompleted) throw new TimeoutException("No connection received!");

            var client = await connecting;
            var stream = client.GetStream();

            var reader = new StreamReader(stream);
            return await reader.ReadLineAsync();
        }
    }        
}
