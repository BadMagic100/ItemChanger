namespace ItemChanger.Tags;

/// <summary>
/// A tag indicating how much the tagged object should be counted for when calculating completion percentage
/// </summary>
public class CompletionWeightTag : Tag
{
    /// <summary>
    /// Relative weight contribution for the tagged object when computing completion.
    /// </summary>
    public required float Weight { get; init; }
}
