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
            ItemChanger.LoggerProxy.LogInfo("In register!");
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
            ItemChanger.LoggerProxy.LogInfo("In modify!");
            HashSet<string> prev = GetActualItems(CollectedItemList, OrderedMemberList, OrderedTransitivePredecessorsLookup);
            CollectedItemList.Add(args.Orig.name);
            HashSet<string> next = GetActualItems(CollectedItemList, OrderedMemberList, OrderedTransitivePredecessorsLookup);
            next.ExceptWith(prev);
            string? nextItem = next.SingleOrDefault();
            if (nextItem is null)
            {
                // either args.Orig is a duplicate, or duplicates have exhausted the item poset containing Orig.
                return;
            }
            args.Item = GroupItems[nextItem];
        }

        /// <summary>
        /// Gets the set of items which replaces the sequence of collected items. 
        /// The sequence is first resorted according to the order of the main item list. Then each item is replaced by its first predecessor not yet in the result, or else unmodified.
        /// Collected items can contain duplicates. If an item and all of its predecessors are in the result, then a duplicate of the item has no effect.
        /// </summary>
        public static HashSet<string> GetActualItems(IEnumerable<string> collectedItems, List<string> itemList, Dictionary<string, List<string>> predecessors)
        {
            HashSet<string> result = [];
            foreach (string item in collectedItems.OrderBy(itemList.IndexOf))
            {
                bool replaced = false;
                foreach (string p in predecessors[item])
                {
                    if (result.Add(p))
                    {
                        replaced = true;
                        break;
                    }
                }
                if (!replaced) result.Add(item);
            }
            return result;
        }

        /// <summary>
        /// Checks that the tag-provided data satisfies all compatibility requirements with each other and with the module.
        /// In particular, all expected items must be registered with the module, 
        /// the "predecessor" relation must be a partial order (transitive, irreflexive, antisymmetric),
        /// and this partial order must be consistent with the order on the module's <see cref="OrderedMemberList"/>.
        /// </summary>
        protected void LateInitialize()
        {
            CheckAllDefined();
            CheckTransitive();
            CheckIrreflexive();
            CheckAntisymmetric();
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

        private void CheckAntisymmetric()
        {
            foreach (var kvp in OrderedTransitivePredecessorsLookup)
            {
                foreach (string p in kvp.Value) if (OrderedTransitivePredecessorsLookup[p].Contains(kvp.Key)) throw AntisymmetryViolation(kvp.Key, p);
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
        private Exception AntisymmetryViolation(string x, string y)
            => new InvalidOperationException($"{nameof(ProgressiveItemGroupTag)}s for {x}, {y} with GroupID {GroupID} declare each other as predecessors.");
        private Exception OrderConsistencyViolation(string x, string y)
            => new InvalidOperationException($"{y} is declared as a predecessor of {x}, but {y} occurs after {x}" +
                $" in the {nameof(OrderedMemberList)} for {nameof(ProgressiveItemGroupModule)} with GroupID {GroupID}.");
    }
}
