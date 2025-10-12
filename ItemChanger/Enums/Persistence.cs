namespace ItemChanger.Enums;

/// <summary>
/// Enum for controlling respawn behavior of items.
/// </summary>
public enum Persistence
{
    /// <summary>
    /// Indicates the item should not be respawned.
    /// </summary>
    NonPersistent,

    /// <summary>
    /// Indicates the item should be respawned when the game resets semipersistent items (triggered by specific game events).
    /// </summary>
    SemiPersistent,

    /// <summary>
    /// Indicates the item should be respawned after any scene load.
    /// </summary>
    Persistent,
}
