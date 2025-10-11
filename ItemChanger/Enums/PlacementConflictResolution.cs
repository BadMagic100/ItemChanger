namespace ItemChanger.Enums;

/// <summary>
/// Enum for controlling what should happen when a placement is added, and another placement with the same name already exists in settings.
/// </summary>
public enum PlacementConflictResolution
{
    /// <summary>
    /// Keep new placement, discard old placement, and append items of old placement to new placement.
    /// </summary>
    MergeKeepingNew,
    /// <summary>
    /// Keep old placement, discard new placement, and append items of new placement to old placement.
    /// </summary>
    MergeKeepingOld,
    /// <summary>
    /// Keep new placement, discard old placement
    /// </summary>
    Replace,
    /// <summary>
    /// Keep old placement, discard new placement
    /// </summary>
    Ignore,
    /// <summary>
    /// A duplicate placement will result in an ArgumentException.
    /// </summary>
    Throw
}
