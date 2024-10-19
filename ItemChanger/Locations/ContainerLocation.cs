using ItemChanger.Placements;

namespace ItemChanger.Locations;

/// <summary>
/// Location type which supports placing multiple kinds of objects.
/// </summary>
public abstract class ContainerLocation : AbstractLocation
{
    /// <summary>
    /// Whether to force a shiny at the location.
    /// </summary>
    public bool ForceShiny { get; init; }

    public void GetContainer(out GameObject obj, out string containerType)
    {
        ((IContainerPlacement)Placement).GetContainer(this, out obj, out containerType);
    }

    public virtual bool Supports(string containerType)
    {
        return containerType == Container.Shiny ? true : !ForceShiny;
    }

    public override AbstractPlacement Wrap()
    {
        return new MutablePlacement(name)
        {
            Location = this,
        };
    }
}
