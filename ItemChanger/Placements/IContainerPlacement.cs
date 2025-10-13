using ItemChanger.Containers;
using ItemChanger.Locations;

namespace ItemChanger.Placements;

/// <summary>
/// Interace for placements which can be used by ContainerLocation. In other words, on demand the placement returns an object which is capable of giving its items.
/// </summary>
public interface IContainerPlacement
{
    public void GetContainer(Location location, out Container container, out ContainerInfo info);
}
