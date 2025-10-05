using ItemChanger.Internal;
using ItemChanger.Placements;
using UnityEngine;

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

    public void GetContainer(out GameObject obj, out string containerType)
    {
        ((IContainerPlacement)Placement).GetContainer(this, out obj, out containerType);
    }

    public virtual bool Supports(string containerType)
    {
        return containerType == ItemChangerHost.Singleton.ContainerRegistry.DefaultSingleItemContainer.Name
            ? true
            : !ForceDefaultContainer;
    }

    public override Placement Wrap()
    {
        return new MutablePlacement(Name)
        {
            Location = this,
        };
    }
}
