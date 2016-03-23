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

## Testing

Most testing frameworks can be very hard to test with. Having to parse strings in tests can make them very brittle, and changes to code can cause unrelated tests to fail. Also setting up logger just for your tests can be very tidious. To help with this Ocelog provides LoggingSpy to do much of the difficult work for you and keep you tests clear.

For the most basic tests that don't tests anything logging specific you can provide a logger to your code under test like this:

    var codeUnderTest = new SomethingThatLogs(new LoggingSpy().Logger);

If you want to test that your code writes to the log you can use LoggingSpys Assert methods.

    [Test]
    public void Test_a_log_is_written_out()
    {
        var loggingSpy = new LoggingSpy();
        var codeUnderTest = new SomethingThatLogs(loggingSpy.Logger);
        
        codeUnderTest.WriteOutLog();
        
        loggingSpy.AssertDidInfo(new { Message = "I wrote a log" });
    }

If the code does not write any matching logs, it will tell you which fields are missing or which values don't match in the execption that is thrown, allowing you to tell if your test is failing for the right reason if your about to make it pass, or tell you what is broken if you didn't mean to break it.

The assert methods also allow you to assert on anonymous types without resorting to reflection, and you can use Dictionary objects that match the equivilent fields if prefer.

If you are using a RequestLog, LoggingSpy combins all the added objects first, before testing the assertion, this means that your implementation can switch between a single log write and a RequestLog without breaking your tests.

If you need a little more control over your assertion you can provide a predicate instead of a value.

    loggingSpy.AssertDidInfo(new { Message = new Predicate<string>(m => m.Contains("wrote")) });

##Moving from development to production (Handling unexpected exceptions)

By default Ocelog does not catch any exceptions. This is by design, as it makes initial setup of your logger much easier as any exceptions thrown in logging code come out through your regular exception handling mechanism instead of being hidden in some debug trace. However you probably don't want this behvaiour in production so Ocelog provides a way of handling these exceptions:

    events
        .IgnoreLoggingErrors()
        .Process(BasicFormatting.Process)
        ...

Adding IgnoreLoggingErrors to the start of your logging pipeline means that any exceptions are simply caught and then ignored. You'll want to add this to teh start of your pipeline so all logging exception are handled. You can also log these exception using HandleLoggingErrors:

    events
        .HandleLoggingErrors(logErrors => logErrors
            .Process(BasicFormatting.Process)
            .Format(JsonFormatter.Format)
            .Subscribe(e => System.Console.WriteLine(e.Content)))
        .Process(BasicFormatting.Process)
        ...

This will log any exceptions thrown by the logging pipeline to the console. `logErrors` is another Observable the same as `events` and you can do exactly the same as you can with any other logging events.

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

