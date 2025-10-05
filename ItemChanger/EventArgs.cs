using ItemChanger.Items;
using ItemChanger.Placements;
using System;
using UnityEngine;

namespace ItemChanger;

public class GetItemEventArgs(string itemName) : EventArgs
{
    public string ItemName { get; } = itemName;
    public Item? Current { get; set; }
}

public class GetLocationEventArgs(string locationName) : EventArgs
{
    public string LocationName { get; } = locationName;
    public Location? Current { get; set; }
}

public class VisitStateChangedEventArgs(Placement placement, VisitState newFlags) : EventArgs
{
    public Placement Placement { get; } = placement;
    public VisitState Orig { get; } = placement.Visited;
    public VisitState NewFlags { get; } = newFlags;
    public bool NoChange => (NewFlags & Orig) == NewFlags;
}

public class GiveEventArgs(Item orig, Item item, Placement? placement, GiveInfo? info, ObtainState state) : EventArgs
{
    public Item Orig { get; } = orig;
    public Item? Item { get; set; } = item;
    public Placement? Placement { get; } = placement;
    public GiveInfo? Info { get; set; } = info;
    public ObtainState OriginalState { get; } = state;
}

public class ReadOnlyGiveEventArgs(Item orig, Item item, Placement? placement, GiveInfo info, ObtainState state) : EventArgs
{
    private readonly GiveInfo info = info;

    public Item Orig { get; } = orig;
    public Item Item { get; } = item;
    public Placement? Placement { get; } = placement;
    public string? Container => info.Container;
    public FlingType Fling => info.FlingType;
    public Transform? Transform => info.Transform;
    public MessageType MessageType => info.MessageType;
    public Action<Item>? Callback => info.Callback;
    public ObtainState OriginalState { get; } = state;
}

public class StringGetArgs
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

public class SpriteGetArgs
{
    public ISprite Source { get; }
    public Sprite Orig { get; }
    public Sprite Current { get; set; }

    public SpriteGetArgs(ISprite source)
    {
        Source = source;
        Current = Orig = source.Value;
    }
}
