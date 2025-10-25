using ItemChanger.Logging;

namespace ItemChanger.Items;

/// <summary>
/// An item that can be used to quickly test the behavior of a location without requiring full item implementations
/// </summary>
public class DebugItem : Item
{
    /// <inheritdoc/>
    public override void GiveImmediate(GiveInfo info)
    {
        LoggerProxy.LogInfo($"Given item {Name} with info: {{container={info.Container} fling={info.FlingType} msg={info.MessageType}}}");
    }
}
