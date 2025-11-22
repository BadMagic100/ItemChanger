using System;
using ItemChanger.Events.Args;
using ItemChanger.Items;
using ItemChanger.Tags.Constraints;

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
    public string? Predecessor { get; init; }

    /// <summary>
    /// The subsequent item in the item chain
    /// </summary>
    public string? Successor { get; init; }

    /// <inheritdoc/>
    protected override void DoLoad(TaggableObject parent)
    {
        Item item = (Item)parent;
        item.ModifyItem += ModifyItem;
    }

    /// <inheritdoc/>
    protected override void DoUnload(TaggableObject parent)
    {
        Item item = (Item)parent;
        item.ModifyItem -= ModifyItem;
    }

    /// <summary>
    /// Retrieves an item by name. Default implementation queries the global <see cref="Finder"/>, but subclasses can resolve names differently.
    /// </summary>
    protected virtual Item GetItem(string name)
    {
        return ItemChangerHost.Singleton.Finder.GetItem(name) ?? throw new ArgumentException(
                "Could not find item " + name,
                nameof(name)
            );
    }

    private void ModifyItem(GiveEventArgs args)
    {
        if (args.Item is null)
        {
            return;
        }

        if (args.Item.Redundant())
        {
            while (
                args.Item is not null
                && args.Item.GetTag<ItemChainTag>()?.Successor is string succ
                && !string.IsNullOrEmpty(succ)
            )
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
            while (
                args.Item?.GetTag<ItemChainTag>()?.Predecessor is string pred
                && !string.IsNullOrEmpty(pred)
            )
            {
                Item item = GetItem(pred);
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
