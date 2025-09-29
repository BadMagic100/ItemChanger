using ItemChanger.Internal;
using ItemChanger.Items;
using ItemChanger.Modules;
using ItemChanger.Tags.Constraints;
using System.Collections.Generic;
using System.Linq;

namespace ItemChanger.Tags
{
    /// <summary>
    /// An item tag which replaces the parent item by a dynamically chosen item.
    /// </summary>
    [ItemTag]
    public class ProgressiveItemGroupTag : Tag
    {
        /// <summary>
        /// An id which matches the id on a <see cref="ProgressiveItemGroupModule"/> for the item group.
        /// </summary>
        public required string GroupID { get; init; }
        /// <summary>
        /// A list of unique item names which must be given before the tagged item. The order of the list determines the preferred order of replacement.
        /// </summary>
        public List<string> OrderedTransitivePredecessors { get; init; } = [];

        /// <inheritdoc/>
        protected override void DoLoad(TaggableObject parent)
        {
            base.DoLoad(parent);
            ItemChangerProfile.ActiveProfile.Modules.OfType<ProgressiveItemGroupModule>()
                .First(m => m.GroupID == GroupID)
                .Register(this, (Item)parent);
        }

        /// <inheritdoc/>
        protected override void DoUnload(TaggableObject parent)
        {
            base.DoUnload(parent);
        }
    }
}
