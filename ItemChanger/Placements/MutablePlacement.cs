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
    public string ContainerType { get; set; } = Container.Unknown;

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
        if (this.ContainerType == Container.Unknown)
        {
            this.ContainerType = ChooseContainerType(this, location as ContainerLocation, Items);
        }

        containerType = this.ContainerType;
        Container? container = Container.GetContainer(containerType);
        if (container == null || !container.SupportsInstantiate)
        {
            this.ContainerType = containerType = ChooseContainerType(this, location as ContainerLocation, Items);
            container = Container.GetContainer(containerType);
            if (container == null)
            {
                throw new InvalidOperationException($"Unable to resolve container type {containerType} for placement {Name}!");
            }
        }

        obj = container.GetNewContainer(new ContainerInfo(container.Name, this, location.FlingType, Cost,
            location.GetTags<Tags.ChangeSceneTag>().FirstOrDefault()?.ToChangeSceneInfo())
        { ContainerType = containerType });
    }

    public static string ChooseContainerType(ISingleCostPlacement placement, ContainerLocation? location, IEnumerable<Item> items)
    {
        if (location?.ForceShiny ?? true)
        {
            return Container.GetDefaultSingleItemContainer().Name;
        }

        bool mustSupportCost = placement.Cost != null;
        bool mustSupportSceneChange = location.GetTags<Tags.ChangeSceneTag>().Any() || ((Placement)placement).GetTags<Tags.ChangeSceneTag>().Any();

        HashSet<string> unsupported = new(((placement as Placement)?.GetPlacementAndLocationTags() ?? Enumerable.Empty<Tag>())
            .OfType<Tags.UnsupportedContainerTag>()
            .Select(t => t.ContainerType));

        string? containerType = items
            .Select(i => i.GetPreferredContainer())
            .FirstOrDefault(c => location.Supports(c) && !unsupported.Contains(c) && Container.SupportsAll(c, true, mustSupportCost, mustSupportSceneChange));

        if (string.IsNullOrEmpty(containerType))
        {
            if (((placement as Placement)?.GetPlacementAndLocationTags() ?? Enumerable.Empty<Tag>())
                .OfType<Tags.PreferredDefaultContainerTag>().FirstOrDefault() is Tags.PreferredDefaultContainerTag t
                && Container.SupportsAll(t.ContainerType, true, mustSupportCost, mustSupportSceneChange))
            {
                containerType = t.ContainerType;
            }
            else if (!mustSupportCost && !mustSupportSceneChange && !unsupported.Contains(Container.GetDefaultMultiItemContainer().Name)
                && items.Skip(1).Any()) // has more than 1 item, and can support Chest

            {
                containerType = Container.GetDefaultMultiItemContainer().Name;
            }
            else
            {
                containerType = Container.GetDefaultSingleItemContainer().Name;
            }
        }

        return containerType;
    }

    public override IEnumerable<Tag> GetPlacementAndLocationTags()
    {
        return base.GetPlacementAndLocationTags().Concat(Location.tags ?? Enumerable.Empty<Tag>());
    }
}
