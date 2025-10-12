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
    public required Cost Cost { get; init; }
    public required IEnumerable<Item> PreviewItems { get; init; }
    public required Placement Placement { get; init; }
    public bool Applied { get; set; }
}
