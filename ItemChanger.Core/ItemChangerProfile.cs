using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using ItemChanger.Containers;
using ItemChanger.Enums;
using ItemChanger.Events;
using ItemChanger.Items;
using ItemChanger.Modules;
using ItemChanger.Placements;
using ItemChanger.Serialization;
using ItemChanger.Tags;
using Newtonsoft.Json;

namespace ItemChanger;

/// <summary>
/// Represents the ItemChanger data (placements, modules, hooks) associated with an in-game save file, tied to a particular host.
/// </summary>
public class ItemChangerProfile : IDisposable
{
    internal enum LoadState : uint
    {
        Unloaded = 0,
        LoadStarted = 1,
        ModuleLoadStarted = 2,
        ModuleLoadCompleted = 3,
        PlacementsLoadStarted = 4,
        PlacementsLoadCompleted = 5,
        LoadCompleted = uint.MaxValue,
    }

    [JsonProperty("Placements")]
    private readonly Dictionary<string, Placement> placements = [];

    /// <summary>
    /// Gets the set of modules that are part of this profile.
    /// </summary>
    [JsonProperty]
    public ModuleCollection Modules { get; private init; } = [];

    private bool hooked;
    internal LoadState State { get; private set; } = LoadState.Unloaded;

    private ItemChangerHost host;
    private LifecycleEvents.Invoker lifecycleInvoker;
    private GameEvents.Invoker gameInvoker;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
    [JsonConstructor]
    private ItemChangerProfile() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor.

    /// <summary>
    /// Initializes a new profile
    /// </summary>
    /// <param name="host">The associated host</param>
    public ItemChangerProfile(ItemChangerHost host)
    {
        AttachHost(host);

        Modules = [.. host.BuildDefaultModules()];

        DoHook();
    }

    /// <summary>
    /// Loads a profile from a stream
    /// </summary>
    /// <param name="host">The associated host</param>
    /// <param name="stream">The stream to read from</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">The stream doesn't contain a profile.</exception>
    /// <returns>The deserialized profile.</returns>
    public static ItemChangerProfile FromStream(ItemChangerHost host, Stream stream)
    {
        ItemChangerProfile? profile = SerializationHelper.DeserializeResource<ItemChangerProfile>(
            stream
        );
        if (profile == null)
        {
            throw new ArgumentException(
                "The provided stream did not contain a valid profile",
                nameof(stream)
            );
        }
        profile.AttachHost(host);
        profile.DoHook();
        return profile;
    }

    /// <summary>
    /// Ensures that the profile is unloaded and unhooked when garbage-collected
    /// </summary>
    ~ItemChangerProfile()
    {
        Dispose();
    }

    private bool disposed;

