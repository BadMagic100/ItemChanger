using ItemChanger.Tags.Constraints;

namespace ItemChanger.Tags;

/// <summary>
/// Indicates the original container type for a location or placement, which will be used
/// if possible if no other preference is present. Also includes various ways that the original
/// container may take precedence over an item's preferred container.
/// </summary>
[LocationTag]
[PlacementTag]
public class OriginalContainerTag : Tag
{
    /// <summary>
    /// The original container type
    /// </summary>
    public required string ContainerType { get; init; }

    /// <summary>
    /// Indicates whether the original container should take precedence over item-specified preferences
    /// during container selection. If the container doesn't have the required capabilities, it will be
    /// slected anyway with a warning.
    /// </summary>
    /// <remarks>
    /// If both Force and <see cref="Priority"/> are <see langword="true"/>, the Force behavior will be used.
    /// </remarks>
    public bool Force { get; init; }

    /// <summary>
    /// Indicates whether the original container should take precedence over item-specified preferences
    /// during container selection. If the container doesn't have the required capabilities, it will be
    /// discarded as a candidate.
    /// </summary>
    /// <remarks>
    /// If both Priority and <see cref="Force"/> are <see langword="true"/>, the <see cref="Force"/> behavior will be used.
    /// </remarks>
    public bool Priority { get; init; }
}
