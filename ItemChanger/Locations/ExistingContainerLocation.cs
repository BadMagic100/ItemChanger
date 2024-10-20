using Newtonsoft.Json;

namespace ItemChanger.Locations;

/// <summary>
/// An abstract location which can be optionally replaced with a container, but has a natural way of giving items if not replaced.
/// </summary>
public abstract class ExistingContainerLocation : AbstractLocation
{
    /// <summary>
    /// The type of container which already exists at this location
    /// </summary>
    public required string ContainerType { get; init; }
    /// <summary>
    /// Whether to prevent replacement of the container
    /// </summary>
    public required bool NonReplaceable { get; init; }

    public virtual bool WillBeReplaced()
    {
        return !(Placement.MainContainerType == ContainerType || NonReplaceable || Container.GetContainer(Placement.MainContainerType) is not Container c || !c.SupportsInstantiate);
    }

    /// <summary>
    /// Returns true if the placement does not need to choose a container that supports costs because the location will manage the cost separately.
    /// </summary>
    [JsonIgnore] public virtual bool HandlesCostBeforeContainer => false;

    public override AbstractPlacement Wrap()
    {
        return new Placements.ExistingContainerPlacement(Name)
        {
            Location = this,
        };
    }
}
