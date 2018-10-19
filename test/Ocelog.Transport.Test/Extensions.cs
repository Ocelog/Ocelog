using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Ocelog.Transport.Test
{
    public static class Extensions
    {
        public static IObservable<TcpClient> ReceiveConnections(this IObservable<int> ports)
            => ports
                .SelectMany(port =>
                {
                    var listener = new TcpListener(IPAddress.Any, port);
                    listener.Start();
                    return Observable.FromAsync(listener.AcceptTcpClientAsync);
                });

        public static IObservable<string> ReadLines(this IObservable<TcpClient> conns)
            => conns
                .Select(cn => new StreamReader(cn.GetStream()))
                .SelectMany(reader => Observable.FromAsync(() => reader.ReadLineAsync()));

    }
}
