using ItemChanger.Internal;
using ItemChanger.Items;
using ItemChanger.Modules;
using ItemChanger.Tags.Constraints;
using System.Linq;

namespace ItemChanger.Tags
{
    /// <summary>
    /// An item tag which allows the parent item to be replaced by an item dynamically chosen by <see cref="ProgressiveItemGroupModule"/>.
    /// </summary>
    [ItemTag]
    public class ProgressiveItemGroupTag : Tag
    {
        /// <summary>
        /// An id which matches the id on a <see cref="ProgressiveItemGroupModule"/> for the item group.
        /// </summary>
        public required string GroupID { get; init; }

        /// <inheritdoc/>
        protected override void DoLoad(TaggableObject parent)
        {
            base.DoLoad(parent);
            ItemChangerProfile.ActiveProfile.Modules.OfType<ProgressiveItemGroupModule>()
                .First(m => m.GroupID == GroupID)
                .RegisterItem(this, (Item)parent);
        }

        /// <inheritdoc/>
        protected override void DoUnload(TaggableObject parent)
        {
            base.DoUnload(parent);
            // unloading handled by module
        }
    }
}
