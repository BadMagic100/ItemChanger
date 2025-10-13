using System;
using System.Collections.Generic;
using System.Linq;
using ItemChanger.Events.Args;
using ItemChanger.Items;
using ItemChanger.Tags;

namespace ItemChanger.Modules;

/// <summary>
/// Module associated with <see cref="ProgressiveItemGroupTag"/>.
/// Carries data on the history of given items for the group, and handles the item modification hook.
/// </summary>
public class ProgressiveItemGroupModule : Module
{
    /// <summary>
    /// An id which matches the id on each <see cref="ProgressiveItemGroupTag"/> for the group.
    /// </summary>
    public required string GroupID { get; init; }

    /// <summary>
    /// A list of unique members of the group. The order is used to determine the order in which collected items are resolved to actual items.
    /// </summary>
    public required List<string> OrderedMemberList { get; init; }

    /// <summary>
    /// A lookup of the group's predecessor partial ordering populated by <see cref="RegisterItem(ProgressiveItemGroupTag, Item)"/>.
    /// </summary>
    public required Dictionary<
        string,
        List<string>
    > OrderedTransitivePredecessorsLookup { get; init; }

    /// <summary>
    /// The list of items associated to the group which have been collected, prior to replacement. Includes duplicates with multiplicity.
    /// </summary>
    public List<string> CollectedItemList { get; } = [];

    private readonly List<Item> registeredItems = [];

    /// <inheritdoc/>
    protected override void DoLoad()
    {
        CheckAllItemsCompletelyDefined();
        CheckTransitive();
        CheckIrreflexive();
        CheckOrderConsistency();
    }

    /// <inheritdoc/>
    protected override void DoUnload()
    {
        foreach (Item i in registeredItems)
        {
            i.ModifyItem -= ModifyItem;
        }

        registeredItems.Clear();
    }

    /// <summary>
    /// Retrieves the item by name, by default using <see cref="Finder.GetItem(string)"/>.
    /// </summary>
    protected virtual Item GetItem(string name) =>
        ItemChangerHost.Singleton.Finder.GetItem(name) ?? throw new KeyNotFoundException(
            $"Failed to find item {name} in Finder."
        );

    /// <summary>
    /// Records the tag's data and its item to be managed by the module.
    /// </summary>>
    /// <exception cref="InvalidOperationException">The item was not found in the <see cref="OrderedMemberList"/>.</exception>
    public void RegisterItem(ProgressiveItemGroupTag tag, Item item)
    {
        if (!OrderedMemberList.Contains(item.Name))
        {
            throw UnexpectedMember(item.Name);
        }

        item.ModifyItem += ModifyItem;
        registeredItems.Add(item);
    }

    /// <summary>
    /// Modifies the given item according to the output of <see cref="GetActualItems(IEnumerable{string}, List{string}, Dictionary{string, List{string}})"/>.
    /// </summary>
    protected void ModifyItem(GiveEventArgs args)
    {
        Dictionary<string, int> prevMultiset = GetActualItems(
            CollectedItemList,
            OrderedMemberList,
            OrderedTransitivePredecessorsLookup
        );
        CollectedItemList.Add(args.Orig.Name);
        Dictionary<string, int> nextMultiset = GetActualItems(
            CollectedItemList,
            OrderedMemberList,
            OrderedTransitivePredecessorsLookup
        );
        // the two multisets differ by 1 in exactly one key, as a guarantee of GetActualItems
        string next = nextMultiset
            .Single(kvp =>
                kvp.Value > (prevMultiset.TryGetValue(kvp.Key, out int value) ? value : 0)
            )
            .Key;
        args.Item = GetItem(next);
    }

