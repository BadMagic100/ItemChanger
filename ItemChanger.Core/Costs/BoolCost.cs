using ItemChanger.Serialization;

namespace ItemChanger.Costs;

/// <summary>
/// A boolean cost requiring the source boolean to be true and has no pay effect.
/// </summary>
public abstract record ThresholdBoolCost : Cost
{
    /// <summary>
    /// The value to use to evaluate whether the cost is payable
    /// </summary>
    protected abstract IBool GetValueSource();

    /// <inheritdoc/>
    public override bool CanPay() => GetValueSource().Value;

    /// <inheritdoc/>
    public override bool HasPayEffects() => false;

    /// <inheritdoc/>
    public override void OnPay() { }
}

/// <summary>
/// A boolean cost requiring the source boolean to be true and sets the boolean to false when the cost is paid.
/// </summary>
public abstract record ConsumableBoolCost : Cost
{
    /// <summary>
    /// The value to use to evaluate and pay the cost
    /// </summary>
    protected abstract IWritableBool GetValueSource();

    /// <inheritdoc/>
    public override bool CanPay() => GetValueSource().Value;

    /// <inheritdoc/>
    public override bool HasPayEffects() => true;

    /// <inheritdoc/>
    public override void OnPay() => GetValueSource().Value = false;
}
