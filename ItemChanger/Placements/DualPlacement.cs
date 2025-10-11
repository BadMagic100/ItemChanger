using ItemChanger.Containers;
using ItemChanger.Costs;
using ItemChanger.Events;
using ItemChanger.Tags;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ItemChanger.Placements;

/// <summary>
/// Placement which handles switching between two possible locations according to a test.
/// </summary>
public class DualPlacement(string Name) : Placement(Name), IContainerPlacement, ISingleCostPlacement, IPrimaryLocationPlacement
{
    public required Location TrueLocation { get; init; }
    public required Location FalseLocation { get; init; }

    public required IBool Test { get; init; }

    private bool cachedValue;

    public string ContainerType { get; set; } = ContainerRegistry.UnknownContainerType;
    public override string MainContainerType => ContainerType;

    [JsonIgnore]
    public Location Location => cachedValue ? TrueLocation : FalseLocation;

    public Cost? Cost { get; set; }

    protected override void DoLoad()
    {
        cachedValue = Test.Value;
        TrueLocation.Placement = this;
        FalseLocation.Placement = this;
        SetContainerType();
        Location.LoadOnce();
        Cost?.LoadOnce();
        GameEvents.OnBeginSceneTransition += OnBeginSceneTransition;
    }

    protected override void DoUnload()
    {
        Location.UnloadOnce();
        Cost?.UnloadOnce();
        GameEvents.OnBeginSceneTransition -= OnBeginSceneTransition;
    }

    private void OnBeginSceneTransition(Transition obj)
    {
        bool value = Test.Value;
        if (cachedValue != value)
        {
            Location.UnloadOnce();
            cachedValue = value;
            Location.LoadOnce();
        }
    }

    // MutablePlacement implementation of GetContainer
    public void GetContainer(Location location, out GameObject obj, out string containerType)
    {
        if (this.ContainerType == ContainerRegistry.UnknownContainerType)
        {
            this.ContainerType = MutablePlacement.ChooseContainerType(this, location as Locations.ContainerLocation, Items);
        }

        ContainerRegistry reg = ItemChangerHost.Singleton.ContainerRegistry;

        containerType = this.ContainerType;
        Container? container = reg.GetContainer(containerType);
        if (container is null || !container.SupportsInstantiate)
        {
            // this means that the container that was chosen on load isn't valid
            // most likely due from switching from a noninstantiatable ECL to a CL
            // so, we make a shiny but we don't modify the saved container type
            containerType = reg.DefaultSingleItemContainer.Name;
            container = reg.DefaultSingleItemContainer;
        }

        obj = container.GetNewContainer(new ContainerInfo(container.Name, this, location.FlingType, Cost)
        { ContainerType = containerType });
    }

    private void SetContainerType()
    {
        uint requestedCapabilities = GetPlacementAndLocationTags()
            .OfType<INeedsContainerCapability>()
            .Select(x => x.RequestedCapabilities)
            .Aggregate(0u, (acc, next) => acc | next);
        if (Cost != null)
        {
            requestedCapabilities |= ContainerCapabilities.PAY_COSTS;
        }

        ContainerRegistry reg = ItemChangerHost.Singleton.ContainerRegistry;
        if (reg.GetContainer(ContainerType)?.SupportsAll(true, requestedCapabilities) == true)
        {
            return;
        }

        Locations.ContainerLocation? cl = (FalseLocation as Locations.ContainerLocation) ?? (TrueLocation as Locations.ContainerLocation);
        if (cl == null)
        {
            return;
        }

        ContainerType = MutablePlacement.ChooseContainerType(this, cl, Items); // container type already failed the initial test
    }

    public override IEnumerable<Tag> GetPlacementAndLocationTags()
    {
        return base.GetPlacementAndLocationTags()
            .Concat(FalseLocation.tags ?? Enumerable.Empty<Tag>())
            .Concat(TrueLocation.tags ?? Enumerable.Empty<Tag>());
    }
}
