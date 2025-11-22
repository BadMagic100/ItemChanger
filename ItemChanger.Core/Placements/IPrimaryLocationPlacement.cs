using ItemChanger.Locations;

namespace ItemChanger.Placements;

/// <summary>
/// Interface for accessing the primary location of a placement, if it has one.
/// </summary>
public interface IPrimaryLocationPlacement
{
    /// <summary>
    /// Gets the primary location that the placement represents.
    /// </summary>
    public Location Location { get; }
}
