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

    private static IEnumerable<Cost> ReduceRedundant(IEnumerable<Cost> costs)
    {
        List<Cost> reduced = [.. costs];
        // remove redundant costs
        for (int i = 0; i < reduced.Count - 1; i++)
        {
            for (int j = i + 1; j < reduced.Count; j++)
            {
                if (reduced[i].Includes(reduced[j]))
                {
                    // j is made redundant by i, so get rid of it
                    reduced.RemoveAt(j);
                    j--;
                }
                else if (reduced[j].Includes(reduced[i]))
                {
                    // i is made redundant by j, bit harder to remove correctly
                    reduced.RemoveAt(i);
                    i--;
                    break;
                }
            }
        }
        return reduced;
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
        this.Costs = [.. ReduceRedundant(Costs.Where(c => c != null).SelectMany(Flatten))];
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
    public override bool Includes(Cost c)
    {
        if (base.Includes(c))
        {
            return true;
        }

        if (c is MultiCost mc)
        {
            // we include a multicost if we include all costs of the multicost
            return mc.Costs.All(Includes);
        }

        return Costs.Any(d => d.Includes(c));
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
