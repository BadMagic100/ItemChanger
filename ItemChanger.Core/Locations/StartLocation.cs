﻿using ItemChanger.Enums;
using ItemChanger.Items;
using ItemChanger.Placements;
using ItemChanger.Tags;

namespace ItemChanger.Locations;

/// <summary>
/// Location for giving items at the start of the scene, late enough that they appear on the UI and soul is not removed if during respawn.
/// </summary>
public class StartLocation : AutoLocation
{
    public MessageType MessageType { get; init; }

    protected override void DoLoad()
    {
        ItemChangerHost.Singleton.LifecycleEvents.OnSafeToGiveItems += OnSafeToGiveItems;
    }

    protected override void DoUnload()
    {
        ItemChangerHost.Singleton.LifecycleEvents.OnSafeToGiveItems -= OnSafeToGiveItems;
    }

    private void OnSafeToGiveItems()
    {
        GiveItems();
    }

    private void GiveItems()
    {
        if (!Placement!.AllObtained())
        {
            Placement.GiveAll(
                new GiveInfo
                {
                    MessageType = MessageType,
                    Container = "Start",
                    FlingType = FlingType,
                    Transform = null,
                    Callback = null,
                }
            );
        }
    }

    public override Placement Wrap()
    {
        return new AutoPlacement(Name)
        {
            Location = this,
            Cost = ImplicitCostTag.GetDefaultCost(this),
        };
    }
}
