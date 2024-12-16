using ItemChanger.Internal;
using System.Linq;

namespace ItemChanger.Tags;

/// <summary>
/// Tag which generates a warning message on load if a mutually incompatible placement exists.
/// </summary>
public class IncompatibilityWarningTag : Tag
{
    public required string IncompatiblePlacementName { get; init; }

    public override void Load(object parent)
    {
        base.Load(parent);
        string? parentPlacementName = parent switch
        {
            AbstractPlacement parentPlacement => parentPlacement.Name,
            AbstractLocation parentLocation => parentLocation.Placement.Name,
            _ => null,
        };

        if (ItemChangerProfile.ActiveProfile.TryGetPlacement(IncompatiblePlacementName, out AbstractPlacement? p) 
            && p.GetPlacementAndLocationTags().OfType<IncompatibilityWarningTag>().Any(t => t.IncompatiblePlacementName == parentPlacementName))
        {
            LoggerProxy.LogWarn($"Placements {parentPlacementName} and {IncompatiblePlacementName} are marked as incompatible.");
        }
    }
}
