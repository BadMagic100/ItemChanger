using System;
using ItemChanger.Enums;
using UnityEngine;

namespace ItemChanger;

/// <summary>
/// Abstractly represents the way an item is to be displayed in various parts of the user interface.
/// </summary>
public abstract class UIDef : IFinderCloneable
{
    /// <summary>
    /// Emits the UIDef to the game.
    /// </summary>
    /// <param name="type">The message type.</param>
    /// <param name="callback">A callback to be invoked after sending the message.</param>
    public abstract void SendMessage(MessageType type, Action? callback = null);

    /// <summary>
    /// Get the displayed name of the item after it is obtained.
    /// </summary>
    public abstract string GetPostviewName();

    /// <summary>
    /// Get the displayed name of the item before it is obtained.
    /// Defaults to <see cref="GetPostviewName"/>
    /// </summary>
    public virtual string GetPreviewName()
    {
        return GetPostviewName();
    }

    /// <summary>
    /// Gets the long description of the item.
    /// </summary>
    /// <remarks>
    /// The long description is not guaranteed to be shown depending on where the item is placed.
    /// For example, the long description may be used as text in a shop inventory.
    /// </remarks>
    public abstract string? GetLongDescription();

    /// <summary>
    /// Gets the sprite for the item
    /// </summary>
    public abstract Sprite GetSprite();
}