    /// <summary>
    /// Gets the multiset of items which replaces the sequence of collected items.
    /// The sequence is first resorted according to the order of the main item list. Then each item is replaced by its first predecessor not yet in the result, or else unmodified.
    /// If an item is added when it and all of its predecessors are given, its multiplicity in the dictionary may be incremented beyond 1.
    /// </summary>
    public static Dictionary<string, int> GetActualItems(
        IEnumerable<string> collectedItems,
        List<string> itemList,
        Dictionary<string, List<string>> predecessors
    )
    {
        Dictionary<string, int> result = [];

        foreach (string item in collectedItems.OrderBy(itemList.IndexOf))
        {
            bool replaced = false;
            foreach (string p in predecessors[item])
            {
                if (!result.ContainsKey(p))
                {
                    result.Add(p, 1);
                    replaced = true;
                    break;
                }
            }
            if (!replaced)
            {
                if (!result.ContainsKey(item))
                {
                    result.Add(item, 1);
                }
                else
                {
                    result[item]++;
                }
            }
        }
        return result;
    }

    private void CheckAllItemsCompletelyDefined()
    {
        foreach (string s in OrderedMemberList)
        {
            if (!OrderedTransitivePredecessorsLookup.ContainsKey(s))
            {
                throw IncompletelyDefinedItem(s);
            }
        }
        if (OrderedMemberList.Count != OrderedTransitivePredecessorsLookup.Count)
        {
            throw IncompletelyDefinedItem(
                OrderedTransitivePredecessorsLookup.Keys.Except(OrderedMemberList).First()
            );
        }
        foreach (string p in OrderedTransitivePredecessorsLookup.Values.SelectMany(l => l))
        {
            if (!OrderedTransitivePredecessorsLookup.ContainsKey(p))
            {
                throw IncompletelyDefinedItem(p);
            }
        }
    }

    private void CheckTransitive()
    {
        foreach (KeyValuePair<string, List<string>> kvp in OrderedTransitivePredecessorsLookup)
        {
            foreach (string p in kvp.Value)
            {
                if (
                    OrderedTransitivePredecessorsLookup[p].Except(kvp.Value).FirstOrDefault()
                    is string s
                )
                {
                    throw TransitivityViolation(s, p, kvp.Key);
                }
            }
        }
    }

    private void CheckIrreflexive()
    {
        foreach (KeyValuePair<string, List<string>> kvp in OrderedTransitivePredecessorsLookup)
        {
            if (kvp.Value.Contains(kvp.Key))
            {
                throw IrreflexivityViolation(kvp.Key);
            }
        }
    }

    private void CheckOrderConsistency()
    {
        HashSet<string> items = [.. OrderedMemberList];
        for (int i = OrderedMemberList.Count - 2; i >= 0; i--)
        {
            items.Remove(OrderedMemberList[i + 1]);
            if (!items.IsSupersetOf(OrderedTransitivePredecessorsLookup[OrderedMemberList[i]]))
            {
                string missingPred = OrderedTransitivePredecessorsLookup[OrderedMemberList[i]]
                    .Except(items)
                    .First();
                throw OrderConsistencyViolation(missingPred, OrderedMemberList[i]);
            }
        }
    }

    private Exception IncompletelyDefinedItem(string name) =>
        new InvalidOperationException(
            $"Item {name} appears in data of {nameof(ProgressiveItemGroupModule)} with GroupID {GroupID}, "
                + $"but item is not both an entry of the member list and a key of the predecessor lookup."
        );

    private Exception UnexpectedMember(string name) =>
        new InvalidOperationException(
            $"Item {name} tagged with {nameof(ProgressiveItemGroupTag)} with GroupID {GroupID} was not declared on the module."
        );

    private Exception TransitivityViolation(string x, string y, string z) =>
        new InvalidOperationException(
            $"{nameof(ProgressiveItemGroupTag)} for {z} with GroupID {GroupID} is missing the transitive predecessor {x} of {y}."
        );

    private Exception IrreflexivityViolation(string name) =>
        new InvalidOperationException(
            $"{nameof(ProgressiveItemGroupTag)} for {name} with GroupID {GroupID} declares {name} as its own predecessor."
        );

    private Exception OrderConsistencyViolation(string x, string y) =>
        new InvalidOperationException(
            $"{y} is declared as a predecessor of {x}, but {y} occurs after {x}"
                + $" in the {nameof(OrderedMemberList)} for {nameof(ProgressiveItemGroupModule)} with GroupID {GroupID}."
        );
}
