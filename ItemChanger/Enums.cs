using System;

namespace ItemChanger;

/// <summary>
/// Enum used to communicate compatibility with different UIDef types.
/// </summary>
[Flags]
public enum MessageType
{
    None = 0,
    /// <summary>
    /// A message which shows a sprite and text in the bottom-left corner without taking control.
    /// </summary>
    Corner = 1,
    /// <summary>
    /// A message which takes control and shows a fullscreen popup.
    /// </summary>
    Big = 2,
    /// <summary>
    /// A message which takes control and starts a dialogue prompt.
    /// </summary>
    Lore = 4,
    Any = Corner | Big | Lore,
}

/// <summary>
/// Enum for the current state of an item, to determine whether it has been given and whether it is eligible to be given.
/// </summary>
public enum ObtainState
{
    /// <summary>
    /// The item has never been given, and is eligible to be given.
    /// </summary>
    Unobtained,
    /// <summary>
    /// The item has been given, and is no longer eligible to be given.
    /// </summary>
    Obtained,
    /// <summary>
    /// The item was previously given, but it has been refreshed and is reeligible to be given.
    /// </summary>
    Refreshed
}

[Flags]
public enum VisitState
{
    None = 0,
    ObtainedAnyItem = 1 << 0,
    /// <summary>
    /// Applies to shops, placements with preview dialogues, and placements with hint boxes.
    /// </summary>
    Previewed = 1 << 1,
    /// <summary>
    /// Corresponds to opening a container: e.g. opening a chest, breaking a grub jar or geo rock, etc.
    /// </summary>
    Opened = 1 << 2,
    /// <summary>
    /// Applies to enemy drop items.
    /// </summary>
    Dropped = 1 << 3,
    /// <summary>
    /// Applies to placements offered by NPCs (Cornifer, Nailmasters). Usually set to indicate that the NPC is no longer required to make the offer when items respawn.
    /// </summary>
    Accepted = 1 << 4,
    /// <summary>
    /// Defined on a per-placement basis.
    /// </summary>
    Special = 1 << 31,
}

/// <summary>
/// Enum for controlling respawn behavior of items.
/// </summary>
public enum Persistence
{
    /// <summary>
    /// Indicates the item should not be respawned.
    /// </summary>
    Single,
    /// <summary>
    /// Indicates the item should be respawned when the game resets semipersistent items (on bench, death, and a few world events).
    /// </summary>
    SemiPersistent,
    /// <summary>
    /// Indicates the item should be respawned after any scene load.
    /// </summary>
    Persistent,
}

/// <summary>
/// Enum for controlling how items (particularly geo) should be flung from a location.
/// </summary>
public enum FlingType
{
    /// <summary>
    /// Any fling behavior is acceptable.
    /// </summary>
    Everywhere,
    /// <summary>
    /// Items should not be flung horizontally.
    /// </summary>
    StraightUp,
    /// <summary>
    /// Items should not be flung at all.
    /// </summary>
    DirectDeposit
}

/// <summary>
/// Enum for controlling how a shiny should be flung when activated.
/// </summary>
public enum ShinyFling
{
    /// <summary>
    /// The shiny should fall straight down.
    /// </summary>
    Down,
    /// <summary>
    /// The shiny should be flung to the left.
    /// </summary>
    Left,
    /// <summary>
    /// The shiny should be flung to the right.
    /// </summary>
    Right,
    /// <summary>
    /// The shiny should be flung to the left or right, randomly.
    /// </summary>
    RandomLR,
    None,
}

/// <summary>
/// Enum which provides additional information for serialization and other tag handling purposes.
/// </summary>
[Flags]
public enum TagHandlingFlags
{
    None = 0,
    /// <summary>
    /// If set, and an error occurs when deserializing this object as part of a TaggableObject's tags list, an InvalidTag will be created with the data of this object, and deserialization will continue.
    /// </summary>
    AllowDeserializationFailure = 1,
    /// <summary>
    /// If set, indicates to consumers that this tag should be removed if the current IC data is copied into a new profile.
    /// </summary>
    RemoveOnNewProfile = 2,
}

/// <summary>
/// Enum which provides additional information for serialization and other module handling purposes.
/// </summary>
[Flags]
public enum ModuleHandlingFlags
{
    None = 0,
    /// <summary>
    /// If set, and an error occurs when deserializing this object as part of a ModuleCollection's modules list, an InvalidModule will be created with the data of this object, and deserialization will continue.
    /// </summary>
    AllowDeserializationFailure = 1,
    /// <summary>
    /// If set, indicates to consumers that this module should be removed if the current IC data is copied into a new profile.
    /// </summary>
    RemoveOnNewProfile = 2,
}

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

/// <summary>
/// Enum for adding special behvaior to the respawn marker tied to a StartDef.
/// </summary>
[Flags]
public enum SpecialStartEffects
{
    None = 0,
    DelayedWake = 1,
    SlowSoulRefill = 1 | 1 << 1,
    ExtraInvincibility = 1 << 2,

    Default = DelayedWake | ExtraInvincibility,
}

/// <summary>
/// Enum used to specify an operation for comparing two numbers.
/// </summary>
public enum ComparisonOperator
{
    Eq,
    Neq,
    Lt,
    Le,
    Gt,
    Ge
}
