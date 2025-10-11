using ItemChanger.Placements;
using ItemChanger.Serialization;
using System;

namespace ItemChanger.Locations;

/// <summary>
/// Helper location representing a binary choice of locations based on a condition.
/// </summary>
public class DualLocation : Location
{
    /// <inheritdoc/>
    protected override void DoLoad()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    protected override void DoUnload()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// A test to determine which location to use
    /// </summary>
    public required IBool Test;
    /// <summary>
    /// The location to use when <see cref="Test"/> is <code>false</code>
    /// </summary>
    public required Location FalseLocation { get; init; }
    /// <summary>
    /// The location to use when <see cref="Test"/> is <code>true</code>
    /// </summary>
    public required Location TrueLocation { get; init; }

    /// <inheritdoc/>
    public override Placement Wrap()
    {
        return new Placements.DualPlacement(Name)
        {
            Test = Test,
            FalseLocation = FalseLocation,
            TrueLocation = TrueLocation,
            tags = tags,
        };
    }
}
