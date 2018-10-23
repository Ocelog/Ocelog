using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Ocelog.Transport.Test
{
    class TcpReceiver : IDisposable
    {
        TcpListener _listener;
        CancellationTokenSource _cancel;
        bool _expectSocketExceptions;
        ConcurrentQueue<TcpClient> _clients = new ConcurrentQueue<TcpClient>();
        ConcurrentQueue<string> _lines = new ConcurrentQueue<string>();

        private TcpReceiver(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _cancel = new CancellationTokenSource();
        }

        public IEnumerable<string> Lines => _lines;

        async Task Start()
        {
            new Thread(GatherConnections).Start(_listener);
            await Task.Delay(50);
        }

        void GatherConnections(object state)
        {
            var listener = (TcpListener)state;
            try
            {
                listener.Start();

                while (!_cancel.IsCancellationRequested)
                {
                    if (listener.Pending())
                    {
                        var client = listener.AcceptTcpClient();
                        _clients.Enqueue(client);
                        new Thread(GatherLines).Start(client);
                    }
                }
            }
            catch(IOException ex)
            {
                if (!_expectSocketExceptions) throw;
            }
            finally
            {
                listener.Stop();
            }
        }

        public async Task ResetSocket()
        {
            _cancel.Cancel();
            await Task.Delay(50);

            _expectSocketExceptions = true;

            foreach(var client in _clients)
            {
                client.Client.Close();
            }
        }

        void GatherLines(object state)
        {
            var client = (TcpClient)state;
            try
            {
                using (var reader = new StreamReader(client.GetStream()))
                {
                    while (!_cancel.IsCancellationRequested)
                    {
                        var line = reader.ReadLine();
                        _lines.Enqueue(line);
                    }
                }
            }
            catch(IOException ex)
            {
                if (!_expectSocketExceptions) throw;
            }
            finally
            {
                client.Close();
            }
        }

        void IDisposable.Dispose()
        {
            _cancel.Cancel();
        }

        public async static Task<TcpReceiver> Receive(int port)
        {
            var receiver = new TcpReceiver(port);
            await receiver.Start();
            return receiver;
        }
    }

}
