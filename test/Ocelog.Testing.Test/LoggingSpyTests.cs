using System;
using System.Collections.Generic;
using Xunit;

namespace Ocelog.Testing.Test
{
    public class LoggingSpyTests
    {
        [Fact]
        public void should_allow_asserting_on_info()
        {
            var logSpy = new LoggingSpy();
            var expectedContent = new { Some = "Content" };

            logSpy.Logger.Info(expectedContent);

            logSpy.AssertDidInfo(expectedContent);
        }

        [Fact]
        public void should_allow_asserting_on_warnings()
        {
            var logSpy = new LoggingSpy();
            var expectedContent = new { Some = "Content" };

            logSpy.Logger.Warn(expectedContent);

            logSpy.AssertDidWarn(expectedContent);
        }

        [Fact]
        public void should_allow_asserting_on_errors()
        {
            var logSpy = new LoggingSpy();
            var expectedContent = new { Some = "Content" };

            logSpy.Logger.Error(expectedContent);

            logSpy.AssertDidError(expectedContent);
        }

        [Fact]
        public void should_allow_matching_by_predicates()
        {
            var logSpy = new LoggingSpy();
            var content = new { Some = 12 };

            logSpy.Logger.Info(content);

            logSpy.AssertDidInfo(new { Some = new Predicate<int>(n => n == 12) });
        }

        [Fact]
        public void should_not_throw_assertion_failed_if_log_matches()
        {
            var logSpy = new LoggingSpy();
            var expectedContent = new { Some = "Content" };

            logSpy.Logger.Info(expectedContent);

            logSpy.AssertDidInfo(expectedContent);
        }

        [Theory]
        [InlineData("Content", "Context")]
        [InlineData(12, 5)]
        [InlineData(12, "Content")]
        public void should_assert_when_no_matching_log_found(object actual, object expected)
        {
            var logSpy = new LoggingSpy();
            var content = new { Some = actual };

            logSpy.Logger.Info(content);

            Assert.Throws<LoggingAssertionFailed>(() =>
                logSpy.AssertDidInfo(new { Some = expected })
            );
        }

        [Fact]
        public void should_assert_when_at_least_one_matching_log_found()
        {
            var logSpy = new LoggingSpy();
            var content = new { Some = "Thing" };

            logSpy.Logger.Info(new { Other = "Content" });
            logSpy.Logger.Info(content);

            logSpy.AssertDidInfo(new { Some = "Thing" });
        }

        [Fact]
        public void should_state_name_of_missing_fields()
        {
            var logSpy = new LoggingSpy();
            var content = new { };

            logSpy.Logger.Info(new { Other = "Content" });
            logSpy.Logger.Info(content);

            Assert.Throws<LoggingAssertionFailed>(() =>
            {
                try
                {
                    logSpy.AssertDidInfo(new { Some = "Field", Second = 34 });
                }
                catch (LoggingAssertionFailed exception)
                {
                    Assert.Contains("Some", exception.Message);
                    Assert.Contains("Second", exception.Message);
                    throw;
                }
            });
        }

        [Theory]
        [InlineData(12, 34)]
        [InlineData(12, "Banana")]
        public void should_state_name_of_not_matching_fields(object actual, object expected)
        {
            var logSpy = new LoggingSpy();
            var content = new { Some = actual };

            logSpy.Logger.Info(content);

            Assert.Throws<LoggingAssertionFailed>(() =>
            {
                try
                {
                    logSpy.AssertDidInfo(new { Some = expected });
                }
                catch (LoggingAssertionFailed exception)
                {
                    Assert.Contains("Some", exception.Message);
                    throw;
                }
            });
        }

        [Theory]
        [InlineData(12, 34)]
        [InlineData(12, "Banana")]
        public void should_state_values_of_not_matching_fields(object actual, object expected)
        {
            var logSpy = new LoggingSpy();
            var content = new { Some = actual };

            logSpy.Logger.Info(content);

            Assert.Throws<LoggingAssertionFailed>(() =>
            {
                try
                {
                    logSpy.AssertDidInfo(new { Some = expected });
                }
                catch (LoggingAssertionFailed exception)
                {
                    Assert.Contains(Convert.ToString(actual), exception.Message);
                    Assert.Contains(Convert.ToString(expected), exception.Message);
                    throw;
                }
            });
        }

        [Theory]
        [InlineData(12)]
        [InlineData("Hi")]
        [InlineData(null)]
        public void should_state_name_of_fields_with_predicate_not_matching<T>(T val)
        {
            var logSpy = new LoggingSpy();
            var content = new { Some = val };

            logSpy.Logger.Info(content);

            Assert.Throws<LoggingAssertionFailed>(() =>
            {
                try
                {
                    logSpy.AssertDidInfo(new { Some = new Predicate<T>(n => false) });
                }
                catch (LoggingAssertionFailed exception)
                {
                    Assert.Contains("Some", exception.Message);
                    throw;
                }
            });
        }

