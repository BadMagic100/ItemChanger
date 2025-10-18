namespace ItemChanger.Tests;

/// <summary>
/// Collection definition to prevent tests requiring an <see cref="ItemChanger.ItemChangerHost"/> (which is likely most tests)
/// from running in parallel. Such tests classes should have the attribute <code>[Collection(RequiresHostCollection.NAME)]</code>.
/// </summary>
[CollectionDefinition(name: NAME, DisableParallelization = true)]
public class RequiresHostCollection
{
    public const string NAME = "Requires Host";
}
