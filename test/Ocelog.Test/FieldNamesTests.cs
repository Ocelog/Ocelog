using System.Collections.Generic;
using System.Linq;
using Xunit;
using System.Reactive.Linq;

namespace Ocelog.Test
{
    using System;

    public class FieldNamesTests
    {
        [Theory]
        [InlineData("field", "field")]
        [InlineData("Field", "field")]
        [InlineData("Field Name", "field_name")]
        [InlineData("FieldName", "field_name")]
        [InlineData("fieldName", "field_name")]
        [InlineData("field_Name", "field_name")]
        [InlineData("field_name", "field_name")]
        [InlineData("Field_name", "field_name")]
        [InlineData("FieldName_Goes here", "field_name_goes_here")]
        [InlineData("hotelId", "hotel_id")]
        public void should_convert_fieldnames_to_snakecase(string originalFieldName, string finalFieldName)
        {
            const string valueToCheckFor = "valuetocheckfor";

            var obj = new Dictionary<string, object> { { originalFieldName, valueToCheckFor } };

            var output = new List<ProcessedLogEvent>();
            var logger = new Logger(logEvents => logEvents
                .Select(BasicFormatting.Process)
                .Select(FieldNameFormatting.ToSnakeCase())
                .Subscribe(log => output.Add(log))
                );

            logger.Info(obj);

            var field = output[0].Content.First();

            Assert.Equal(finalFieldName, field.Key);
            Assert.Equal(valueToCheckFor, field.Value);
        }

        [Fact]
        public void should_convert_fieldnames_to_snakecase_recursively()
        {
            const string valueToCheckFor = "valuetocheckfor";

            var obj = new Dictionary<string, object> { { "Base", new Dictionary<string, object> { { "FieldName", valueToCheckFor } } } };

            var output = new List<ProcessedLogEvent>();
            var logger = new Logger(logEvents => logEvents
                .Select(BasicFormatting.Process)
                .Select(FieldNameFormatting.ToSnakeCase())
                .Subscribe(log => output.Add(log))
                );

            logger.Info(obj);

            var topLevel = (Dictionary<string, object>)output[0].Content.First().Value;
            var field = topLevel.First();

            Assert.Equal("field_name", field.Key);
            Assert.Equal(valueToCheckFor, field.Value);
        }

        [Fact]
        public void should_convert_fieldnames_to_snakecase_recursively_through_lists()
        {
            const string valueToCheckFor = "valuetocheckfor";

            var obj = new Dictionary<string, object> { { "Base", new[] { new Dictionary<string, object> { { "FieldName", valueToCheckFor } } } } };

            var output = new List<ProcessedLogEvent>();
            var logger = new Logger(logEvents => logEvents
                .Select(BasicFormatting.Process)
                .Select(FieldNameFormatting.ToSnakeCase())
                .Subscribe(log => output.Add(log))
                );

            logger.Info(obj);

            var array = (IEnumerable<object>)output[0].Content.First().Value;
            var subObj = (Dictionary<string, object>)array.First();
            var field = subObj.First();

            Assert.Equal("field_name", field.Key);
            Assert.Equal(valueToCheckFor, field.Value);
        }

        [Theory]
        [InlineData("field", "Field")]
        [InlineData("Field", "Field")]
        [InlineData("Field Name", "FieldName")]
        [InlineData("FieldName", "FieldName")]
        [InlineData("fieldName", "FieldName")]
        [InlineData("field_Name", "FieldName")]
        [InlineData("field_name", "FieldName")]
        [InlineData("Field_name", "FieldName")]
        [InlineData("FieldName_Goes here", "FieldNameGoesHere")]
        [InlineData("hotelId", "HotelId")]
        public void should_convert_fieldnames_to_pascalcase(string originalFieldName, string finalFieldName)
        {
            const string valueToCheckFor = "valuetocheckfor";

            var obj = new Dictionary<string, object> { { originalFieldName, valueToCheckFor } };

            var output = new List<ProcessedLogEvent>();
            var logger = new Logger(logEvents => logEvents
                .Select(BasicFormatting.Process)
                .Select(FieldNameFormatting.ToPascalCase())
                .Subscribe(log => output.Add(log))
                );

            logger.Info(obj);

            var field = output[0].Content.First();

            Assert.Equal(finalFieldName, field.Key);
            Assert.Equal(valueToCheckFor, field.Value);
        }

