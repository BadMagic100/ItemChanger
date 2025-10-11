using ItemChanger.Enums;
using ItemChanger.Items;
using ItemChanger.Placements;
using System;
using UnityEngine;

namespace ItemChanger.Events.Args;

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
