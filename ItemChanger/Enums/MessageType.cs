using System;

namespace ItemChanger.Enums;

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
