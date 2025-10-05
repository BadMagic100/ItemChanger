namespace ItemChanger;

public interface IString
{
    string Value { get; }
    IString Clone();
}

public class BoxedString(string Value) : IString
{
    public string Value { get; set; } = Value;

    public IString Clone() => (IString)MemberwiseClone();
}
