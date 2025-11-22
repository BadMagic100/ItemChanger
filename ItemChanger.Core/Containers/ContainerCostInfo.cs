using System.Collections.Generic;
using ItemChanger.Costs;
using ItemChanger.Items;
using ItemChanger.Placements;

namespace ItemChanger.Containers;

/// <summary>
/// Instructions for a container to enforce a Cost.
/// </summary>
public class ContainerCostInfo
{
    /// <summary>
    /// Cost that the container should charge before dispensing items.
    /// </summary>
    public required Cost Cost { get; init; }

    /// <summary>
    /// Items whose previews should be shown alongside the cost.
    /// </summary>
    public required IEnumerable<Item> PreviewItems { get; init; }

    /// <summary>
    /// Placement that owns the items.
    /// </summary>
    public required Placement Placement { get; init; }

    /// <summary>
    /// Indicates whether the cost enforcement has already been applied.
    /// </summary>
    public bool Applied { get; set; }
}
