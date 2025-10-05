using ItemChanger.Components;
using ItemChanger.Costs;
using ItemChanger.Items;
using System.Collections.Generic;
using UnityEngine;

namespace ItemChanger.Containers;

/// <summary>
/// Data for instructing a Container class to make changes. The ContainerGiveInfo field must not be null.
/// </summary>
public class ContainerInfo
{
    public required string ContainerType { get; init; }

    public ContainerGiveInfo GiveInfo { get; init; }
    public ContainerCostInfo? CostInfo { get; init; }

    /// <summary>
    /// Creates ContainerInfo with standard ContainerGiveInfo.
    /// </summary>
    public ContainerInfo(string containerType, Placement placement, FlingType flingType)
        : this(containerType, placement, placement.Items, flingType)
    {
    }

    /// <summary>
    /// Creates ContainerInfo with standard ContainerGiveInfo.
    /// </summary>
    public ContainerInfo(string containerType, Placement placement, IEnumerable<Item> items, FlingType flingType)
        : this()
    {
        ContainerType = containerType;
        GiveInfo = new()
        {
            Placement = placement,
            Items = items,
            FlingType = flingType,
        };
    }

    /// <summary>
    /// Creates ContainerInfo with standard ContainerGiveInfo. If the cost parameter is not null, initializes costInfo with the cost.
    /// </summary>
    public ContainerInfo(string containerType, Placement placement, FlingType flingType, Cost? cost)
        : this(containerType, placement, placement.Items, flingType, cost)
    {
    }

    /// <summary>
    /// Creates ContainerInfo with standard ContainerGiveInfo. If the cost parameter is not null, initializes costInfo with the cost.
    /// </summary>
    public ContainerInfo(string containerType, Placement placement, IEnumerable<Item> items, FlingType flingType, Cost? cost)
        : this(containerType, placement, items, flingType)
    {
        if (cost is not null)
        {
            CostInfo = new()
            {
                Placement = placement,
                Cost = cost,
                PreviewItems = items,
            };
        }
    }

    public ContainerInfo(string containerType, ContainerGiveInfo giveInfo, ContainerCostInfo? costInfo)
    {
        ContainerType = containerType;
        GiveInfo = giveInfo;
        CostInfo = costInfo;
    }


    /// <summary>
    /// Creates uninitialized ContainerInfo. The giveInfo and containerType fields must be initialized before use.
    /// </summary>
    public ContainerInfo()
    {
    }

    /// <summary>
    /// Searches for ContainerInfo on a ContainerInfoComponent. Returns null if neither is found.
    /// </summary>
    public static ContainerInfo? FindContainerInfo(GameObject obj)
    {
        ContainerInfoComponent cdc = obj.GetComponent<ContainerInfoComponent>();
        if (cdc != null)
        {
            return cdc.info;
        }

        return null;
    }
}
