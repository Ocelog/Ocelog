﻿using System;
using System.Collections.Generic;
using Xunit;

namespace Ocelog.Test
{
    public class TagTests
    {
        [Fact]
        public void should_have_empty_list_of_tags()
        {
            var logEvent = new LogEvent();

            Assert.Empty(logEvent.Tags);
        }

        [Fact]
        public void should_add_tags_to_event()
        {
            var logEvent = new LogEvent();

            logEvent.AddTag("mytag");

            Assert.Contains("mytag", logEvent.Tags);
        }

        [Fact]
        public void should_add_tags_when_event_logged()
        {
            var output = new List<LogEvent>();

            var logger = new Logger(logEvents => logEvents
                .AddTag("mytag")
                .Subscribe(log => output.Add(log))
                );

            var obj = new { message = "Hi" };
            logger.Info(obj);

            Assert.Collection(output,
                log => Assert.Equal(new[] { "mytag" }, log.Tags));
        }
    }
}
