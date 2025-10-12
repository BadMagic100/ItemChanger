using System;
using ItemChanger.Enums;
using ItemChanger.Items;
using ItemChanger.Placements;

namespace ItemChanger.Events.Args;

public class GiveEventArgs(
    Item orig,
    Item item,
    Placement? placement,
    GiveInfo? info,
    ObtainState state
) : EventArgs
{
    public Item Orig { get; } = orig;
    public Item? Item { get; set; } = item;
    public Placement? Placement { get; } = placement;
    public GiveInfo? Info { get; set; } = info;
    public ObtainState OriginalState { get; } = state;
}
