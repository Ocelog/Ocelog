# Ocelog

## Log objects not strings

Structured Logging is now becoming more and more common. Logging complex objects into a document store allows for analysis to be performed on log, so we can alert, diagnose, and track changes in behaviour much more easily than with traditional message logging.

See http://engineering.laterooms.com/structured-logging-with-elk-part-1/ , http://engineering.laterooms.com/structured-logging-with-elk-part-2/ and http://engineering.laterooms.com/structured-logging-with-elk-part-3/ to understand the reasons this logging library came about.

## Adding context

### Additional Fields

Extra context fields that can be used to help filtering later. Every logger can have a number of additional fields added to it, to provide context to the detail being logged.

### Tags

Tags are a list of strings associated with the log event.

### Caller Info

File path and line number of where the log event originated from.

## Request Logging

    _requestLog = logger.StartRequestLog();
    _requestLog.Add(new { Something = "Happened"});
    _requestLog.Add(new { Timing = 23});
    _requestLog.Complete();

## Configuration by code

By default Ocelog doesn't do very much, this is by design. The intention is to avoid the 'magic' of what is
happening under the hood.

    events 
        .Process(BasicFormatting.Process) 
        .Format(JsonFormatter.Format) 
        .Subscribe(e => System.Console.WriteLine(e.Content)); 

This is the most basic of configurations. `events` is an `Observeable` of `LogEvent`. This is your stream of log events that you can then process however you want. There are 3 important steps. Processing, Formatting and Subscribing. Processing is the act of taking the raw C# objects and converting them to a tree of Dictionary<string, object>, List<object>, and simple types which represents the final structure you want to log. Formatting takes the output of Processing and converts it to a string encoded in the out documents format e.g. JSON, XML etc. Subscribe is the final output, this is usually the transport that is used, e.g. UDP, TCP, AMPQ, Console of File.

Between these 3 steps is where you can do any customisation you want. Here is a more complete example.

    new Logger(logEvents => logEvents
        .AddTimestamp()
        .AddLevelToTags()
        .AddTagsToAdditionalFields()
        .AddCallerInfoToAdditionalFields()
        .AddFields(new { type = "my_logging", host = System.Net.Dns.GetHostName() })
        .Process(LogstashJson.Process)
        .Select(FieldNameFormatting.ToSnakeCase())
        .Select(MessageTrimming.TrimFields(1024))
        .Format(JsonFormatter.Format)
        .Subscribe(UDPTransport.Send("logstash.local", 2345)));

In this example first all the context information such as message level, caller info, and hostname is added first. Once the stream is processed we can tweak the over fields, by trimming the length of long string values, and converting all the field names to snake case. Finaly the events are sent over UDP to the given address and port.

## Examples

### Building a logger

    var logger = new Logger(logEvents => logEvents
        .Process(LogstashJson.Process)
        .Format(JsonFormatter.Format)
        .Subscribe(UDPTransport.Send("127.0.0.1", 789))
        );

### Basic Logging

    logger.Info(new { Message = "DBconnection Timing", SPROCName = sprocName, TimeInMs = time});
    logger.Warn(new { Message = "Connection failed", Server = serverName, Response = response});
    logger.Error(exception);

### Filtering

    var logger = new Logger(logEvents =>
    {
        logEvents
            .Where(log => log.Level == LogLevel.Warn)
            .Subscribe(warnOutput);
    }

### Logging to multiple outputs

    var logger = new Logger(logEvents =>
    {
        logEvents
            .Where(log => log.Level == LogLevel.Warn)
            .Subscribe(warnOutput);

        logEvents
            .Where(log => log.Level == LogLevel.Error)
            .Subscribe(errorOutput);

        logEvents
            .Subscribe(allOutput);
    });

