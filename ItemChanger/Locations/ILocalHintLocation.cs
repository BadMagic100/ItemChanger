namespace ItemChanger.Locations;

/// <summary>
/// Interface for locations which support a nearby toggleable item preview.
/// </summary>
public interface ILocalHintLocation
{
    bool HintActive { get; set; }
}

public static class LocalHintLocationExtensions
{
    public static bool GetItemHintActive(this ILocalHintLocation ilhl)
    {
        if (ilhl is Location loc && loc.Placement?.HasTag<Tags.DisableItemPreviewTag>() == true)
        {
            return false;
        }

        return ilhl.HintActive;
    }
}
