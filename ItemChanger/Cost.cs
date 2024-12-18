﻿using ItemChanger.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ItemChanger;

/// <summary>
/// Data type used generally for cost handling, including in shops and y/n dialogue prompts.
/// </summary>
public abstract record Cost
{
    /// <summary>
    /// Returns whether the cost can currently be paid.
    /// </summary>
    public abstract bool CanPay();

    /// <summary>
    /// Pays the cost, performing any effects and setting the cost to Paid.
    /// </summary>
    public void Pay()
    {
        OnPay();
        if (!Recurring)
        {
            Paid = true;
        }

        AfterPay();
    }

    /// <summary>
    /// Method for administering all effects of the cost during Pay.
    /// </summary>
    public abstract void OnPay();

    /// <summary>
    /// Method for any effects which should take place after the cost has been paid (e.g. conditionally setting Paid, etc).
    /// </summary>
    public virtual void AfterPay() { }

    /// <summary>
    /// Represents whether the cost has been paid yet. Paid costs will be subsequently ignored.
    /// </summary>
    public bool Paid { get; set; }

    /// <summary>
    /// If true, the cost will not set the value of Paid during Pay. Use for costs which are expected to be paid multiple times.
    /// <br/>Note that Paid can still be set independently to indicate when the cost should no longer be required.
    /// </summary>
    public virtual bool Recurring { get; set; }

    /// <summary>
    /// A number between 0 and 1 which modifies numeric costs. Only considered by some costs.
    /// <br/>For example, the Leg Eater dung discount sets this to 0.8, to indicate that geo costs should be at 80% price.
    /// </summary>
    public virtual float DiscountRate { get; set; } = 1.0f;

    /// <summary>
    /// Method which provides the cost text used in y/n prompts.
    /// </summary>
    public abstract string GetCostText();

    /// <summary>
    /// Points to the root-level cost for pattern-matching contexts such as CostDisplayer. Primarily intended 
    /// for implementation by costs which wrap a single other cost to apply additional functionality.
    /// </summary>
    /// <remarks>
    /// Implementers of wrapper costs should keep in mind that costs being wrapped may themselves be wrapper costs.
    /// A typical correct implementation would likely be `WrappedCost.GetBaseCost()`.
    /// </remarks>
    public virtual Cost GetBaseCost() => this;

    /// <summary>
    /// Is the other cost a subset of this cost?
    /// </summary>
    public virtual bool Includes(Cost c) =>  c is null || Equals(c);

    /// <summary>
    /// Does paying this cost have effects (particularly that could prevent paying other costs of the same type)?
    /// </summary>
    public abstract bool HasPayEffects();
    /// <summary>
    /// Method which should be called by the Cost's owner during initial loading. Used by certain costs which require global or shared tracking.
    /// </summary>
    public virtual void Load() { }
    /// <summary>
    /// Method which should be called by the Cost's owner during unloading. Used by certain costs which require global or shared tracking.
    /// </summary>
    public virtual void Unload() { }

    /// <summary>
    /// Combines two costs into a MultiCost. If either argument is null, returns the other argument.  If one or both costs is a MultiCost, flattens the result.
    /// </summary>
    public static Cost operator +(Cost a, Cost b)
    {
        if (a == null)
        {
            return b;
        }

        if (b == null)
        {
            return a;
        }

        return new MultiCost(a, b);
    }
}

/// <summary>
/// Cost which is the concatenation of other costs. Can only be paid if all of its costs can be paid, and pays all its costs sequentially.
/// </summary>
[JsonObject]
public sealed record MultiCost : Cost, IReadOnlyList<Cost>
{
    [JsonProperty]
    private readonly Cost[] Costs;

    public int Count => Costs.Length;

    public Cost this[int index] { get => Costs[index]; }

    private static IEnumerable<Cost> Flatten(Cost c)
    {
        if (c is MultiCost mc)
        {
            return mc.Costs;
        }
        return c.Yield();
    }

    public MultiCost()
    {
        Costs = Array.Empty<Cost>();
    }

    [JsonConstructor]
    public MultiCost(IEnumerable<Cost> Costs)
    {
        this.Costs = Costs
            .Where(c => c != null)
            .SelectMany(Flatten)
            .ToArray();
    }

    public MultiCost(params Cost[] Costs) : this((IEnumerable<Cost>)Costs) { }

    public override bool CanPay()
    {
        return Costs.All(c => c.Paid || c.CanPay());
    }

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

    public override string GetCostText()
    {
        return string.Join(", ", Costs.Select(c => c.GetCostText()).ToArray());
    }

    public override bool Includes(Cost c)
    {
        if (c is MultiCost mc)
        {
            return mc.Costs.All(d => Includes(d));
        }

        return Costs.Any(d => d.Includes(c));
    }

    public override bool HasPayEffects()
    {
        return Costs.Any(d => d.HasPayEffects());
    }

    public override void Load()
    {
        foreach (Cost c in Costs)
        {
            c.Load();
        }
    }

    public override void Unload()
    {
        foreach (Cost c in Costs)
        {
            c.Unload();
        }
    }

    public int IndexOf(Cost item) => Costs.IndexOf(item);

    public bool Contains(Cost item) => Costs.Contains(item);

    public void CopyTo(Cost[] array, int arrayIndex) => Costs.CopyTo(array, arrayIndex);

    public IEnumerator<Cost> GetEnumerator() => Costs.OfType<Cost>().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Costs.GetEnumerator();
}
