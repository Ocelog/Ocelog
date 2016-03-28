using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Ocelog.Transport.Http
{
    public static class HttpTransport
    {
        public static IObserver<FormattedLogEvent> Post(string baseUrl, string path = null, string username = null, string password = null)
        {
            return Post(baseUrl, _ => path, username, password);
        }

        public static IObserver<FormattedLogEvent> Post(string baseUrl, Func<FormattedLogEvent, string> pathFunc, string username = null, string password = null)
        {
            return new PostLogsToHttp(baseUrl, pathFunc, username, password);
        }

        internal class PostLogsToHttp : IObserver<FormattedLogEvent>
        {
            private Func<FormattedLogEvent, string> _pathFunc;
            private HttpClient _client;

            public PostLogsToHttp(string baseUrl, Func<FormattedLogEvent, string> pathFunc, string username, string password)
            {
                _client = new HttpClient() { BaseAddress = new Uri(baseUrl) };
                if (!string.IsNullOrWhiteSpace(username))
                {
                    var byteArray = Encoding.ASCII.GetBytes($"{username}:{password}");
                    _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                }
                _pathFunc = pathFunc;

            }

            public void OnCompleted()
            {
                _client.Dispose();
            }

            public void OnError(Exception error)
            {
            }

            public void OnNext(FormattedLogEvent value)
            {
                var result = _client.PostAsync(_pathFunc(value), new StringContent(value.Content)).Result;
            }
        }
    }
}
