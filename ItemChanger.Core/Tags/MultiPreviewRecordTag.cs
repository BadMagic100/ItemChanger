using ItemChanger.Tags.Constraints;

namespace ItemChanger.Tags;

/// <summary>
/// Tag which contains preview information for each item of a multi cost placement.
/// </summary>
[PlacementTag]
public class MultiPreviewRecordTag : Tag
{
    public string[] PreviewTexts { get; set; } = [];
}
