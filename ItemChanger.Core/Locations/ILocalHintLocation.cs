namespace ItemChanger.Locations;

/// <summary>
/// Interface for locations which support a nearby toggleable item preview.
/// </summary>
public interface ILocalHintLocation
{
    /// <summary>
    /// Gets or sets whether the hint is currently active.
    /// </summary>
    bool HintActive { get; set; }
}

/// <summary>
/// Helpers for working with <see cref="ILocalHintLocation"/> types.
/// </summary>
public static class LocalHintLocationExtensions
{
    /// <summary>
    /// Returns true if the hint is active and not overridden by a disable tag.
    /// </summary>
    public static bool GetItemHintActive(this ILocalHintLocation ilhl)
    {
        if (ilhl is Location loc && loc.Placement?.HasTag<Tags.DisableItemPreviewTag>() == true)
        {
            return false;
        }

        return ilhl.HintActive;
    }
}
