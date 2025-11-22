using ItemChanger.Costs;

namespace ItemChanger.Tags;

/// <summary>
/// Tag used for carrying information about costs, particularly for items in a shop or other multicost placement.
/// </summary>
public class CostTag : Tag
{
    /// <summary>
    /// Cost associated with the tagged object.
    /// </summary>
    public required Cost Cost { get; init; }

    /// <summary>
    /// Ensures the cost is loaded alongside the parent.
    /// </summary>
    protected override void DoLoad(TaggableObject parent)
    {
        Cost?.LoadOnce();
    }

    /// <summary>
    /// Unloads the cost when the parent unloads.
    /// </summary>
    protected override void DoUnload(TaggableObject parent)
    {
        Cost?.UnloadOnce();
    }
}
