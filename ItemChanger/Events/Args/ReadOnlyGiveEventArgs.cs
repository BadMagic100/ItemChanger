﻿using System;
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
    public MessageType MessageType => info.MessageType;
    public Action<Item>? Callback => info.Callback;
    public ObtainState OriginalState => state;
}
