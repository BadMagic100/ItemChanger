using System;
using ItemChanger.Serialization.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ItemChanger.Tags;

/// <summary>
/// Tag which failed to deserialize. Contains the raw data of the tag and the error which prevented deserialization.
/// </summary>
[JsonConverter(typeof(InvalidTagSerializer))]
public sealed class InvalidTag : Tag
{
    /// <summary>
    /// The raw data of the tag, as a JToken.
    /// </summary>
    public required JToken JSON { get; init; }

    /// <summary>
    /// The error thrown during deserialization.
    /// </summary>
    public required Exception DeserializationError { get; init; }
}
