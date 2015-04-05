using System;
using Newtonsoft.Json;

namespace Nxt.NET.Request
{
    /// <summary>
    /// Used to put quotes around long values when serializing to json, since the java client expects that.
    /// </summary>
    class AddQuoteConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var formatted = value.ToString();
            writer.WriteValue(formatted);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(long);
        }
    }
}