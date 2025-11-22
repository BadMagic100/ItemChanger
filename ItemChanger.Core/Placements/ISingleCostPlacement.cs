using ItemChanger.Costs;

namespace ItemChanger.Placements;

/// <summary>
/// Interface which indicates that placement expects all items to share a common cost.
/// </summary>
public interface ISingleCostPlacement
{
    /// <summary>
    /// Gets or sets the cost shared across the placement's items.
    /// </summary>
    Cost? Cost { get; set; }
}
