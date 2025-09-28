using ItemChanger.Items;
using System;

namespace ItemChanger.Tags;

/// <summary>
/// An attribute which appears on a subclass of <see cref="Tag"/> to indicate that the tag can be placed on a certain type of <see cref="TaggableObject"/>.
/// <br/>If the tag has multiple copies of the attribute, it can be placed on any of the specified types. The attribute is not inherited.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class TagConstrainedToAttribute<T> : Attribute where T : TaggableObject
{
    /// <summary>
    /// The type of object that the tag is constrained to
    /// </summary>
    public Type TaggableObjectType => typeof(T);
}

/// <summary>
/// Convenience implementation of <see cref="TagConstrainedToAttribute{T}"/> for items.
/// </summary>
public class ItemTagAttribute : TagConstrainedToAttribute<Item> { }

/// <summary>
/// Convenience implementation of <see cref="TagConstrainedToAttribute{T}"/> for locations.
/// </summary>
public class LocationTagAttribute : TagConstrainedToAttribute<Location> { }

/// <summary>
/// Convenience implementation of <see cref="TagConstrainedToAttribute{T}"/> for placements.
/// </summary>
public class PlacementTagAttribute : TagConstrainedToAttribute<Placement> { }
