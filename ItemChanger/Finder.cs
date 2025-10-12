using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ItemChanger.Events.Args;
using ItemChanger.Items;

namespace ItemChanger;

public class FinderSheet<T>(Dictionary<string, T> members, float priority)
{
    public float Priority => priority;

    public IEnumerable<string> Names => members.Keys;

    public virtual bool Enabled => true;

    public bool TryGet(string name, [NotNullWhen(true)] out T? result)
    {
        return members.TryGetValue(name, out result!);
    }
}

public class Finder
{
    /// <summary>
    /// Invoked by Finder.GetItem. The initial arguments are the requested name, and null. If the event finishes with a non-null item, that item is returned to the requester.
    /// <br/>Otherwise, the ItemChanger internal implementation of that item is cloned and returned, if it exists. Otherwise, null is returned.
    /// </summary>
    public event Action<GetItemEventArgs>? GetItemOverride;

    /// <summary>
    /// Invoked by Finder.GetLocation. The initial arguments are the requested name, and null. If the event finishes with a non-null location, that location is returned to the requester.
    /// <br/>Otherwise, the ItemChanger internal implementation of that location is cloned and returned, if it exists. Otherwise, null is returned.
    /// </summary>
    public event Action<GetLocationEventArgs>? GetLocationOverride;

    private readonly Dictionary<string, Item> Items = [];
    private readonly Dictionary<string, Location> Locations = [];

    private readonly List<FinderSheet<Item>> ItemSheets = [];
    private readonly List<FinderSheet<Location>> LocationSheets = [];
    public IEnumerable<string> ItemNames =>
        Items.Keys.Concat(ItemSheets.SelectMany(s => s.Names)).Distinct();
    public IEnumerable<string> LocationNames =>
        Locations.Keys.Concat(LocationSheets.SelectMany(s => s.Names)).Distinct();

    /// <summary>
    /// The most general method for looking up an item. Invokes an event to allow subscribers to modify the search result. Return value defaults to that of GetItemInternal.
    /// </summary>
    public Item? GetItem(string name)
    {
        GetItemEventArgs args = new(name);
        GetItemOverride?.Invoke(args);
        if (args.Current != null)
        {
            return args.Current;
        }
        else
        {
            return GetItemInternal(name);
        }
    }

    /// <summary>
    /// Searches for the item by name, first in the custom item list, then in the list of enabled additional item sheets by priority. Returns null if not found.
    /// </summary>
    public Item? GetItemInternal(string name)
    {
        if (Items.TryGetValue(name, out Item? item))
        {
            return item;
        }
        foreach (FinderSheet<Item> sheet in ItemSheets)
        {
            if (sheet.Enabled && sheet.TryGet(name, out Item? item1))
            {
                return item1;
            }
        }
        return null;
    }

    /// <summary>
    /// The most general method for looking up a location. Invokes an event to allow subscribers to modify the search result. Return value defaults to that of GetLocationInternal.
    /// </summary>
    public Location? GetLocation(string name)
    {
        GetLocationEventArgs args = new(name);
        GetLocationOverride?.Invoke(args);
        if (args.Current != null)
        {
            return args.Current;
        }
        else
        {
            return GetLocationInternal(name);
        }
    }

    /// <summary>
    /// Searches for the location by name, first in the custom location list, then in the list of enabled additional location sheets by priority. Returns null if not found.
    /// </summary>
    public Location? GetLocationInternal(string name)
    {
        if (Locations.TryGetValue(name, out Location? location))
        {
            return location;
        }
        foreach (FinderSheet<Location> sheet in LocationSheets)
        {
            if (sheet.Enabled && sheet.TryGet(name, out Location? location1))
            {
                return location1;
            }
        }
        return null;
    }

    public void DefineItem(Item item, bool overwrite = false)
    {
        if (Items.ContainsKey(item.name) && !overwrite)
        {
            throw new ArgumentException(
                $"Item {item.name} is already defined (type is {item.GetType()})."
            );
        }

        Items[item.name] = item;
    }

    public void DefineLocation(Location loc, bool overwrite = false)
    {
        if (Locations.ContainsKey(loc.Name) && !overwrite)
        {
            throw new ArgumentException(
                $"Location {loc.Name} is already defined (type is {loc.GetType()})."
            );
        }

        Locations[loc.Name] = loc;
    }

    /// <summary>
    /// Adds an ItemSheet to finder.
    /// </summary>
    public void DefineItemSheet(FinderSheet<Item> sheet)
    {
        int i = 0;
        while (i < ItemSheets.Count && ItemSheets[i].Priority > sheet.Priority)
        {
            i++;
        }
        ItemSheets.Insert(i, sheet);
    }

    /// <summary>
    /// Adds a LocationSheet to finder.
    /// </summary>
    public void DefineLocationSheet(FinderSheet<Location> sheet)
    {
        int i = 0;
        while (i < LocationSheets.Count && LocationSheets[i].Priority > sheet.Priority)
        {
            i++;
        }
        LocationSheets.Insert(i, sheet);
    }
}
