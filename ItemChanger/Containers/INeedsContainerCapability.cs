namespace ItemChanger.Containers;

/// <summary>
/// An interface to be implemented by placement and location tags in order to request specific capabilities during container selection.
/// </summary>
public interface INeedsContainerCapability
{
    /// <summary>
    /// A bitfield of requested capabilities, which can be ItemChanger- or host-defined.
    /// The lower order 8 bits are reserved by ItemChanger in <see cref="ContainerCapabilities"/>
    /// </summary>
    public uint RequestedCapabilities { get; }
}
