namespace ItemChanger.Items;

/// <summary>
/// Placeholder item that performs no action when given.
/// </summary>
public class NullItem : Item
{
    /// <summary>
    /// Creates a new null item instance with a default name.
    /// </summary>
    public static NullItem Create()
    {
        return new NullItem { Name = "Nothing" };
    }

    /// <inheritdoc/>
    public override void GiveImmediate(GiveInfo info)
    {
        // intentional no-op
    }
}
