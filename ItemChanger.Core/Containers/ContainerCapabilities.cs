namespace ItemChanger.Containers;

/// <summary>
/// Constants for the ItemChanger-defined container capabilities. The lower-order 8 bits are reserved.
/// </summary>
public static class ContainerCapabilities
{
    /// <summary>
    /// Describes a container with no particular capabilities.
    /// </summary>
    public const uint None = 0;

    /// <summary>
    /// Describes a container which can offer the player an option to pay a cost.
    /// </summary>
    public const uint PayCosts = 1 << 0;
}
