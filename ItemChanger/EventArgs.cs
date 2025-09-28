using ItemChanger.Items;
using System;
using UnityEngine;

namespace ItemChanger;

public class GetItemEventArgs : EventArgs
{
    public string ItemName { get; }
    public Item? Current { get; set; }

    public GetItemEventArgs(string itemName) => this.ItemName = itemName;
}

public class GetLocationEventArgs : EventArgs
{
    public string LocationName { get; }
    public Location? Current { get; set; }

    public GetLocationEventArgs(string locationName) => this.LocationName = locationName;
}

public class VisitStateChangedEventArgs : EventArgs
{
    public VisitStateChangedEventArgs(Placement placement, VisitState newFlags)
    {
        Placement = placement;
        NewFlags = newFlags;
        Orig = placement.Visited;
    }

    public Placement Placement { get; }
    public VisitState Orig { get; }
    public VisitState NewFlags { get; }
    public bool NoChange => (NewFlags & Orig) == NewFlags;
}

public class GiveEventArgs : EventArgs
{
    public GiveEventArgs(Item orig, Item item, Placement? placement, GiveInfo? info, ObtainState state)
    {
        this.Orig = orig;
        this.Item = item;
        this.Placement = placement;
        this.Info = info;
        this.OriginalState = state;
    }

    public Item Orig { get; }
    public Item? Item { get; set; }
    public Placement? Placement { get; }
    public GiveInfo? Info { get; set; }
    public ObtainState OriginalState { get; }
}

public class ReadOnlyGiveEventArgs : EventArgs
{
    private readonly GiveInfo info;

    public ReadOnlyGiveEventArgs(Item orig, Item item, Placement? placement, GiveInfo info, ObtainState state)
    {
        this.Orig = orig;
        this.Item = item;
        this.Placement = placement;
        this.info = info;
        this.OriginalState = state;
    }

    public Item Orig { get; }
    public Item Item { get; }
    public Placement? Placement { get; }
    public string? Container => info.Container;
    public FlingType Fling => info.FlingType;
    public Transform? Transform => info.Transform;
    public MessageType MessageType => info.MessageType;
    public Action<Item>? Callback => info.Callback;
    public ObtainState OriginalState { get; }
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
