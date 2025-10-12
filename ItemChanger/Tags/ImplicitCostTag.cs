using ItemChanger.Costs;
using ItemChanger.Tags.Constraints;

namespace ItemChanger.Tags;

/// <summary>
/// A tag which does not modify behavior, but provides information about the implicit costs of a placement or location.
/// </summary>
[LocationTag]
[PlacementTag]
public class ImplicitCostTag : Tag
{
    public required Cost Cost { get; init; }

    /// <summary>
    /// An inherent cost always applies. A non-inherent cost applies as a substitute when the placement does not have a (non-null) cost.
    /// </summary>
    public bool Inherent { get; init; }

    protected override void DoLoad(TaggableObject parent)
    {
        Cost.LoadOnce();
    }

    protected override void DoUnload(TaggableObject parent)
    {
        Cost.UnloadOnce();
    }
}
