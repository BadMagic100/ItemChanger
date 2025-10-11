using System;

namespace ItemChanger.Enums;

[Flags]
public enum VisitState
{
    /// <summary>
    /// The placement has not been visited
    /// </summary>
    None = 0,
    /// <summary>
    /// Any item from the placement has been obtained
    /// </summary>
    ObtainedAnyItem = 1 << 0,
    /// <summary>
    /// The content of the placement has been previewed, such as through a local hint box or shop UI.
    /// </summary>
    Previewed = 1 << 1,
    /// <summary>
    /// Corresponds to opening a container, such as opening a chest.
    /// </summary>
    Opened = 1 << 2,
    /// <summary>
    /// Applies to enemy drop items.
    /// </summary>
    Dropped = 1 << 3,
    /// <summary>
    /// Applies to placements offered by NPCs or other mechanisms. 
    /// Usually set to indicate that the placement can respawn the items without prompting the player again.
    /// </summary>
    Accepted = 1 << 4,
    /// <summary>
    /// Defined on a per-placement basis.
    /// </summary>
    Special = 1 << 31,
}
