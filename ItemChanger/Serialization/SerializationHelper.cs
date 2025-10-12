using System.IO;
using Newtonsoft.Json;

namespace ItemChanger.Serialization;

/// <summary>
/// Utility class containing the necessary serializer configuration to read and write ItemChanger objects to a stream as JSON.
/// </summary>
public static class SerializationHelper
{
    /// <summary>
    /// Utility to serialize an object with the necessary metadata for polymorphic deserialization
    /// </summary>
    /// <param name="stream">Stream to serialize to</param>
    /// <param name="o">The object to be serialized</param>
    public static void Serialize(Stream stream, object o)
    {
        JsonSerializer js = new()
        {
            DefaultValueHandling = DefaultValueHandling.Include,
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto,
        };

        js.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

        using StreamWriter sw = new(stream);
        js.Serialize(sw, o);
    }

    /// <summary>
    /// Utility to deserialize an object polymorphically for use in Finder (typically created from <see cref="Serialize(Stream, object)"/>).
    /// </summary>
    /// <typeparam name="T">The type to deserialize to</typeparam>
    /// <param name="stream">The stream to read from</param>
    public static T? DeserializeResource<T>(Stream stream)
    {
        JsonSerializer js = new() { TypeNameHandling = TypeNameHandling.Auto };
        js.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        using StreamReader sr = new(stream);
        return js.Deserialize<T>(new JsonTextReader(sr));
    }
}
