namespace ItemChanger.Locations;

/// <summary>
/// A location with no effects. Use, for example, with DualPlacement, or in other situations where a dummy location may be needed.
/// </summary>
public class EmptyLocation : AutoLocation
{
    /// <inheritdoc/>
    protected override void DoLoad() { }

    /// <inheritdoc/>
    protected override void DoUnload() { }
}
