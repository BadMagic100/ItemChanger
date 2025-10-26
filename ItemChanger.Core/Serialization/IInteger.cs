namespace ItemChanger.Serialization;

/// <summary>
/// Interface which can supply an int value. Used frequently for serializable int tests.
/// </summary>
public interface IInteger : IFinderCloneable
{
    /// <summary>
    /// The defined value
    /// </summary>
    int Value { get; }
}

/// <summary>
/// IInteger which supports write operations.
/// </summary>
public interface IWritableInteger : IInteger
{
    /// <inheritdoc/>
    new int Value { get; set; }
}

/// <summary>
/// IInteger which represents a constant value.
/// </summary>
public class BoxedInteger(int Value) : IWritableInteger
{
    /// <inheritdoc/>
    public int Value { get; set; } = Value;
}
