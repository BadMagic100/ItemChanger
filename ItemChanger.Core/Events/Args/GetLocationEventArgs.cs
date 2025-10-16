using System;
using ItemChanger.Locations;

namespace ItemChanger.Events.Args;

public class GetLocationEventArgs(string locationName) : EventArgs
{
    public string LocationName => locationName;
    public Location? Current { get; set; }
}
