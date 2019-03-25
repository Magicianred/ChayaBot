using Newtonsoft.Json;
using System;

namespace ChayaBot.Core.Music.Songs
{
    public class DurationConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(TimeSpan));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            int value = Convert.ToInt32(reader.Value);
            return TimeSpan.FromSeconds(value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
