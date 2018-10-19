using System;
using System.IO;
using System.Net.Sockets;

namespace Ocelog.Transport
{
    public static class TcpTransport
    {
        public static IObserver<FormattedLogEvent> Send(string host, int port)
        {
            return new Sender(host, port);
        }

        internal class Sender : IObserver<FormattedLogEvent>
        {
            readonly TcpClient _tcp;
            NetworkStream _stream;
            StreamWriter _writer;

            public Sender(string host, int port)
            {
                _tcp = new TcpClient();
                _tcp.Connect(host, port);
                _stream = _tcp.GetStream();
                _writer = new StreamWriter(_stream);
            }

            public void OnCompleted()
            {
                _tcp.Close();
            }

            public void OnError(Exception error)
            {
                //...
            }

            public void OnNext(FormattedLogEvent value)
            {
                _writer.WriteLine(value.Content);
                _writer.Flush();
            }
        }
    }
}
