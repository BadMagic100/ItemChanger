using ItemChanger.Locations;
using System.Collections.Generic;
using System.Linq;

namespace ItemChanger.Placements;

/// <summary>
/// Placement for self-implementing locations, usually acting through cutscene or conversation fsms.
/// </summary>
public class AutoPlacement(string Name) : Placement(Name), IPrimaryLocationPlacement, ISingleCostPlacement
{
    public required AutoLocation Location { get; init; }

    Location IPrimaryLocationPlacement.Location => Location;

    public Cost? Cost { get; set; }
    public virtual bool SupportsCost => Location.SupportsCost;

    protected override void OnLoad()
    {
        Location.Placement = this;
        Location.Load();
    }

    protected override void OnUnload()
    {
        Location.Unload();
    }

    public override IEnumerable<Tag> GetPlacementAndLocationTags()
    {
        return base.GetPlacementAndLocationTags().Concat(Location.tags ?? Enumerable.Empty<Tag>());
    }
}
