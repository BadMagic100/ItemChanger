using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ItemChanger.Serialization.Converters;

internal class VectorConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Vector2) || objectType == typeof(Vector3);
    }

    public override object? ReadJson(
        JsonReader reader,
        Type objectType,
        object? existingValue,
        JsonSerializer serializer
    )
    {
        if (reader.TokenType == JsonToken.StartObject)
        {
            JObject obj = serializer.Deserialize<JObject>(reader)!;
            float x = obj["x"]?.Value<float>() ?? 0;
            float y = obj["y"]?.Value<float>() ?? 0;
            float z = obj["z"]?.Value<float>() ?? 0;
            if (objectType == typeof(Vector3))
            {
                return new Vector3(x, y, z);
            }
            else
            {
                return new Vector2(x, y);
            }
        }
        return null;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        Vector3 vec = (Vector3)value!;

        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(vec.x);
        writer.WritePropertyName("y");
        writer.WriteValue(vec.y);
        writer.WritePropertyName("z");
        writer.WriteValue(vec.z);
        writer.WriteEndObject();
    }
}
