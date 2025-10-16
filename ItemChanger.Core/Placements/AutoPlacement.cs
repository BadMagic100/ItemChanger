using System.Collections.Generic;
using System.Linq;
using ItemChanger.Costs;
using ItemChanger.Locations;
using ItemChanger.Tags;

namespace ItemChanger.Placements;

/// <summary>
/// Placement for self-implementing locations, usually acting through cutscene or conversation fsms.
/// </summary>
public class AutoPlacement(string Name)
    : Placement(Name),
        IPrimaryLocationPlacement,
        ISingleCostPlacement
{
    public required AutoLocation Location { get; init; }

    Location IPrimaryLocationPlacement.Location => Location;

    public Cost? Cost { get; set; }
    public virtual bool SupportsCost => Location.SupportsCost;

    protected override void DoLoad()
    {
        Location.Placement = this;
        Location.LoadOnce();
    }

    protected override void DoUnload()
    {
        Location.UnloadOnce();
    }

    public override IEnumerable<Tag> GetPlacementAndLocationTags()
    {
        return base.GetPlacementAndLocationTags().Concat(Location.Tags ?? Enumerable.Empty<Tag>());
    }
}
