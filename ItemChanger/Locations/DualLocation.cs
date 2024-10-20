using System;

namespace ItemChanger.Locations;

/// <summary>
/// Helper location representing a binary choice of locations based on a condition.
/// </summary>
public class DualLocation : AbstractLocation
{
    /// <inheritdoc/>
    protected override void OnLoad()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    protected override void OnUnload()
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
    public required AbstractLocation FalseLocation { get; init; }
    /// <summary>
    /// The location to use when <see cref="Test"/> is <code>true</code>
    /// </summary>
    public required AbstractLocation TrueLocation { get; init; }

    /// <inheritdoc/>
    public override AbstractPlacement Wrap()
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
