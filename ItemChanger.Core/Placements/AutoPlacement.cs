using System.Collections.Generic;
using System.Linq;
using ItemChanger.Costs;
using ItemChanger.Locations;
using ItemChanger.Tags;

namespace ItemChanger.Placements;

/// <summary>
/// Placement for self-implementing locations, usually acting through cutscene or conversation FSMs.
/// </summary>
public class AutoPlacement(string Name)
    : Placement(Name),
        IPrimaryLocationPlacement,
        ISingleCostPlacement
{
    /// <summary>
    /// Location responsible for delivering the placement logic.
    /// </summary>
    public required AutoLocation Location { get; init; }

    Location IPrimaryLocationPlacement.Location => Location;

    /// <summary>
    /// Optional cost that the <see cref="AutoLocation"/> may enforce.
    /// </summary>
    public Cost? Cost { get; set; }

    /// <summary>
    /// Indicates whether the underlying <see cref="AutoLocation"/> can enforce costs.
    /// </summary>
    public virtual bool SupportsCost => Location.SupportsCost;

    /// <summary>
    /// Connects the location to this placement and loads it once.
    /// </summary>
    protected override void DoLoad()
    {
        Location.Placement = this;
        Location.LoadOnce();
    }

    /// <summary>
    /// Unloads the underlying location.
    /// </summary>
    protected override void DoUnload()
    {
        Location.UnloadOnce();
    }

    /// <summary>
    /// Merges placement tags with the tags provided by the auto location.
    /// </summary>
    public override IEnumerable<Tag> GetPlacementAndLocationTags()
    {
        return base.GetPlacementAndLocationTags().Concat(Location.Tags ?? Enumerable.Empty<Tag>());
    }
}
