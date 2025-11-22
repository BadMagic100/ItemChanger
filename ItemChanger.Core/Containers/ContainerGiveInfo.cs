using System.Collections.Generic;
using ItemChanger.Enums;
using ItemChanger.Items;
using ItemChanger.Placements;

namespace ItemChanger.Containers;

/// <summary>
/// Instructions for a container to give items.
/// </summary>
public class ContainerGiveInfo
{
    /// <summary>
    /// Items that should be dispensed.
    /// </summary>
    public required IEnumerable<Item> Items { get; init; }

    /// <summary>
    /// Placement owning the items.
    /// </summary>
    public required Placement Placement { get; init; }

    /// <summary>
    /// Method used to fling items upon give.
    /// </summary>
    public required FlingType FlingType { get; init; }

    /// <summary>
    /// Indicates whether the give logic has been applied to the underlying GameObject.
    /// </summary>
    public bool Applied { get; set; }
}
