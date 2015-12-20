# Ocelog

## Log objects not strings

## Adding context

### Additional Fields

Extra context fields that can be used to help filtering later.

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

    new Logger(logEvents => logEvents
        .IgnoreLoggingExceptions()
        .AddTimestamp()
        .AddLevelToTags()
        .AddTagsToAdditionalFields()
        .AddCallerInfoToAdditionalFields()
        .AddFields(new { type = "my_logging", host = System.Net.Dns.GetHostName() })
        .Select(LogstashJson.Format)
        .Subscribe(UDPTransport.Send(logstashHostName, logstashPort)));

## Examples

### Building a logger

    var logger = new Logger(logEvents => logEvents
        .Select(LogstashJson.Format)
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

