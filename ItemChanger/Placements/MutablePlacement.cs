using ItemChanger.Containers;
using ItemChanger.Costs;
using ItemChanger.Items;
using ItemChanger.Locations;
using ItemChanger.Tags;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ItemChanger.Placements;

/// <summary>
/// The default placement for most use cases.
/// Chooses an item container for its location based on its item list.
/// </summary>
public class MutablePlacement(string Name) : Placement(Name), IContainerPlacement, ISingleCostPlacement, IPrimaryLocationPlacement
{
    public required ContainerLocation Location { get; init; }
    Location IPrimaryLocationPlacement.Location => Location;

    public override string MainContainerType => ContainerType;
    public string ContainerType { get; set; } = ContainerRegistry.UnknownContainerType;

    public Cost? Cost { get; set; }

    protected override void DoLoad()
    {
        Location.Placement = this;
        Location.LoadOnce();
        Cost?.LoadOnce();
    }

    protected override void DoUnload()
    {
        Location.UnloadOnce();
        Cost?.UnloadOnce();
    }

    public void GetContainer(Location location, out GameObject obj, out string containerType)
    {
        if (this.ContainerType == ContainerRegistry.UnknownContainerType)
        {
            this.ContainerType = ChooseContainerType(this, location as ContainerLocation, Items);
        }

        containerType = this.ContainerType;
        Container? container = ItemChangerHost.Singleton.ContainerRegistry.GetContainer(containerType);
        if (container == null || !container.SupportsInstantiate)
        {
            this.ContainerType = containerType = ChooseContainerType(this, location as ContainerLocation, Items);
            container = ItemChangerHost.Singleton.ContainerRegistry.GetContainer(containerType);
            if (container == null)
            {
                throw new InvalidOperationException($"Unable to resolve container type {containerType} for placement {Name}!");
            }
        }

        obj = container.GetNewContainer(new ContainerInfo(container.Name, this, location.FlingType, Cost)
        { ContainerType = containerType });
    }

    public static string ChooseContainerType<T>(T placement, ContainerLocation? location, IEnumerable<Item> items) where T : Placement, ISingleCostPlacement
    {
        ContainerRegistry reg = ItemChangerHost.Singleton.ContainerRegistry;
        if (location?.ForceDefaultContainer ?? true)
        {
            return ItemChangerHost.Singleton.ContainerRegistry.DefaultSingleItemContainer.Name;
        }

        uint requestedCapabilities = placement.GetPlacementAndLocationTags()
            .OfType<INeedsContainerCapability>()
            .Select(x => x.RequestedCapabilities)
            .Aggregate(0u, (acc, next) => acc | next);
        if (placement.Cost != null)
        {
            requestedCapabilities |= ContainerCapabilities.PAY_COSTS;
        }


        HashSet<string> unsupported = [
            .. placement.GetPlacementAndLocationTags()
                .OfType<UnsupportedContainerTag>()
                .Select(t => t.ContainerType)
        ];

        string? containerType = items
            .Select(i => i.GetPreferredContainer())
            .FirstOrDefault(c => location.Supports(c) && !unsupported.Contains(c) && reg.GetContainer(c)?.SupportsAll(true, requestedCapabilities) == true);

        if (string.IsNullOrEmpty(containerType))
        {
            if (placement.GetPlacementAndLocationTags().OfType<PreferredDefaultContainerTag>().FirstOrDefault() is PreferredDefaultContainerTag t
                && reg.GetContainer(t.ContainerType)?.SupportsAll(true, requestedCapabilities) == true)
            {
                containerType = t.ContainerType;
            }
            // has more than 1 item and can support the default multi item container
            else if (items.Skip(1).Any()
                && !unsupported.Contains(ItemChangerHost.Singleton.ContainerRegistry.DefaultMultiItemContainer.Name)
                && reg.DefaultMultiItemContainer.SupportsAll(true, requestedCapabilities))
            {
                containerType = reg.DefaultMultiItemContainer.Name;
            }
            else
            {
                containerType = reg.DefaultSingleItemContainer.Name;
            }
        }

        return containerType;
    }

    public override IEnumerable<Tag> GetPlacementAndLocationTags()
    {
        return base.GetPlacementAndLocationTags().Concat(Location.tags ?? Enumerable.Empty<Tag>());
    }
}
