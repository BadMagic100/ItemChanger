using System;
using ItemChanger.Tags;
using Newtonsoft.Json;

namespace ItemChanger.Serialization.Converters;

/// <summary>
/// Converter which erases the InvalidTag during serialization and writes the JSON which it wraps.
/// </summary>
internal class InvalidTagSerializer : JsonConverter<InvalidTag>
{
    public override bool CanRead => false;
    public override bool CanWrite => true;

    public override InvalidTag? ReadJson(
        JsonReader reader,
        Type objectType,
        InvalidTag? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        throw new NotImplementedException();
    }

    public override void WriteJson(JsonWriter writer, InvalidTag? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        value.JSON.WriteTo(writer);
    }
}
