using ItemChanger.Enums;
using ItemChanger.Events;
using ItemChanger.Items;
using ItemChanger.Placements;

namespace ItemChanger.Locations;

/// <summary>
/// Location for giving items at the start of the scene, late enough that they appear on the UI and soul is not removed if during respawn.
/// </summary>
public class StartLocation : AutoLocation
{
    public MessageType MessageType;

    protected override void DoLoad()
    {
        LifecycleEvents.OnSafeToGiveItems += OnSafeToGiveItems;
    }

    protected override void DoUnload()
    {
        LifecycleEvents.OnSafeToGiveItems -= OnSafeToGiveItems;
    }


    private void OnSafeToGiveItems()
    {
        GiveItems();
    }

    private void GiveItems()
    {
        if (!Placement.AllObtained())
        {
            Placement.GiveAll(new GiveInfo
            {
                MessageType = MessageType,
                Container = "Start",
                FlingType = FlingType,
                Transform = null,
                Callback = null,
            });
        }
    }


    public override Placement Wrap()
    {
        return new Placements.AutoPlacement(Name)
        {
            Location = this,
        };
    }
}
