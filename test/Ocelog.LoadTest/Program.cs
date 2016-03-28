using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocelog.Formatting.Json;
using Ocelog.Formatting.Logstash;
using Ocelog.Transport.Http;
using Ocelog.Transport.UDP;
using System.Reactive.Linq;
using System.Linq;
using System.Xml.Linq;

namespace Ocelog.LoadTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = BuildLogger();

            foreach (var i in Enumerable.Range(1, 1000))
            {
                logger.Info(new { Message = "Test", Number = i, RandomNumber = new Random().Next(100), ListOfThings = GetListOfThings() });
            }
        }

        private static object[] GetListOfThings()
        {
            return Enumerable.Range(1, 10).Select(n => new { NumberWang = n }).ToArray();
        }

        static Logger BuildLogger()
        {
            var xml = XElement.Load("credentials.xml");
            string elasticsearchUrl = xml.Element("elasticsearchUrl").Value;
            string elasticsearchUsername = xml.Element("elasticsearchUsername").Value;
            string elasticsearchPassword = xml.Element("elasticsearchPassword").Value;

            return new Logger(logEvents =>
            {
                logEvents
                    .Process(BasicFormatting.Process)
                    .Format(JsonFormatter.Format)
                    .Subscribe(ev => Console.WriteLine(ev.Content));

                logEvents
                    .AddTimestamp()
                    .AddLevelToTags()
                    .AddTagsToAdditionalFields()
                    .AddCallerInfoToAdditionalFields()
                    .AddFields(new { type = "my_logging", host = System.Net.Dns.GetHostName() })
                    .Process(LogstashJson.Process)
                    .Select(FieldNameFormatting.ToSnakeCase())
                    .Format(JsonFormatter.Format)
                    .Subscribe(HttpTransport.Post(elasticsearchUrl, ev => $"logstash-{ev.Timestamp.ToString("yyyy.MM.dd")}/logs", elasticsearchUsername, elasticsearchPassword));
            });
        }
    }
}