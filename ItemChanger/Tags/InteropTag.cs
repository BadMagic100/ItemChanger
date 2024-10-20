using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ItemChanger.Tags;

/// <summary>
/// An interface implemented by tags for sharing information between assemblies that do not strongly reference each other.
/// </summary>
public interface IInteropTag
{
    /// <summary>
    /// A description of the tag that can be recognized by consumers.
    /// </summary>
    string Message { get; }
    /// <summary>
    /// Returns true if the property name corresponds to a non-null value of the specified type, and outputs the casted value.
    /// </summary>
    bool TryGetProperty<T>(string propertyName, [NotNullWhen(true)] out T? value);
}

/// <summary>
/// Tag which provides the default implementation of IInteropTag.
/// </summary>
public class InteropTag : Tag, IInteropTag
{
    /// <inheritdoc/>
    public required string Message { get; init; }
    /// <summary>
    /// A read/writable dictionary of properties
    /// </summary>
    public Dictionary<string, object?> Properties = new();

    /// <inheritdoc/>
    public bool TryGetProperty<T>(string propertyName, [NotNullWhen(true)] out T? value)
    {
        if (propertyName == null || Properties == null || !Properties.TryGetValue(propertyName, out object? val) || val is not T t)
        {
            value = default;
            return false;
        }

        value = t;
        return true;
    }
}
