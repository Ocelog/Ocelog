using System;
using System.Net.Sockets;
using System.Text;
using Xunit;

namespace Ocelog.Transport.Test
{
    public class UDPTransportTests
    {
        [Fact]
        public async void should_send_content_to_udp_endpoint()
        {
            var port = new Random().Next(10000, 20000);
            var client = new UdpClient(port);

            UDPTransport.Send("127.0.0.1", port)
                .OnNext(new FormattedLogEvent { Content = "This is my content" });

            var response = await client.ReceiveAsync();

            var receivedContent = Encoding.ASCII.GetString(response.Buffer);

            Assert.Equal("This is my content", receivedContent);
        }
    }
}
