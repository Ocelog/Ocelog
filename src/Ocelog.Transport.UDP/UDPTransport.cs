using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Ocelog.Transport.UDP
{
    public static class UDPTransport
    {
        public static IObserver<FormattedLogEvent> Send(string host, int port)
        {
            return new SendLogsToUdp(host, port);
        }

        internal class SendLogsToUdp : IObserver<FormattedLogEvent>
        {
            private UdpClient _client;

            public SendLogsToUdp(string host, int port)
            {
                _client = new UdpClient(host, port);
            }

            public void OnCompleted()
            {
                _client.Close();
            }

            public void OnError(Exception error)
            {
            }

            public void OnNext(FormattedLogEvent value)
            {
                byte[] packet = Encoding.ASCII.GetBytes(value.Content);
                _client.Send(packet, packet.Length);
            }
        }
    }
}
