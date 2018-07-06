using System;
using Newtonsoft.Json;
using LucidJson;

namespace LucidGeometry.Converters 
{
    public class GeometryJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ILucidGeometry) ||
                    objectType == typeof(ILucidVertex);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType == typeof(ILucidVertex)) {
                var coords = serializer.Deserialize<Array<double>>(reader);
                return new LucidVertex(coords[0], coords[1]);
            }

            var tokenString = reader.ReadAsString();
            return GeometryHelper.Create(tokenString);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new InvalidOperationException();
        }
    }
}
