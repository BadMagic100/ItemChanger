using System;
using System.Collections.Generic;
using ItemChanger.Enums;
using ItemChanger.Modules;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ItemChanger.Serialization.Converters;

internal class ModuleCollectionConverter : JsonConverter<ModuleCollection>
{
    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override ModuleCollection? ReadJson(
        JsonReader reader,
        Type objectType,
        ModuleCollection? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        JToken jt = JToken.Load(reader);
        if (jt.Type == JTokenType.Null)
        {
            return null;
        }
        else if (jt.Type == JTokenType.Array)
        {
            JArray ja = (JArray)jt;
            List<Module> list = new(ja.Count);
            foreach (JToken jModule in ja)
            {
                Module t;
                try
                {
                    t = jModule.ToObject<Module>(serializer)!;
                }
                catch (Exception e)
                {
                    ModuleHandlingFlags flags =
                        ((JObject)jModule)
                            .GetValue(nameof(Module.ModuleHandlingProperties))
                            ?.ToObject<ModuleHandlingFlags>(serializer) ?? ModuleHandlingFlags.None;
                    if (flags.HasFlag(ModuleHandlingFlags.AllowDeserializationFailure))
                    {
                        t = new InvalidModule { JSON = jModule, DeserializationError = e };
                    }
                    else
                    {
                        throw;
                    }
                }
                list.Add(t);
            }
            return [.. list];
        }
        throw new JsonSerializationException("Unable to handle tag list pattern: " + jt.ToString());
    }

    public override void WriteJson(
        JsonWriter writer,
        ModuleCollection? value,
        JsonSerializer serializer
    )
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteStartArray();
        foreach (Module module in value)
        {
            serializer.Serialize(writer, module, typeof(Module));
        }
        writer.WriteEndArray();
    }
}
