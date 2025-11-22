using System.Collections.Generic;
using System.Linq;
using ItemChanger.Containers;
using ItemChanger.Costs;
using ItemChanger.Events.Args;
using ItemChanger.Locations;
using ItemChanger.Serialization;
using ItemChanger.Tags;
using Newtonsoft.Json;

namespace ItemChanger.Placements;

/// <summary>
/// Placement which handles switching between two possible locations according to a test.
/// </summary>
public class DualPlacement(string Name)
    : Placement(Name),
        IContainerPlacement,
        ISingleCostPlacement,
        IPrimaryLocationPlacement
{
    /// <summary>
    /// Location used when <see cref="Test"/> evaluates to true.
    /// </summary>
    public required Location TrueLocation { get; init; }

    /// <summary>
    /// Location used when <see cref="Test"/> evaluates to false.
    /// </summary>
    public required Location FalseLocation { get; init; }

    /// <summary>
    /// Test determining which location is active.
    /// </summary>
    public required IBool Test { get; init; }

    private bool cachedValue;

    /// <summary>
    /// Container type currently assigned to the placement.
    /// </summary>
    public string ContainerType { get; private set; } = ContainerRegistry.UnknownContainerType;

    /// <summary>
    /// Gets the container type currently assigned to this placement.
    /// </summary>
    public override string MainContainerType => ContainerType;

    /// <summary>
    /// Gets the location currently selected by <see cref="Test"/>.
    /// </summary>
    [JsonIgnore]
    public Location Location => cachedValue ? TrueLocation : FalseLocation;

    /// <summary>
    /// Optional cost shared across both locations.
    /// </summary>
    public Cost? Cost { get; set; }

    /// <summary>
    /// Loads whichever location the test currently selects and hooks state-change tracking.
    /// </summary>
    protected override void DoLoad()
    {
        cachedValue = Test.Value;
        TrueLocation.Placement = this;
        FalseLocation.Placement = this;
        SetContainerType();
        Location.LoadOnce();
        Cost?.LoadOnce();
        ItemChangerHost.Singleton.GameEvents.BeforeNextSceneLoaded += BeforeNextSceneLoaded;
    }

    /// <summary>
    /// Unloads the active location and unhooks state-change tracking.
    /// </summary>
    protected override void DoUnload()
    {
        Location.UnloadOnce();
        Cost?.UnloadOnce();
        ItemChangerHost.Singleton.GameEvents.BeforeNextSceneLoaded -= BeforeNextSceneLoaded;
    }

    private void BeforeNextSceneLoaded(BeforeSceneLoadedEventArgs _)
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
    /// <summary>
    /// The <see cref="MutablePlacement"/> implementation for selecting a container at runtime.
    /// </summary>
    public void GetContainer(Location location, out Container container, out ContainerInfo info)
    {
        if (this.ContainerType == ContainerRegistry.UnknownContainerType)
        {
            this.ContainerType = MutablePlacement.ChooseContainerType(
                this,
                location as Locations.ContainerLocation,
                Items
            );
        }

        ContainerRegistry reg = ItemChangerHost.Singleton.ContainerRegistry;

        string containerType = this.ContainerType;
        Container? candidateContainer = reg.GetContainer(containerType);
        if (candidateContainer is null || !candidateContainer.SupportsInstantiate)
        {
            // this means that the container that was chosen on load isn't valid
            // most likely due from switching from a noninstantiatable ECL to a CL
            // so, we make a shiny but we don't modify the saved container type
            containerType = reg.DefaultSingleItemContainer.Name;
            candidateContainer = reg.DefaultSingleItemContainer;
        }

        container = candidateContainer;
        info = new ContainerInfo(candidateContainer.Name, this, location.FlingType, Cost)
        {
            ContainerType = containerType,
        };
    }

    private void SetContainerType()
    {
        uint requestedCapabilities = GetPlacementAndLocationTags()
            .OfType<INeedsContainerCapability>()
            .Select(x => x.RequestedCapabilities)
            .Aggregate(0u, (acc, next) => acc | next);
        if (Cost != null)
        {
            requestedCapabilities |= ContainerCapabilities.PayCosts;
        }

        ContainerRegistry reg = ItemChangerHost.Singleton.ContainerRegistry;
        if (reg.GetContainer(ContainerType)?.SupportsAll(true, requestedCapabilities) == true)
        {
            return;
        }

        Locations.ContainerLocation? cl =
            (FalseLocation as Locations.ContainerLocation)
            ?? (TrueLocation as Locations.ContainerLocation);
        if (cl == null)
        {
            return;
        }

        ContainerType = MutablePlacement.ChooseContainerType(this, cl, Items); // container type already failed the initial test
    }

    /// <summary>
    /// Combines placement tags with tags from both potential locations.
    /// </summary>
    public override IEnumerable<Tag> GetPlacementAndLocationTags()
    {
        return base.GetPlacementAndLocationTags()
            .Concat(FalseLocation.Tags ?? Enumerable.Empty<Tag>())
            .Concat(TrueLocation.Tags ?? Enumerable.Empty<Tag>());
    }
}