    /// <inheritdoc/>
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        if (State == LoadState.LoadCompleted)
        {
            Unload();
        }
        DoUnhook();
        host.ActiveProfile = null;
        GC.SuppressFinalize(this);
        disposed = true;
    }

    /// <summary>
    /// Saves the profile to a stream as a JSON blob
    /// </summary>
    /// <param name="stream">The stream to save to.</param>
    public void ToStream(Stream stream)
    {
        SerializationHelper.Serialize(stream, this);
    }

    /// <summary>
    /// Gets all placements currently registered with this profile.
    /// </summary>
    public IEnumerable<Placement> GetPlacements() => placements.Values;

    /// <summary>
    /// Enumerates every item across all placements.
    /// </summary>
    public IEnumerable<Item> GetItems() => placements.Values.SelectMany(x => x.Items);

    /// <summary>
    /// Retrieves a placement by name.
    /// </summary>
    /// <param name="name">Placement name.</param>
    /// <returns>The requested placement.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when no placement with the given name exists.</exception>
    public Placement GetPlacement(string name)
    {
        if (!placements.TryGetValue(name, out Placement? placement))
        {
            throw new KeyNotFoundException($"No placement with name {name} found");
        }
        return placement;
    }

    /// <summary>
    /// Attempts to find a placement by name.
    /// </summary>
    /// <param name="name">Placement name.</param>
    /// <param name="placement">Resolved placement when found.</param>
    /// <returns><see langword="true"/> when the placement exists; otherwise <see langword="false"/>.</returns>
    public bool TryGetPlacement(string name, [NotNullWhen(true)] out Placement? placement)
    {
        return placements.TryGetValue(name, out placement);
    }

    /// <summary>
    /// Resets the obtained state on items that match the provided persistence category.
    /// </summary>
    /// <param name="persistence">Persistence type to refresh.</param>
    internal void ResetPersistentItems(Persistence persistence)
    {
        if (persistence == Persistence.NonPersistent)
        {
            throw new ArgumentException(
                $"Cannot reset non-persistent items (persistence {nameof(Persistence.NonPersistent)})",
                nameof(persistence)
            );
        }

        foreach (Item item in GetItems())
        {
            if (
                item.GetTag<IPersistenceTag>(out IPersistenceTag? tag)
                && tag.Persistence == persistence
            )
            {
                item.RefreshObtained();
            }
        }
    }

    /// <summary>
    /// Loads modules and placements associated with this profile.
    /// </summary>
    public void Load()
    {
        if (State != LoadState.Unloaded)
        {
            throw new InvalidOperationException(
                $"Cannot load an already loaded profile. Current state is {State}"
            );
        }

        State = LoadState.LoadStarted;

        State = LoadState.ModuleLoadStarted;
        Modules.Load();
        State = LoadState.ModuleLoadCompleted;

        State = LoadState.PlacementsLoadStarted;
        foreach (Placement placement in placements.Values)
        {
            placement.LoadOnce();
        }
        State = LoadState.PlacementsLoadCompleted;

        State = LoadState.LoadCompleted;
    }

    /// <summary>
    /// Unloads modules and placements associated with this profile.
    /// </summary>
    public void Unload()
    {
        if (State != LoadState.LoadCompleted)
        {
            throw new InvalidOperationException(
                $"Cannot unload an unloaded or partially loaded profile. Current state is {State}"
            );
        }

        State = LoadState.PlacementsLoadCompleted;
        foreach (Placement placement in placements.Values)
        {
            placement.Unload();
        }
        State = LoadState.PlacementsLoadStarted;

        State = LoadState.ModuleLoadCompleted;
        Modules.Unload();
        State = LoadState.ModuleLoadStarted;

        State = LoadState.Unloaded;
    }

    /// <summary>
    /// Adds a placement to the profile, optionally resolving naming conflicts.
    /// </summary>
    /// <param name="placement">Placement to add.</param>
    /// <param name="conflictResolution">Conflict behavior when a placement with the same name exists.</param>
    public void AddPlacement(
        Placement placement,
        PlacementConflictResolution conflictResolution = PlacementConflictResolution.MergeKeepingNew
    )
    {
        if (State == LoadState.PlacementsLoadStarted)
        {
            throw new InvalidOperationException(
                "Cannot add a placement while placement loading is in progress"
            );
        }

        if (placements.TryGetValue(placement.Name, out Placement? existP))
        {
            switch (conflictResolution)
            {
                case PlacementConflictResolution.MergeKeepingNew:
                    placement.Items.AddRange(existP.Items);
                    placements[placement.Name] = placement;
                    if (State >= LoadState.PlacementsLoadCompleted)
                    {
                        existP.Unload();
                    }
                    break;
                case PlacementConflictResolution.MergeKeepingOld:
                    existP.Items.AddRange(placement.Items);
                    if (State >= LoadState.PlacementsLoadCompleted)
                    {
                        foreach (Item item in placement.Items)
                        {
                            item.LoadOnce();
                        }
                    }
                    break;
                case PlacementConflictResolution.Replace:
                    placements[placement.Name] = placement;
                    if (State >= LoadState.PlacementsLoadCompleted)
                    {
                        existP.Unload();
                    }
                    break;
                case PlacementConflictResolution.Ignore:
                    break;
                case PlacementConflictResolution.Throw:
                default:
                    throw new ArgumentException(
                        $"A placement named {placement.Name} already exists"
                    );
            }
        }
        else
        {
            placements.Add(placement.Name, placement);
        }

        // if the final placement ending up in the profile is the newly added one, it may need to be loaded to catch up.
        if (State >= LoadState.PlacementsLoadCompleted && placements[placement.Name] == placement)
        {
            placement.LoadOnce();
        }
    }

    public void AddPlacements(
        IEnumerable<Placement> placements,
        PlacementConflictResolution conflictResolution = PlacementConflictResolution.MergeKeepingNew
    )
    {
        foreach (Placement placement in placements)
        {
            AddPlacement(placement, conflictResolution);
        }
    }

    [MemberNotNull(nameof(host), nameof(lifecycleInvoker), nameof(gameInvoker))]
    private void AttachHost(ItemChangerHost host)
    {
        host.ActiveProfile = this;
        this.host = host;
        lifecycleInvoker = new LifecycleEvents.Invoker(host.LifecycleEvents);
        gameInvoker = new GameEvents.Invoker(this, host.GameEvents);
    }

    private void DoHook()
    {
        if (hooked)
        {
            return;
        }

        host.GameEvents.Hook();
        host.PrepareEvents(lifecycleInvoker, gameInvoker);
        foreach (Container c in host.ContainerRegistry)
        {
            c.Load();
        }

        lifecycleInvoker.NotifyHooked();

        hooked = true;
    }

    private void DoUnhook()
    {
        if (!hooked)
        {
            return;
        }

        foreach (Container c in host.ContainerRegistry)
        {
            c.Unload();
        }
        host.UnhookEvents(lifecycleInvoker, gameInvoker);
        host.GameEvents.Unhook();

        lifecycleInvoker.NotifyUnhooked();

        hooked = false;
    }
}
