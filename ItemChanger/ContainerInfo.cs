using ItemChanger.Components;
using ItemChanger.Items;
using ItemChanger.Tags;
using System.Collections.Generic;
using UnityEngine;

namespace ItemChanger;

/// <summary>
/// Data for instructing a Container class to make changes. The ContainerGiveInfo field must not be null.
/// </summary>
public class ContainerInfo
{
    public required string ContainerType { get; init; }

    public ContainerGiveInfo GiveInfo { get; init; }
    public ChangeSceneInfo? ChangeSceneInfo { get; init; }
    public CostInfo? CostInfo { get; init; }

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
        this.ContainerType = containerType;
        this.GiveInfo = new()
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
            this.CostInfo = new()
            {
                Placement = placement,
                Cost = cost,
                PreviewItems = items,
            };
        }
    }

    /// <summary>
    /// Creates ContainerInfo with standard ContainerGiveInfo and the provided ChangeSceneInfo. If the cost parameter is not null, initializes costInfo with the cost.
    /// </summary>
    public ContainerInfo(string containerType, Placement placement, FlingType flingType, Cost? cost, ChangeSceneInfo? changeSceneInfo) 
        : this(containerType, placement, placement.Items, flingType, cost, changeSceneInfo)
    {
    }

    /// <summary>
    /// Creates ContainerInfo with standard ContainerGiveInfo and the provided ChangeSceneInfo. If the cost parameter is not null, initializes costInfo with the cost.
    /// </summary>
    public ContainerInfo(string containerType, Placement placement, IEnumerable<Item> items, FlingType flingType, Cost? cost, ChangeSceneInfo? changeSceneInfo)
        : this(containerType, placement, items, flingType, cost)
    {
        this.ChangeSceneInfo = changeSceneInfo;
    }

    public ContainerInfo(string containerType, ContainerGiveInfo giveInfo, CostInfo? costInfo, ChangeSceneInfo? changeSceneInfo)
    {
        this.ContainerType = containerType;
        this.GiveInfo = giveInfo;
        this.CostInfo = costInfo;
        this.ChangeSceneInfo = changeSceneInfo;
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
        var cdc = obj.GetComponent<ContainerInfoComponent>();
        if (cdc != null)
        {
            return cdc.info;
        }

        return null;
    }
}

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

/// <summary>
/// Instructions for a container to change scene.
/// </summary>
public class ChangeSceneInfo
{
    public const string door_dreamReturn = "door_dreamReturn";

    public Transition transition;
    public bool dreamReturn;
    public bool deactivateNoCharms;

    public bool applied;

    public ChangeSceneInfo() { }
    public ChangeSceneInfo(Transition transition)
    {
        this.transition = transition;
        this.dreamReturn = this.deactivateNoCharms = transition.GateName == door_dreamReturn;
    }
    public ChangeSceneInfo(Transition transition, bool dreamReturn)
    {
        this.transition = transition;
        this.dreamReturn = this.deactivateNoCharms = dreamReturn;
    }
    public ChangeSceneInfo(Transition transition, bool dreamReturn, bool deactivateNoCharms)
    {
        this.transition = transition;
        this.dreamReturn = dreamReturn;
        this.deactivateNoCharms = deactivateNoCharms;
    }
    public ChangeSceneInfo(ChangeSceneTag tag)
    {
        this.transition = tag.changeTo;
        this.dreamReturn= tag.dreamReturn;
        this.deactivateNoCharms = tag.deactivateNoCharms;
    }
}

/// <summary>
/// Instructions for a container to enforce a Cost.
/// </summary>
public class CostInfo
{
    public required Cost Cost { get; init; }
    public required IEnumerable<Item> PreviewItems { get; init; }
    public required Placement Placement { get; init; }
    public bool Applied { get; set; }
}
