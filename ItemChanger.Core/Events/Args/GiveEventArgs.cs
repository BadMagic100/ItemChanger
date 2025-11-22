using System;
using ItemChanger.Enums;
using ItemChanger.Items;
using ItemChanger.Placements;

namespace ItemChanger.Events.Args;

/// <summary>
/// Event arguments allowing listeners to inspect or override an item before it is given.
/// </summary>
public class GiveEventArgs(
    Item orig,
    Item item,
    Placement? placement,
    GiveInfo? info,
    ObtainState state
) : EventArgs
{
    /// <summary>Original item identifier used for comparison.</summary>
    public Item Orig => orig;

    /// <summary>Item that will be given; can be replaced.</summary>
    public Item? Item { get; set; } = item;

    /// <summary>Placement that initiated the give operation.</summary>
    public Placement? Placement => placement;

    /// <summary>Additional give parameters that may be modified.</summary>
    public GiveInfo? Info { get; set; } = info;

    /// <summary>Obtain state before this give process started.</summary>
    public ObtainState OriginalState => state;
}
