﻿using System;

namespace ItemChanger.Tags;

/// <summary>
/// Tag which triggers a recursive search through the AbstractItem.ModifyItem hook.
/// <br />Recursion is by looking up the predecessor and successor items in Finder, and basing a search at their ItemChainTags.
/// <br />Selected item is first nonredundant item in the sequence, or null (handled by AbstractItem) if all items are redundant.
/// </summary>
[ItemTag]
public class ItemChainTag : Tag
{
    /// <summary>
    /// The previous item in the item chain
    /// </summary>
    public string? Predecessor { get; set; }
    /// <summary>
    /// The subsequent item in the item chain
    /// </summary>
    public string? Successor { get; set; }

    public override void Load(object parent)
    {
        base.Load(parent);
        AbstractItem item = (AbstractItem)parent;
        item.ModifyItem += ModifyItem;
    }

    public override void Unload(object parent)
    {
        base.Unload(parent);
        AbstractItem item = (AbstractItem)parent;
        item.ModifyItem -= ModifyItem;
    }


    protected virtual AbstractItem GetItem(string name)
    {
        return Finder.GetItem(name) ?? throw new NullReferenceException("Could not find item " + name);
    }

    private void ModifyItem(GiveEventArgs args)
    {
        if (args.Item is null)
        {
            return;
        }

        if (args.Item.Redundant())
        {
            while (args.Item is not null && args.Item.GetTag<ItemChainTag>()?.Successor is string succ && !string.IsNullOrEmpty(succ))
            {
                args.Item = GetItem(succ);
                if (!args.Item.Redundant())
                {
                    return;
                }
            }

            args.Item = null;
            return;
        }
        else
        {
            while (args.Item?.GetTag<ItemChainTag>()?.Predecessor is string pred && !string.IsNullOrEmpty(pred))
            {
                AbstractItem item = GetItem(pred);
                if (item.Redundant())
                {
                    return;
                }
                else
                {
                    args.Item = item;
                }
            }
            return;
        }
    }
}
