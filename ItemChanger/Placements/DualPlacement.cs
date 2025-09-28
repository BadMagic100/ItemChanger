using ItemChanger.Containers;
using ItemChanger.Tags;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ItemChanger.Placements;

/// <summary>
/// Placement which handles switching between two possible locations according to a test.
/// </summary>
public class DualPlacement : Placement, IContainerPlacement, ISingleCostPlacement, IPrimaryLocationPlacement
{
    public DualPlacement(string Name) : base(Name) { }

    public required Location TrueLocation { get; init; }
    public required Location FalseLocation { get; init; }

    public required IBool Test { get; init; }

    private bool cachedValue;

    public string ContainerType { get; set; } = Container.Unknown;
    public override string MainContainerType => ContainerType;

    [JsonIgnore]
    public Location Location => cachedValue ? TrueLocation : FalseLocation;

    public Cost? Cost { get; set; }

    protected override void OnLoad()
    {
        cachedValue = Test.Value;
        TrueLocation.Placement = this;
        FalseLocation.Placement = this;
        SetContainerType();
        Location.Load();
        Cost?.Load();
        GameEvents.OnBeginSceneTransition += OnBeginSceneTransition;
    }

    protected override void OnUnload()
    {
        Location.Unload();
        Cost?.Unload();
        GameEvents.OnBeginSceneTransition -= OnBeginSceneTransition;
    }

    private void OnBeginSceneTransition(Transition obj)
    {
        bool value = Test.Value;
        if (cachedValue != value)
        {
            Location.Unload();
            cachedValue = value;
            Location.Load();
        }
    }

    // MutablePlacement implementation of GetContainer
    public void GetContainer(Location location, out GameObject obj, out string containerType)
    {
        if (this.ContainerType == Container.Unknown)
        {
            this.ContainerType = MutablePlacement.ChooseContainerType(this, location as Locations.ContainerLocation, Items);
        }

        containerType = this.ContainerType;
        Container? container = Container.GetContainer(containerType);
        if (container is null || !container.SupportsInstantiate)
        {
            // this means that the container that was chosen on load isn't valid
            // most likely due from switching from a noninstantiatable ECL to a CL
            // so, we make a shiny but we don't modify the saved container type
            containerType = Container.GetDefaultSingleItemContainer().Name;
            container = Container.GetContainer(containerType)!;
        }

        obj = container.GetNewContainer(new ContainerInfo(container.Name, this, location.FlingType, Cost,
            location.GetTags<Tags.ChangeSceneTag>().FirstOrDefault()?.ToChangeSceneInfo())
        { ContainerType = containerType });
    }

    private void SetContainerType()
    {
        bool mustSupportCost = Cost != null;
        bool mustSupportSceneChange = FalseLocation.GetTags<Tags.ChangeSceneTag>().Any()
            || TrueLocation.GetTags<Tags.ChangeSceneTag>().Any() || GetTags<Tags.ChangeSceneTag>().Any();
        if (Container.SupportsAll(ContainerType, true, mustSupportCost, mustSupportSceneChange))
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
