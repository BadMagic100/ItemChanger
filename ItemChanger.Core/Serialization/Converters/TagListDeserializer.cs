using System;
using System.Collections.Generic;
using ItemChanger.Enums;
using ItemChanger.Tags;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ItemChanger.Serialization.Converters;

internal sealed class TagListDeserializer : JsonConverter<List<Tag>>
{
    public override bool CanRead => true;
    public override bool CanWrite => false;

    public override List<Tag>? ReadJson(
        JsonReader reader,
        Type objectType,
        List<Tag>? existingValue,
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
            List<Tag> list = new(ja.Count);
            foreach (JToken jTag in ja)
            {
                Tag t;
                try
                {
                    t = jTag.ToObject<Tag>(serializer)!;
                }
                catch (Exception e)
                {
                    TagHandlingOptions flags =
                        ((JObject)jTag)
                            .GetValue(nameof(Tag.TagHandlingProperties))
                            ?.ToObject<TagHandlingOptions>(serializer) ?? TagHandlingOptions.None;
                    if (flags.HasFlag(TagHandlingOptions.AllowDeserializationFailure))
                    {
                        t = new InvalidTag { JSON = jTag, DeserializationError = e };
                    }
                    else
                    {
                        throw;
                    }
                }
                list.Add(t);
            }
            return list;
        }
        throw new JsonSerializationException("Unable to handle tag list pattern: " + jt.ToString());
    }

    public override void WriteJson(JsonWriter writer, List<Tag>? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
