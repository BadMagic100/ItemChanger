using ItemChanger.Costs;

namespace ItemChanger.Tags;

/// <summary>
/// Tag used for carrying information about costs, particularly for items in a shop or other multicost placement.
/// </summary>
public class CostTag : Tag
{
    public required Cost Cost { get; init; }

    protected override void DoLoad(TaggableObject parent)
    {
        Cost?.LoadOnce();
    }

    protected override void DoUnload(TaggableObject parent)
    {
        Cost?.UnloadOnce();
    }
}
