using ItemChanger.Items;
using ItemChanger.Tags.Constraints;
using System;
using System.Linq;

namespace ItemChanger.Tags;

/// <summary>
/// Tag which carries ordered lists of predecessors and successors item names.
/// <br/>Hooks AbstractItem.ModifyItem, and returns the first item in the sequence which is not redundant.
/// <br/> Note that unlike ItemChainTag, this does not recursively check tags of the predecessors and successors.
/// </summary>
[ItemTag]
public class ItemTreeTag : Tag
{
    public string[]? Predecessors { get; set; }
    public string[]? Successors { get; set; }
    /// <summary>
    /// If true, the first nonredundant item starting from the first element in the list will be chosen.
    /// <br/>Otherwise, the search will begin at the parent item, and will assume that predecessors of a redundant item are redundant.
    /// <br/>Only relevant when predecessors is nonempty.
    /// </summary>
    public bool strictEvaluation;

    protected override void DoLoad(TaggableObject parent)
    {
        Item item = (Item)parent;
        item.ModifyItem += ModifyItem;
    }

    protected override void DoUnload(TaggableObject parent)
    {
        base.DoUnload(parent);
        Item item = (Item)parent;
        item.ModifyItem -= ModifyItem;
    }


    protected virtual Item GetItem(string name)
    {
        return Finder.GetItem(name) ?? throw new NullReferenceException("Could not find item " + name);
    }


    public void ModifyItem(GiveEventArgs args)
    {
        if (args.Item is null)
        {
            return;
        }

        if (strictEvaluation)
        {
            if (Predecessors != null)
            {
                foreach (string s in Predecessors)
                {
                    Item item = GetItem(s);
                    if (!item.Redundant())
                    {
                        args.Item = item;
                        return;
                    }
                }
            }
            if (!args.Item.Redundant())
            {
                return;
            }
            if (Successors != null)
            {
                foreach (string s in Successors)
                {
                    Item item = GetItem(s);
                    if (!item.Redundant())
                    {
                        args.Item = item;
                        return;
                    }
                }
            }
            // all redundant
            args.Item = null;
            return;
        }
        else
        {
            if (args.Item.Redundant())
            {
                if (Successors != null)
                {
                    foreach (string s in Successors)
                    {
                        Item item = GetItem(s);
                        if (!item.Redundant())
                        {
                            args.Item = item;
                            return;
                        }
                    }
                }
                // all redundant
                args.Item = null;
                return;
            }
            else
            {
                if (Predecessors != null)
                {
                    foreach (string s in Predecessors.Reverse())
                    {
                        Item item = GetItem(s);
                        if (!item.Redundant())
                        {
                            args.Item = item;
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                // none redundant
                return;
            }
        }
    }
}
