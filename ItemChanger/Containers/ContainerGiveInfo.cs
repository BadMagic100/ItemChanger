using ItemChanger.Items;
using ItemChanger.Placements;
using System.Collections.Generic;

namespace ItemChanger.Containers;

/// <summary>
/// Instructions for a container to give items.
/// </summary>
public class ContainerGiveInfo
{
    public required IEnumerable<Item> Items { get; init; }
    public required Placement Placement { get; init; }
    public required FlingType FlingType { get; init; }
    public bool Applied { get; set; }
}
