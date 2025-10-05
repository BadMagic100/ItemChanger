using ItemChanger.Internal;
using ItemChanger.Placements;

namespace ItemChanger.Tags;

/// <summary>
/// Tag which generates a warning message on load if a mutually incompatible placement exists.
/// </summary>
public class IncompatibilityWarningTag : Tag
{
    public required string IncompatiblePlacementName { get; init; }

    protected override void DoLoad(TaggableObject parent)
    {
        string? parentPlacementName = parent switch
        {
            Placement parentPlacement => parentPlacement.Name,
            Location parentLocation => parentLocation.Placement.Name,
            _ => null,
        };

        if (ItemChangerHost.Singleton.ActiveProfile!.TryGetPlacement(IncompatiblePlacementName, out Placement? p))
        {
            LoggerProxy.LogWarn($"Placement {parentPlacementName} is incompatible with {IncompatiblePlacementName}, but both are present.");
        }
    }
}
