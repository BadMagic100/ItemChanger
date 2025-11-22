using System;
using ItemChanger.Locations;

namespace ItemChanger.Events.Args;

/// <summary>
/// Event arguments used when listeners can provide or override a location lookup.
/// </summary>
public class GetLocationEventArgs(string locationName) : EventArgs
{
    /// <summary>
    /// Name requested by the finder.
    /// </summary>
    public string LocationName => locationName;

    /// <summary>
    /// Replacement location supplied by a listener, if any.
    /// </summary>
    public Location? Current { get; set; }
}
