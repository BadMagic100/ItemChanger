using ItemChanger.Events;
using ItemChanger.Items;
using ItemChanger.Tags;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemChanger.Modules
{
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
        /// A lookup of the group's items populated by <see cref="Register(ProgressiveItemGroupTag, Item)"/>.
        /// </summary>
        protected Dictionary<string, Item> GroupItems { get; } = [];
        /// <summary>
        /// A lookup of the group's predecessor partial ordering populated by <see cref="Register(ProgressiveItemGroupTag, Item)"/>.
        /// </summary>
        protected Dictionary<string, List<string>> OrderedTransitivePredecessorsLookup { get; } = [];

        /// <summary>
        /// The list of items associated to the group which have been collected, prior to replacement. Includes duplicates with multiplicity.
        /// </summary>
        public List<string> CollectedItemList { get; } = [];

        /// <inheritdoc/>
        protected override void DoLoad()
        {
            LifecycleEvents.OnEnterGame += LateInitialize;
        }

        /// <inheritdoc/>
        protected override void DoUnload()
        {
            LifecycleEvents.OnEnterGame -= LateInitialize;
            foreach (Item i in GroupItems.Values) i.ModifyItem -= ModifyItem;
            GroupItems.Clear();
            OrderedTransitivePredecessorsLookup.Clear();
        }

        /// <summary>
        /// Records the tag's data and its item to be managed by the module.
        /// </summary>>
        /// <exception cref="InvalidOperationException">The item was not found in the <see cref="OrderedMemberList"/>.</exception>
        public void Register(ProgressiveItemGroupTag tag, Item item)
        {
            if (!OrderedMemberList.Contains(item.name)) throw UnexpectedMember(item.name);
            GroupItems[item.name] = item;
            OrderedTransitivePredecessorsLookup[item.name] = tag.OrderedTransitivePredecessors;
            item.ModifyItem += ModifyItem;
        }

        /// <summary>
        /// Modifies the given item according to the output of <see cref="GetActualItems(IEnumerable{string}, List{string}, Dictionary{string, List{string}})"/>.
        /// </summary>
        protected void ModifyItem(GiveEventArgs args)
        {
            Dictionary<string, int> prev = GetActualItems(CollectedItemList, OrderedMemberList, OrderedTransitivePredecessorsLookup);
            CollectedItemList.Add(args.Orig.name);
            Dictionary<string, int> next = GetActualItems(CollectedItemList, OrderedMemberList, OrderedTransitivePredecessorsLookup);

            string nextItem = next.Single(kvp => kvp.Value > (prev.TryGetValue(kvp.Key, out int value) ? value : 0)).Key;
            args.Item = GroupItems[nextItem];
        }

        /// <summary>
        /// Gets the multiset of items which replaces the sequence of collected items.
        /// The sequence is first resorted according to the order of the main item list. Then each item is replaced by its first predecessor not yet in the result, or else unmodified.
        /// If an item is added when it and all of its predecessors are given, its multiplicity in the dictionary may be incremented beyond 1.
        /// </summary>
        public static Dictionary<string, int> GetActualItems(IEnumerable<string> collectedItems, List<string> itemList, Dictionary<string, List<string>> predecessors)
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

        /// <summary>
        /// Checks that the tag-provided data satisfies all compatibility requirements with each other and with the module.
        /// In particular, all expected items must be registered with the module, 
        /// the "predecessor" relation must be a strict partial order (transitive and irreflexive),
        /// and this partial order must be consistent with the order on the module's <see cref="OrderedMemberList"/>.
        /// </summary>
        protected void LateInitialize()
        {
            CheckAllDefined();
            CheckTransitive();
            CheckIrreflexive();
            CheckOrderConsistency();
        }

        private void CheckAllDefined()
        {
            foreach (string i in OrderedMemberList)
            {
                if (!GroupItems.ContainsKey(i) || !OrderedTransitivePredecessorsLookup.ContainsKey(i)) throw MissingMember(i);
            }
        }

        private void CheckTransitive()
        {
            foreach (var kvp in OrderedTransitivePredecessorsLookup)
            {
                foreach (string p in kvp.Value)
                {
                    if (OrderedTransitivePredecessorsLookup[p].Except(kvp.Value).FirstOrDefault() is string s) throw TransitivityViolation(s, p, kvp.Key);
                }
            }
        }

        private void CheckIrreflexive()
        {
            foreach (var kvp in OrderedTransitivePredecessorsLookup)
            {
                if (kvp.Value.Contains(kvp.Key)) throw IrreflexivityViolation(kvp.Key);
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
                    string missingPred = OrderedTransitivePredecessorsLookup[OrderedMemberList[i]].Except(items).First();
                    throw OrderConsistencyViolation(missingPred, OrderedMemberList[i]);
                }
            }
        }

        private Exception MissingMember(string name)
            => new KeyNotFoundException($"Item {name} was not loaded with a {nameof(ProgressiveItemGroupTag)} with GroupID {GroupID}.");
        private Exception UnexpectedMember(string name)
            => new InvalidOperationException($"Item {name} tagged with {nameof(ProgressiveItemGroupTag)} with GroupID {GroupID} was not declared on the module.");
        private Exception TransitivityViolation(string x, string y, string z)
            => new InvalidOperationException($"{nameof(ProgressiveItemGroupTag)} for {z} with GroupID {GroupID} is missing the transitive predecessor {x} of {y}.");
        private Exception IrreflexivityViolation(string name)
            => new InvalidOperationException($"{nameof(ProgressiveItemGroupTag)} for {name} with GroupID {GroupID} declares {name} as its own predecessor.");
        private Exception OrderConsistencyViolation(string x, string y)
            => new InvalidOperationException($"{y} is declared as a predecessor of {x}, but {y} occurs after {x}" +
                $" in the {nameof(OrderedMemberList)} for {nameof(ProgressiveItemGroupModule)} with GroupID {GroupID}.");
    }
}
