using System;
using System.Collections.Generic;
using System.Linq;
using ItemChanger.Containers;
using ItemChanger.Costs;
using ItemChanger.Items;
using ItemChanger.Locations;
using ItemChanger.Logging;
using ItemChanger.Tags;

namespace ItemChanger.Placements;

/// <summary>
/// The default placement for most use cases.
/// Chooses an item container for its location based on its item list.
/// </summary>
public class MutablePlacement(string Name)
    : Placement(Name),
        IContainerPlacement,
        ISingleCostPlacement,
        IPrimaryLocationPlacement
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

    public void GetContainer(Location location, out Container container, out ContainerInfo info)
    {
        string containerType;
        if (this.ContainerType == ContainerRegistry.UnknownContainerType)
        {
            this.ContainerType = ChooseContainerType(this, location as ContainerLocation, Items);
        }

        containerType = this.ContainerType;
        Container? candidateContainer = ItemChangerHost.Singleton.ContainerRegistry.GetContainer(
            containerType
        );
        if (candidateContainer == null || !candidateContainer.SupportsInstantiate)
        {
            this.ContainerType = containerType = ChooseContainerType(
                this,
                location as ContainerLocation,
                Items
            );
            candidateContainer = ItemChangerHost.Singleton.ContainerRegistry.GetContainer(
                containerType
            );
            if (candidateContainer == null)
            {
                throw new InvalidOperationException(
                    $"Unable to resolve container type {containerType} for placement {Name}!"
                );
            }
        }

        container = candidateContainer;
        info = new ContainerInfo(candidateContainer.Name, this, location.FlingType, Cost)
        {
            ContainerType = containerType,
        };
    }

    public static string ChooseContainerType<T>(
        T placement,
        ContainerLocation? location,
        IEnumerable<Item> items
    )
        where T : Placement, ISingleCostPlacement
    {
        ContainerRegistry reg = ItemChangerHost.Singleton.ContainerRegistry;
        if (location?.ForceDefaultContainer ?? true)
        {
            return ItemChangerHost.Singleton.ContainerRegistry.DefaultSingleItemContainer.Name;
        }

        uint requestedCapabilities = placement
            .GetPlacementAndLocationTags()
            .OfType<INeedsContainerCapability>()
            .Select(x => x.RequestedCapabilities)
            .Aggregate(0u, (acc, next) => acc | next);
        if (placement.Cost != null)
        {
            requestedCapabilities |= ContainerCapabilities.PAY_COSTS;
        }

        HashSet<string> unsupported =
        [
            .. placement
                .GetPlacementAndLocationTags()
                .OfType<UnsupportedContainerTag>()
                .Select(t => t.ContainerType),
        ];

        OriginalContainerTag? originalContainerTag = placement
            .GetPlacementAndLocationTags()
            .OfType<OriginalContainerTag>()
            .FirstOrDefault();

        // if original container has priority over item preferences, determine whether it is meets the location's needs and force as needed
        if (
            originalContainerTag != null
            && (originalContainerTag.Force || originalContainerTag.Priority)
        )
        {
            Container? originalContainer = ItemChangerHost.Singleton.ContainerRegistry.GetContainer(
                originalContainerTag.ContainerType
            );
            if (originalContainer != null)
            {
                if (
                    !unsupported.Contains(originalContainerTag.ContainerType)
                    && originalContainer.SupportsAll(false, requestedCapabilities)
                )
                {
                    return originalContainerTag.ContainerType;
                }
                else if (originalContainerTag.Force)
                {
                    LoggerProxy.LogWarn(
                        $"During container selection for {placement.Name}, the container "
                            + $"{originalContainer.Name} was forced despite being unsupported by the location or missing "
                            + $"necessary capabilities."
                    );
                    return originalContainerTag.ContainerType;
                }
            }
        }

        // original container was not prioritized, try item preferences first
        string? containerType = items
            .Select(i => i.GetPreferredContainer())
            .FirstOrDefault(c =>
                location.Supports(c)
                && !unsupported.Contains(c)
                && reg.GetContainer(c)?.SupportsAll(true, requestedCapabilities) == true
            );

        if (string.IsNullOrEmpty(containerType))
        {
            if (
                originalContainerTag != null
                && reg.GetContainer(originalContainerTag.ContainerType)
                    ?.SupportsAll(true, requestedCapabilities) == true
            )
            {
                containerType = originalContainerTag.ContainerType;
            }
            // has more than 1 item and can support the default multi item container
            else if (
                items.Skip(1).Any()
                && !unsupported.Contains(
                    ItemChangerHost.Singleton.ContainerRegistry.DefaultMultiItemContainer.Name
                )
                && reg.DefaultMultiItemContainer.SupportsAll(true, requestedCapabilities)
            )
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
        return base.GetPlacementAndLocationTags().Concat(Location.Tags ?? Enumerable.Empty<Tag>());
    }
}
