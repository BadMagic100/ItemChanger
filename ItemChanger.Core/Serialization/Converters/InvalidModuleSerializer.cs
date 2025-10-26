using System;
using ItemChanger.Modules;
using Newtonsoft.Json;

namespace ItemChanger.Serialization.Converters;

/// <summary>
/// Converter which erases the InvalidModule during serialization and writes the JSON which it wraps.
/// </summary>
internal sealed class InvalidModuleSerializer : JsonConverter<InvalidModule>
{
    public override bool CanRead => false;
    public override bool CanWrite => true;

    public override InvalidModule? ReadJson(
        JsonReader reader,
        Type objectType,
        InvalidModule? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        throw new NotImplementedException();
    }

    public override void WriteJson(
        JsonWriter writer,
        InvalidModule? value,
        JsonSerializer serializer
    )
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        value.JSON.WriteTo(writer);
    }
}