        [Fact]
        public void should_convert_fieldnames_to_pascalcase_recursively()
        {
            const string valueToCheckFor = "valuetocheckfor";

            var obj = new Dictionary<string, object> { { "Base", new Dictionary<string, object> { { "field_Name", valueToCheckFor } } } };

            var output = new List<ProcessedLogEvent>();
            var logger = new Logger(logEvents => logEvents
                .Select(BasicFormatting.Process)
                .Select(FieldNameFormatting.ToPascalCase())
                .Subscribe(log => output.Add(log))
                );

            logger.Info(obj);

            var topLevel = (Dictionary<string, object>)output[0].Content.First().Value;
            var field = topLevel.First();

            Assert.Equal("FieldName", field.Key);
            Assert.Equal(valueToCheckFor, field.Value);
        }

        [Fact]
        public void should_convert_fieldnames_to_pascalcase_recursively_through_lists()
        {
            const string valueToCheckFor = "valuetocheckfor";

            var obj = new Dictionary<string, object> { { "Base", new[] { new Dictionary<string, object> { { "field_Name", valueToCheckFor } } } } };

            var output = new List<ProcessedLogEvent>();
            var logger = new Logger(logEvents => logEvents
                .Select(BasicFormatting.Process)
                .Select(FieldNameFormatting.ToPascalCase())
                .Subscribe(log => output.Add(log))
                );

            logger.Info(obj);

            var array = (IEnumerable<object>)output[0].Content.First().Value;
            var subObj = (Dictionary<string, object>)array.First();
            var field = subObj.First();

            Assert.Equal("FieldName", field.Key);
            Assert.Equal(valueToCheckFor, field.Value);
        }

        [Theory]
        [InlineData("field", "field")]
        [InlineData("Field", "field")]
        [InlineData("Field Name", "fieldName")]
        [InlineData("FieldName", "fieldName")]
        [InlineData("fieldName", "fieldName")]
        [InlineData("field_Name", "fieldName")]
        [InlineData("field_name", "fieldName")]
        [InlineData("Field_name", "fieldName")]
        [InlineData("FieldName_Goes here", "fieldNameGoesHere")]
        [InlineData("hotelId", "hotelId")]
        public void should_convert_fieldnames_to_camelcase(string originalFieldName, string finalFieldName)
        {
            const string valueToCheckFor = "valuetocheckfor";

            var obj = new Dictionary<string, object> { { originalFieldName, valueToCheckFor } };

            var output = new List<ProcessedLogEvent>();
            var logger = new Logger(logEvents => logEvents
                .Select(BasicFormatting.Process)
                .Select(FieldNameFormatting.ToCamelCase())
                .Subscribe(log => output.Add(log))
                );

            logger.Info(obj);

            var field = output[0].Content.First();

            Assert.Equal(finalFieldName, field.Key);
            Assert.Equal(valueToCheckFor, field.Value);
        }

        [Fact]
        public void should_convert_fieldnames_to_camelcase_recursively()
        {
            const string valueToCheckFor = "valuetocheckfor";

            var obj = new Dictionary<string, object> { { "Base", new Dictionary<string, object> { { "field_Name", valueToCheckFor } } } };

            var output = new List<ProcessedLogEvent>();
            var logger = new Logger(logEvents => logEvents
                .Select(BasicFormatting.Process)
                .Select(FieldNameFormatting.ToCamelCase())
                .Subscribe(log => output.Add(log))
                );

            logger.Info(obj);

            var topLevel = (Dictionary<string, object>)output[0].Content.First().Value;
            var field = topLevel.First();

            Assert.Equal("fieldName", field.Key);
            Assert.Equal(valueToCheckFor, field.Value);
        }

        [Fact]
        public void should_convert_fieldnames_to_Camelcase_recursively_through_lists()
        {
            const string valueToCheckFor = "valuetocheckfor";

            var obj = new Dictionary<string, object> { { "Base", new[] { new Dictionary<string, object> { { "field_Name", valueToCheckFor } } } } };

            var output = new List<ProcessedLogEvent>();
            var logger = new Logger(logEvents => logEvents
                .Select(BasicFormatting.Process)
                .Select(FieldNameFormatting.ToCamelCase())
                .Subscribe(log => output.Add(log))
                );

            logger.Info(obj);

            var array = (IEnumerable<object>)output[0].Content.First().Value;
            var subObj = (Dictionary<string, object>)array.First();
            var field = subObj.First();

            Assert.Equal("fieldName", field.Key);
            Assert.Equal(valueToCheckFor, field.Value);
        }
    }
}
