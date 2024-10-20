namespace ItemChanger.Tags;

/// <summary>
/// A tag indicating how much the tagged object should be counted for when calculating completion percentage
/// </summary>
public class CompletionWeightTag : Tag
{
    public float Weight { get; set; } = 1;
}
