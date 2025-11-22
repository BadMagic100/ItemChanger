using System;
using ItemChanger.Enums;
using ItemChanger.Items;
using ItemChanger.Placements;
using UnityEngine;

namespace ItemChanger.Events.Args;

public class ReadOnlyGiveEventArgs(
    Item orig,
    Item item,
    Placement? placement,
    GiveInfo info,
    ObtainState state
) : EventArgs
{
    public Item Orig => orig;
    public Item Item => item;
    public Placement? Placement => placement;
    public string? Container => info.Container;
    public FlingType Fling => info.FlingType;
    public Transform? Transform => info.Transform;

    /// <summary>UI message types permitted for showing the item.</summary>
    public MessageTypes MessageType => info.MessageType;

    /// <summary>Callback invoked after the UI message completes.</summary>
    public Action<Item>? Callback => info.Callback;
    public ObtainState OriginalState => state;
}
