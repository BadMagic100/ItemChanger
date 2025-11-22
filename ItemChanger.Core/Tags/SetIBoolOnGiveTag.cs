using ItemChanger.Enums;
using ItemChanger.Events.Args;
using ItemChanger.Items;
using ItemChanger.Locations;
using ItemChanger.Placements;
using ItemChanger.Serialization;

namespace ItemChanger.Tags;

/// <summary>
/// Tag which sets an IWriteableBool when its parent item is given.
/// <br/>If attached to a location or placement, sets the bool when VisitStates.ObtainedAnyItem is first set on the placement.
/// </summary>
public class SetIBoolOnGiveTag : Tag
{
    public required IWritableBool Bool { get; init; }
    public required bool Value { get; init; }

    protected override void DoLoad(TaggableObject parent)
    {
        if (parent is Item item)
        {
            item.OnGive += OnGive;
        }
        else
        {
            Placement? placement = parent as Placement ?? (parent as Location)?.Placement;
            if (placement is not null)
            {
                placement.OnVisitStateChanged += OnVisitStateChanged;
            }
        }
    }

    protected override void DoUnload(TaggableObject parent)
    {
        if (parent is Item item)
        {
            item.OnGive -= OnGive;
        }
        else
        {
            Placement? placement = parent as Placement ?? (parent as Location)?.Placement;
            if (placement is not null)
            {
                placement.OnVisitStateChanged -= OnVisitStateChanged;
            }
        }
    }

    private void OnGive(ReadOnlyGiveEventArgs obj)
    {
        Bool.Value = Value;
    }

    private void OnVisitStateChanged(VisitStateChangedEventArgs obj)
    {
        if (
            obj.NewFlags.HasFlag(VisitStates.ObtainedAnyItem)
            && !obj.Orig.HasFlag(VisitStates.ObtainedAnyItem)
        )
        {
            Bool.Value = Value;
        }
    }
}
