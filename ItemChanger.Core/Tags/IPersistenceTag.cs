using ItemChanger.Enums;

namespace ItemChanger.Tags;

/// <summary>
/// Interface used when ItemChanger checks tags to determine whether an item is persistent or semipersistent, and should be refreshed.
/// </summary>
public interface IPersistenceTag
{
    /// <summary>
    /// Declares how the tagged item should persist across save boundaries.
    /// </summary>
    Persistence Persistence { get; }
}
