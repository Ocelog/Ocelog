using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            Assert.True(logSpy.DidInfo(expectedContent));
        }

        [Fact]
        public void should_allow_asserting_on_warnings()
        {
            var logSpy = new LoggingSpy();
            var expectedContent = new { Some = "Content" };

            logSpy.Logger.Warn(expectedContent);

            Assert.True(logSpy.DidWarn(expectedContent));
        }

        [Fact]
        public void should_allow_asserting_on_errors()
        {
            var logSpy = new LoggingSpy();
            var expectedContent = new { Some = "Content" };

            logSpy.Logger.Error(expectedContent);

            Assert.True(logSpy.DidError(expectedContent));
        }

        [Theory]
        [InlineData("Content", "Context")]
        [InlineData(12, 5)]
        [InlineData(12, "Content")]
        public void should_be_false_when_no_matching_log_found(object actual, object expected)
        {
            var logSpy = new LoggingSpy();
            var content = new { Some = actual };

            logSpy.Logger.Info(content);

            Assert.False(logSpy.DidInfo(new { Some = expected }));
        }

        [Fact]
        public void should_not_match_value_and_complex_type()
        {
            var logSpy = new LoggingSpy();
            var content = new { Some = "Content" };

            logSpy.Logger.Info(content);

            Assert.False(logSpy.DidInfo(new { Some = new { Complex = false } }));
        }

        [Fact]
        public void should_not_match_complex_and_value_type()
        {
            var logSpy = new LoggingSpy();
            var content = new { Some = new { Complex = false } };

            logSpy.Logger.Info(content);

            Assert.False(logSpy.DidInfo(new { Some = "Content" }));
        }

        [Fact]
        public void should_allow_matching_of_same_shape()
        {
            var logSpy = new LoggingSpy();
            var content = new { Some = "Content" };

            logSpy.Logger.Info(content);

            Assert.True(logSpy.DidInfo(new { Some = "Content" }));
        }

        [Fact]
        public void should_allow_actual_to_have_extra_content()
        {
            var logSpy = new LoggingSpy();
            var content = new { Some = "Content", Extra = "Content" };

            logSpy.Logger.Info(content);

            Assert.True(logSpy.DidInfo(new { Some = "Content" }));
        }

        [Fact]
        public void should_allow_actual_to_have_extra_content_at_any_depth()
        {
            var logSpy = new LoggingSpy();
            var content = new { Some = "Content", Extra = new { Content = "blah", Extra = true } };

            logSpy.Logger.Info(content);

            Assert.True(logSpy.DidInfo(new { Some = "Content", Extra = new { Content = "blah" } }));
        }

        [Fact]
        public void should_not_allow_expected_to_have_extra_content_at_any_depth()
        {
            var logSpy = new LoggingSpy();
            var content = new { Some = "Content", Extra = new { Content = "blah" } };

            logSpy.Logger.Info(content);

            Assert.False(logSpy.DidInfo(new { Some = "Content", Extra = new { Content = "blah", Extra = true } }));
        }

        [Fact]
        public void should_allow_fields_to_be_ingored_with_null()
        {
            var logSpy = new LoggingSpy();
            var content = new TestData() { Prop1 = 2, Prop2 = "to ignore" };

            logSpy.Logger.Info(content);

            Assert.True(logSpy.DidInfo(new TestData() { Prop1 = 2, Prop2 = null }));
        }

        [Fact]
        public void should_not_allow_null_fields_to_match_non_null()
        {
            var logSpy = new LoggingSpy();
            var content = new TestData() { Prop1 = 2, Prop2 = null };

            logSpy.Logger.Info(content);

            Assert.False(logSpy.DidInfo(new TestData() { Prop1 = 2, Prop2 = "Somtehing here" }));
        }

        public class TestData
        {
            public object Prop1 { get; set; }
            public object Prop2 { get; set; }
        }
    }
}
