using System;
using ItemChanger.Items;

namespace ItemChanger.Events.Args;

/// <summary>
/// Event arguments used when listeners can provide or override an item lookup.
/// </summary>
public class GetItemEventArgs(string itemName) : EventArgs
{
    /// <summary>
    /// Name requested by the finder.
    /// </summary>
    public string ItemName => itemName;

    /// <summary>
    /// Replacement item supplied by a listener, if any.
    /// </summary>
    public Item? Current { get; set; }
}
