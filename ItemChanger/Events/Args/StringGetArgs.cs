using ItemChanger.Serialization;
using System;

namespace ItemChanger.Events.Args;

public class StringGetArgs : EventArgs
{
    public IString Source { get; }
    public string Orig { get; }
    public string Current { get; set; }

    public StringGetArgs(IString source)
    {
        Source = source;
        Current = Orig = source.Value;
    }
}