        [Fact]
        public void should_show_deep_path_of_fields()
        {
            var logSpy = new LoggingSpy();
            var content = new { Some = new { Deep = new { } } };

            logSpy.Logger.Info(content);

            Assert.Throws<LoggingAssertionFailed>(() =>
            {
                try
                {
                    logSpy.AssertDidInfo(new { Some = new { Deep = new { Field = "Field" } } });
                }
                catch (LoggingAssertionFailed exception)
                {
                    Assert.Contains("Some.Deep.Field", exception.Message);
                    throw;
                }
            });
        }

        [Fact]
        public void should_show_deep_path_of_fields_when_comparing_to_dictionary()
        {
            var logSpy = new LoggingSpy();
            var content = new { Some = new { Deep = new { } } };

            logSpy.Logger.Info(content);

            Assert.Throws<LoggingAssertionFailed>(() =>
            {
                try
                {
                    logSpy.AssertDidInfo(ObjectMerging.ToDictionary(new { Some = new { Deep = new { Field = "Field" } } }));
                }
                catch (LoggingAssertionFailed exception)
                {
                    Assert.Contains("Some.Deep.Field", exception.Message);
                    throw;
                }
            });
        }

        [Fact]
        public void should_allow_asserting_on_request_log()
        {
            var logSpy = new LoggingSpy();
            var requestLog = logSpy.Logger.StartRequestLog();
            var expectedContent = new { Some = "Content" };

            requestLog.Add(expectedContent);

            requestLog.Complete();

            logSpy.AssertDidInfo(expectedContent);
        }

        [Fact]
        public void should_allow_asserting_on_unboxed_arrays()
        {
            var logSpy = new LoggingSpy();
            var expectedContent = new { Some = new[] { 1, 2 } };

            logSpy.Logger.Info(expectedContent);

            logSpy.AssertDidInfo(expectedContent);
        }

        [Fact]
        public void should_allow_asserting_on_unboxed_arrays_fail_due_to_order()
        {
            var logSpy = new LoggingSpy();
            var actualContent = new { Some = new[] { 1, 2 } };
            var expectedContent = new { Some = new[] { 2, 1 } };

            logSpy.Logger.Info(actualContent);

            Assert.Throws<LoggingAssertionFailed>(() =>
            {
                try
                {
                    logSpy.AssertDidInfo(expectedContent);
                }
                catch (LoggingAssertionFailed exception)
                {
                    Assert.Contains("Some[0]", exception.Message);
                    throw;
                }
            });
        }

        [Fact]
        public void should_allow_asserting_on_unboxed_arrays_fail_due_to_different_values()
        {
            var logSpy = new LoggingSpy();
            var actualContent = new { Some = new[] { 1, 2 } };
            var expectedContent = new { Some = new[] { 1, 3 } };

            logSpy.Logger.Info(actualContent);

            Assert.Throws<LoggingAssertionFailed>(() =>
            {
                try
                {
                    logSpy.AssertDidInfo(expectedContent);
                }
                catch (LoggingAssertionFailed exception)
                {
                    Assert.Contains("Some[1]", exception.Message);
                    throw;
                }
            });
        }

        [Fact]
        public void should_state_name_of_not_matching_collections_failed_due_to_length()
        {
            var logSpy = new LoggingSpy();
            var actualContent = new { Some = new[] { 1, 2 } };
            var expectedContent = new { Some = new[] { 1, 2, 3 } };

            logSpy.Logger.Info(actualContent);

            Assert.Throws<LoggingAssertionFailed>(() =>
            {
                try
                {
                    logSpy.AssertDidInfo(expectedContent);
                }
                catch (LoggingAssertionFailed exception)
                {
                    Assert.Contains("Some", exception.Message);
                    throw;
                }
            });
        }

        [Fact]
        public void should_allow_asserting_on_unboxed_arrays_in_requestlog()
        {
            var logSpy = new LoggingSpy();
            var requestLog = logSpy.Logger.StartRequestLog();
            var expectedContent = new { Some = new[] { 1, 2 } };

            requestLog.Add(expectedContent);

            requestLog.Complete();

            logSpy.AssertDidInfo(expectedContent);
        }

        [Fact]
        public void should_allow_asserting_on_unboxed_arrays_fail_due_to_order_in_requestlog()
        {
            var logSpy = new LoggingSpy();
            var requestLog = logSpy.Logger.StartRequestLog();
            var actualContent = new { Some = new[] { 1, 2 } };
            var expectedContent = new { Some = new[] { 2, 1 } };

            requestLog.Add(actualContent);

            requestLog.Complete();

            Assert.Throws<LoggingAssertionFailed>(() =>
            {
                try
                {
                    logSpy.AssertDidInfo(expectedContent);
                }
                catch (LoggingAssertionFailed exception)
                {
                    Assert.Contains("Some", exception.Message);
                    throw;
                }
            });
        }

        [Fact]
        public void should_not_treat_dictionaries_as_collections()
        {
            var logSpy = new LoggingSpy();
            var requestLog = logSpy.Logger.StartRequestLog();

            requestLog.Add(new Dictionary<string, object> { { "Blah", 1 } });

            requestLog.Complete();

            logSpy.AssertDidInfo(new Dictionary<string, object>() { { "Blah", new Predicate<int>(num => num >= 0) } });
        }
    }
}
