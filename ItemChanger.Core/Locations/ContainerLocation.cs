using System;
using ItemChanger.Containers;
using ItemChanger.Placements;
using ItemChanger.Tags;
using Newtonsoft.Json;

namespace ItemChanger.Locations;

/// <summary>
/// Location type which supports placing multiple kinds of objects.
/// </summary>
public abstract class ContainerLocation : Location
{
    /// <summary>
    /// Whether to force a default single-item container at the location.
    /// </summary>
    public bool ForceDefaultContainer { get; init; }

    /// <summary>
    /// The original container type associated with this
    /// </summary>
    [JsonIgnore]
    public string? OriginalContainerType => GetTag<OriginalContainerTag>()?.ContainerType;

    public void GetContainer(out Container container, out ContainerInfo info)
    {
        if (Placement is not IContainerPlacement cp)
        {
            throw new InvalidOperationException(
                $"Cannot get container for {nameof(ContainerLocation)} {Name} because the placement {Placement?.Name} is not an {nameof(IContainerPlacement)}"
            );
        }
        cp.GetContainer(this, out container, out info);
    }

    public virtual bool Supports(string containerType)
    {
        return containerType
                == ItemChangerHost.Singleton.ContainerRegistry.DefaultSingleItemContainer.Name
            || !ForceDefaultContainer;
    }

    public override Placement Wrap()
    {
        return new MutablePlacement(Name)
        {
            Location = this,
            Cost = ImplicitCostTag.GetDefaultCost(this),
        };
    }
}
