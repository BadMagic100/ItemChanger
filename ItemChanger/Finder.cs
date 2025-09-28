using ItemChanger.Items;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace ItemChanger;

public static class Finder
{
    /// <summary>
    /// Invoked by Finder.GetItem. The initial arguments are the requested name, and null. If the event finishes with a non-null item, that item is returned to the requester.
    /// <br/>Otherwise, the ItemChanger internal implementation of that item is cloned and returned, if it exists. Otherwise, null is returned.
    /// </summary>
    public static event Action<GetItemEventArgs>? GetItemOverride;
    /// <summary>
    /// Invoked by Finder.GetLocation. The initial arguments are the requested name, and null. If the event finishes with a non-null location, that location is returned to the requester.
    /// <br/>Otherwise, the ItemChanger internal implementation of that location is cloned and returned, if it exists. Otherwise, null is returned.
    /// </summary>
    public static event Action<GetLocationEventArgs>? GetLocationOverride;

    private static readonly Dictionary<string, Item> Items = new();
    private static readonly Dictionary<string, Location> Locations = new();

    public static IEnumerable<string> ItemNames => Items.Keys;
    public static IEnumerable<string> LocationNames => Locations.Keys;

    /// <summary>
    /// The most general method for looking up an item. Invokes an event to allow subscribers to modify the search result. Return value defaults to that of GetItemInternal.
    /// </summary>
    public static Item? GetItem(string name)
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
    /// Searches for the item by name, first in the CustomItems list, then in the list of extra sheets held by GlobalSettings, and finally in the default item sheet. Returns null if not found.
    /// </summary>
    public static Item? GetItemInternal(string name)
    {
        Items.TryGetValue(name, out Item? item);
        return item;
    }

    /// <summary>
    /// The most general method for looking up a location. Invokes an event to allow subscribers to modify the search result. Return value defaults to that of GetLocationInternal.
    /// </summary>
    public static Location? GetLocation(string name)
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
    /// Searches for the location by name, first in the CustomLocations list, then in the list of extra sheets held by GlobalSettings, and finally in the default location sheet. Returns null if not found.
    /// </summary>
    public static Location? GetLocationInternal(string name)
    {
        Locations.TryGetValue(name, out Location? location);
        return location;
    }

    public static void DefineItem(Item item, bool overwrite = false)
    {
        if (Items.ContainsKey(item.name) && !overwrite)
        {
            throw new ArgumentException($"Item {item.name} is already defined (type is {item.GetType()}).");
        }

        Items[item.name] = item;
    }

    public static bool UndefineItem(string name) => Items.Remove(name);

    public static void DefineLocation(Location loc, bool overwrite = false)
    {
        if (Locations.ContainsKey(loc.Name) && !overwrite)
        {
            throw new ArgumentException($"Location {loc.Name} is already defined (type is {loc.GetType()}).");
        }

        Locations[loc.Name] = loc;
    }

    public static bool UndefineLocation(string name) => Locations.Remove(name);

    /// <summary>
    /// Utility to serialize an object with the necessary metadata for polymorphic deserialization
    /// </summary>
    /// <param name="stream">Stream to serialize to</param>
    /// <param name="o">The object to be serialized</param>
    public static void Serialize(Stream stream, object o)
    {
        JsonSerializer js = new()
        {
            DefaultValueHandling = DefaultValueHandling.Include,
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto,
        };

        js.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

        using StreamWriter sw = new(stream);
        js.Serialize(sw, o);
    }

    /// <summary>
    /// Utility to deserialize an object polymorphically for use in Finder (typically created from <see cref="Serialize(Stream, object)"/>).
    /// </summary>
    /// <typeparam name="T">The type to deserialize to</typeparam>
    /// <param name="stream">The stream to read from</param>
    public static T? DeserializeResource<T>(Stream stream)
    {
        JsonSerializer js = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
        };
        js.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        using StreamReader sr = new(stream);
        return js.Deserialize<T>(new JsonTextReader(sr));
    }
}
