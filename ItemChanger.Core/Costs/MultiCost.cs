using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ItemChanger.Extensions;
using Newtonsoft.Json;

namespace ItemChanger.Costs;

/// <summary>
/// Cost which is the concatenation of other costs. Can only be paid if all of its costs can be paid, and pays all its costs sequentially.
/// </summary>
[JsonObject]
public sealed class MultiCost : Cost, IReadOnlyList<Cost>
{
    [JsonProperty]
    private readonly Cost[] Costs;

    /// <inheritdoc/>
    public int Count => Costs.Length;

    /// <inheritdoc/>
    public Cost this[int index]
    {
        get => Costs[index];
    }

    private static IEnumerable<Cost> Flatten(Cost c)
    {
        if (c is MultiCost mc)
        {
            return mc.Costs;
        }
        return c.Yield();
    }

    /// <summary>
    /// Constructs an empty MultiCost.
    /// </summary>
    public MultiCost()
    {
        Costs = Array.Empty<Cost>();
    }

    /// <summary>
    /// Constructs a MultiCost of the provided costs, flattening nested MultiCosts.
    /// </summary>
    /// <param name="costs">The costs to include</param>
    [JsonConstructor]
    public MultiCost(IEnumerable<Cost> costs)
    {
        this.Costs = [.. costs.Where(c => c != null).SelectMany(Flatten)];
    }

    /// <summary>
    /// Constructs a MultiCost of the provided costs, flattening nested MultiCosts.
    /// </summary>
    /// <param name="Costs">The costs to include</param>
    public MultiCost(params Cost[] Costs)
        : this((IEnumerable<Cost>)Costs) { }

    /// <inheritdoc/>
    public override bool CanPay()
    {
        return Costs.All(c => c.Paid || c.CanPay());
    }

    /// <inheritdoc/>
    public override void OnPay()
    {
        foreach (Cost c in Costs)
        {
            if (!c.Paid)
            {
                c.Pay();
            }
        }
    }

    /// <inheritdoc/>
    public override float DiscountRate
    {
        get => base.DiscountRate;
        set
        {
            base.DiscountRate = value;
            foreach (Cost c in Costs)
            {
                c.DiscountRate = value;
            }
        }
    }

    /// <inheritdoc/>
    public override bool IsFree => Costs.All(c => c.IsFree);

    /// <inheritdoc/>
    public override string GetCostText()
    {
        return string.Join(", ", Costs.Select(c => c.GetCostText()).ToArray());
    }

    /// <inheritdoc/>
    public override bool HasPayEffects()
    {
        return Costs.Any(d => d.HasPayEffects());
    }

    /// <inheritdoc/>
    protected override void DoLoad()
    {
        foreach (Cost c in Costs)
        {
            c.LoadOnce();
        }
    }

    /// <inheritdoc/>
    protected override void DoUnload()
    {
        foreach (Cost c in Costs)
        {
            c.UnloadOnce();
        }
    }

    /// <inheritdoc/>
    public int IndexOf(Cost item) => Costs.IndexOf(item);

    /// <inheritdoc/>
    public bool Contains(Cost item) => Costs.Contains(item);

    /// <inheritdoc/>
    public void CopyTo(Cost[] array, int arrayIndex) => Costs.CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    public IEnumerator<Cost> GetEnumerator() => Costs.OfType<Cost>().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Costs.GetEnumerator();
}
