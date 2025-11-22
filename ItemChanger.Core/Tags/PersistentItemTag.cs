using ItemChanger.Enums;
using ItemChanger.Tags.Constraints;

namespace ItemChanger.Tags;

/// <summary>
/// Tag which indicates an item has a fixed persistence.
/// </summary>
[ItemTag]
public class PersistentItemTag : Tag, IPersistenceTag
{
    /// <summary>
    /// Persistence level to apply to the tagged item.
    /// </summary>
    public required Persistence Persistence { get; set; }
}
