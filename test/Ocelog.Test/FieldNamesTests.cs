using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using System.Reactive.Linq;

namespace Ocelog.Test
{
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
    //    [InlineData("hotelID", "hotel_id")]
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
        //    [InlineData("hotelID", "hotelId")]
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
        //    [InlineData("hotelID", "hotelId")]
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
    }
}

