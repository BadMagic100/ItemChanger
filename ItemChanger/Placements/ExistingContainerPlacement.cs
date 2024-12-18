﻿using ItemChanger.Locations;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace ItemChanger.Placements;

/// <summary>
/// Placement which supports modifying existing containers in place or replacing them with a container preferred by the item list.
/// </summary>
public class ExistingContainerPlacement(string Name) : AbstractPlacement(Name), ISingleCostPlacement, IPrimaryLocationPlacement
{
    public required ExistingContainerLocation Location { get; init; }
    AbstractLocation IPrimaryLocationPlacement.Location => Location;

    [JsonProperty]
    private string currentContainerType = Container.Unknown;
    [JsonIgnore]
    public override string MainContainerType => currentContainerType;

    public Cost? Cost { get; set; }

    protected override void OnLoad()
    {
        Location.Placement = this;
        UpdateContainerType();
        Location.Load();
        Cost?.Load();
    }

    protected override void OnUnload()
    {
        Location.Unload();
        Cost?.Unload();
    }

    private void UpdateContainerType()
    {
        if (currentContainerType == Location.ContainerType)
        {
            return;
        }

        if (Location.NonReplaceable)
        {
            currentContainerType = Location.ContainerType;
            return;
        }
        if (Container.GetContainer(currentContainerType) is Container c && c.SupportsInstantiate)
        {
            return;
        }

        currentContainerType = ChooseContainerType(this, Location, Items);
    }

    public static string ChooseContainerType(ISingleCostPlacement placement, ExistingContainerLocation location, IEnumerable<AbstractItem> items)
    {
        if (location.NonReplaceable)
        {
            return location.ContainerType;
        }

        bool mustSupportCost = placement.Cost != null && !location.HandlesCostBeforeContainer;
        bool mustSupportSceneChange = location.GetTags<Tags.ChangeSceneTag>().Any() || ((AbstractPlacement)placement).GetTags<Tags.ChangeSceneTag>().Any();

        HashSet<string> unsupported = new(((placement as AbstractPlacement)?.GetPlacementAndLocationTags() ?? Enumerable.Empty<Tag>())
            .OfType<Tags.UnsupportedContainerTag>()
            .Select(t => t.ContainerType));

        string? containerType = items
            .Select(i => i.GetPreferredContainer())
            .FirstOrDefault(c => Container.GetContainer(c) is Container container && container.SupportsInstantiate && !unsupported.Contains(c) &&
            (!mustSupportCost || container.SupportsCost) && 
            (!mustSupportSceneChange || container.SupportsSceneChange));

        if (string.IsNullOrEmpty(containerType))
        {
            return mustSupportCost ? Container.GetDefaultSingleItemContainer().Name : location.ContainerType;
        }

        return containerType;
    }

    public override IEnumerable<Tag> GetPlacementAndLocationTags()
    {
        return base.GetPlacementAndLocationTags().Concat(Location.tags ?? Enumerable.Empty<Tag>());
    }
}
