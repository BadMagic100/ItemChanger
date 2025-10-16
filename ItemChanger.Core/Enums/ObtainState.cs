namespace ItemChanger.Enums;

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
    Refreshed,
}
