using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocelog.Formatting.Json;
using Ocelog.Processing.Logstash;
using Ocelog.Transport.Http;
using Ocelog.Transport.UDP;
using System.Reactive.Linq;
using System.Linq;
using System.Xml.Linq;
using Ocelog.Formatting.Elasticsearch;
using System.Reactive.Concurrency;

namespace Ocelog.LoadTest
{
    class Program
    {
        public static void Main(string[] args)
        {
            Run(args[0]);
        }

        private static void Run(string outputType)
        {
            using (var logger = BuildLogger(outputType))
            {
                foreach (var i in Enumerable.Range(1, 1000))
                {
                    logger.Info(new { Message = "Test", Number = i, RandomNumber = new Random().Next(100), ListOfThings = GetListOfThings() });
                }

                Console.WriteLine("Complete!");
                Console.ReadKey();
            }
        }

        private static object[] GetListOfThings()
        {
            return Enumerable.Range(1, 5).Select(n => new { NumberWang = n }).ToArray();
        }

        private static Logger BuildLogger(string outputType)
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

                var formattedEvents = logEvents
                    .AddTimestamp()
                    .AddLevelToTags()
                    .AddTagsToAdditionalFields()
                    .AddCallerInfoToAdditionalFields()
                    .AddFields(new { type = "my_logging", host = System.Net.Dns.GetHostName() })
                    .Process(LogstashJson.Process)
                    .Select(FieldNameFormatting.ToSnakeCase());

                if (outputType == "es")
                {
                    formattedEvents
                        .Format(JsonFormatter.Format)
                        .ObserveOn(NewThreadScheduler.Default)
                        .Subscribe(HttpTransport.Post(elasticsearchUrl, ev => $"logstash-{ev.Timestamp.ToString("yyyy.MM.dd")}/logs", elasticsearchUsername, elasticsearchPassword));
                }

                if (outputType == "esbulk")
                {
                    formattedEvents
                        .Buffer(TimeSpan.FromSeconds(10), 100)
                        .Where(buffer => buffer.Any())
                        .ObserveOn(NewThreadScheduler.Default)
                        .Format(ElasticsearchBulkFormatter.Format(ev => $"logstash-{ev.Timestamp.ToString("yyyy.MM.dd")}", ev => "logs"))
                        .Subscribe(HttpTransport.Post(elasticsearchUrl, ev => "_bulk", elasticsearchUsername, elasticsearchPassword));
                }
            });
        }
    }
}