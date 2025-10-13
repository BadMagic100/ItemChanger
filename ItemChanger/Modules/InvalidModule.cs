using System;
using ItemChanger.Serialization.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ItemChanger.Modules;

/// <summary>
/// Module which failed to deserialize. Contains the raw data of the module and the error which prevented deserialization.
/// </summary>
[JsonConverter(typeof(InvalidModuleSerializer))]
public sealed class InvalidModule : Module
{
    /// <summary>
    /// The raw data of the module, as a JToken.
    /// </summary>
    public required JToken JSON { get; init; }

    /// <summary>
    /// The error thrown during deserialization.
    /// </summary>
    public required Exception DeserializationError { get; init; }

    /// <inheritdoc/>
    protected override void DoLoad() { }

    /// <inheritdoc/>
    protected override void DoUnload() { }
}
