using System;
using System.Net.Sockets;
using System.Text;

namespace Ocelog.Transport.UDP
{
    public static class UDPTransport
    {
        public static IObserver<FormattedLogEvent> Send(string host, int port)
        {
            return new SendLogsToLogstash(host, port);
        }

        internal class SendLogsToLogstash : IObserver<FormattedLogEvent>
        {
            private readonly UdpClient _client;

            public SendLogsToLogstash(string host, int port)
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
