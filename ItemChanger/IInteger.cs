namespace ItemChanger;

/// <summary>
/// Interface which can supply an int value. Used frequently for serializable int tests.
/// </summary>
public interface IInteger
{
    int Value { get; }
    IInteger Clone();
}

/// <summary>
/// IInteger which supports write operations.
/// </summary>
public interface IWritableInteger : IInteger
{
    new int Value { get; set; }
}

/// <summary>
/// IInteger which represents a constant value.
/// </summary>
public class BoxedInteger(int Value) : IWritableInteger
{
    public int Value { get; set; } = Value;

    public IInteger Clone() => (IInteger)MemberwiseClone();
}
