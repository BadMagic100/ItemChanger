namespace ItemChanger.Tags;

/// <summary>
/// Tag which indicates an item has a fixed persistence.
/// </summary>
[ItemTag]
public class PersistentItemTag : Tag, IPersistenceTag
{
    public required Persistence Persistence { get; set; }
}
