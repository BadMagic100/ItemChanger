namespace ItemChanger;

public interface IString
{
    string Value { get; }
    IString Clone();
}

public class BoxedString : IString
{
    public string Value { get; set; }

    public BoxedString(string Value) => this.Value = Value;
    
    public IString Clone() => (IString)MemberwiseClone();
}
