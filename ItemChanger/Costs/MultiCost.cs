using ItemChanger.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ItemChanger.Costs;

/// <summary>
/// Cost which is the concatenation of other costs. Can only be paid if all of its costs can be paid, and pays all its costs sequentially.
/// </summary>
[JsonObject]
public sealed record MultiCost : Cost, IReadOnlyList<Cost>
{
    [JsonProperty]
    private readonly Cost[] Costs;

    /// <inheritdoc/>
    public int Count => Costs.Length;

    /// <inheritdoc/>
    public Cost this[int index] { get => Costs[index]; }

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
    /// <param name="Costs">The costs to include</param>
    [JsonConstructor]
    public MultiCost(IEnumerable<Cost> Costs)
    {
        this.Costs = Costs
            .Where(c => c != null)
            .SelectMany(Flatten)
            .ToArray();
    }

    /// <summary>
    /// Constructs a MultiCost of the provided costs, flattening nested MultiCosts.
    /// </summary>
    /// <param name="Costs">The costs to include</param>
    public MultiCost(params Cost[] Costs) : this((IEnumerable<Cost>)Costs) { }

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
    public override string GetCostText()
    {
        return string.Join(", ", Costs.Select(c => c.GetCostText()).ToArray());
    }

    /// <inheritdoc/>
    public override bool Includes(Cost c)
    {
        if (c is MultiCost mc)
        {
            return mc.Costs.All(d => Includes(d));
        }

        return Costs.Any(d => d.Includes(c));
    }

    /// <inheritdoc/>
    public override bool HasPayEffects()
    {
        return Costs.Any(d => d.HasPayEffects());
    }

    /// <inheritdoc/>
    public override void Load()
    {
        foreach (Cost c in Costs)
        {
            c.Load();
        }
    }

    /// <inheritdoc/>
    public override void Unload()
    {
        foreach (Cost c in Costs)
        {
            c.Unload();
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
