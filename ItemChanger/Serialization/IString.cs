namespace ItemChanger.Serialization;

/// <summary>
/// Interface which allows representing functions to provide a string in a serializable manner
/// </summary>
public interface IString
{
    /// <summary>
    /// The defined string
    /// </summary>
    string Value { get; }

    /// <summary>
    /// Creates a deep copy of this string
    /// </summary>
    IString Clone();
}

/// <summary>
/// IString that represents a constant value
/// </summary>
public class BoxedString(string Value) : IString
{
    /// <inheritdoc/>
    public string Value { get; set; } = Value;

    /// <inheritdoc/>
    public IString Clone() => (IString)MemberwiseClone();
}
