using ItemChanger.Tags.Constraints;

namespace ItemChanger.Tags;

/// <summary>
/// Tag for a location or placement to indicate that a container is not supported and should not be chosen.
/// </summary>
[LocationTag]
[PlacementTag]
public class UnsupportedContainerTag : Tag
{
    /// <summary>
    /// Container type that the associated location or placement does not support.
    /// </summary>
    public required string ContainerType { get; init; }
}
