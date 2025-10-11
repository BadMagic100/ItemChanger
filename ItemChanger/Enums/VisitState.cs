using System;

namespace ItemChanger.Enums;

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
