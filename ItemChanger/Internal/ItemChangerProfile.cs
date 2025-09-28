using ItemChanger.Items;
using ItemChanger.Tags;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ItemChanger.Internal;

public class ItemChangerProfile
{
    private static ItemChangerProfile? activeProfile;

    /// <summary>
    /// The active ItemChanger profile. Will throw if no profile is loaded.
    /// </summary>
    public static ItemChangerProfile ActiveProfile
    {
        get => activeProfile ?? throw new InvalidOperationException("Attempted to access active profile while no profile was loaded");
    }

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

    [JsonProperty]
    private readonly Dictionary<string, Placement> placements = new();

    [JsonProperty]
    public ModuleCollection Modules { get; }

    internal LoadState state = LoadState.Unloaded;

    public ItemChangerProfile()
    {
        Modules = new(this);
    }

    public IEnumerable<Placement> GetPlacements() => placements.Values;

    public IEnumerable<Item> GetItems() => placements.Values.SelectMany(x => x.Items);

    public Placement GetPlacement(string name)
    {
        if (!placements.TryGetValue(name, out Placement? placement))
        {
            throw new KeyNotFoundException($"No placement with name {name} found");
        }
        return placement;
    }

    public bool TryGetPlacement(string name, [NotNullWhen(true)] out Placement? placement)
    {
        return placements.TryGetValue(name, out placement);
    }

    public void ResetPersistentItems(Persistence persistence)
    {
        if (persistence == Persistence.NonPersistent)
        {
            throw new ArgumentException($"Cannot reset non-persistent items (persistence {nameof(Persistence.NonPersistent)})", nameof(persistence));
        }

        foreach (Item item in GetItems())
        {
            if (item.GetTag<IPersistenceTag>(out IPersistenceTag? tag) && tag.Persistence == persistence)
            {
                item.RefreshObtained();
            }
        }
    }

    public void Load()
    {
        if (state != LoadState.Unloaded)
        {
            throw new InvalidOperationException($"Cannot load an already loaded profile. Current state is {state}");
        }
        if (activeProfile != null)
        {
            throw new InvalidOperationException("Cannot load profile while another profile is active");
        }

        activeProfile = this;
        state = LoadState.LoadStarted;

        state = LoadState.ModuleLoadStarted;
        Modules.Initialize();
        state = LoadState.ModuleLoadCompleted;

        state = LoadState.PlacementsLoadStarted;
        foreach (Placement placement in placements.Values)
        {
            placement.LoadOnce();
        }
        state = LoadState.PlacementsLoadCompleted;

        state = LoadState.LoadCompleted;
    }

    public void Unload()
    {
        if (state != LoadState.LoadCompleted)
        {
            throw new InvalidOperationException($"Cannot unload an unloaded or partially loaded profile. Current state is {state}");
        }

        state = LoadState.PlacementsLoadCompleted;
        foreach (Placement placement in placements.Values)
        {
            placement.Unload();
        }
        state = LoadState.PlacementsLoadStarted;

        state = LoadState.ModuleLoadCompleted;
        Modules.Unload();
        state = LoadState.ModuleLoadStarted;

        state = LoadState.Unloaded;
        activeProfile = null;
    }

    public void AddPlacement(Placement placement, PlacementConflictResolution conflictResolution = PlacementConflictResolution.MergeKeepingNew)
    {
        if (state == LoadState.PlacementsLoadStarted)
        {
            throw new InvalidOperationException("Cannot add a placement while placement loading is in progress");
        }

        if (placements.TryGetValue(placement.Name, out Placement? existP))
        {
            switch (conflictResolution)
            {
                case PlacementConflictResolution.MergeKeepingNew:
                    placement.Items.AddRange(existP.Items);
                    placements[placement.Name] = placement;
                    if (state >= LoadState.PlacementsLoadCompleted)
                    {
                        existP.Unload();
                    }
                    break;
                case PlacementConflictResolution.MergeKeepingOld:
                    existP.Items.AddRange(placement.Items);
                    if (state >= LoadState.PlacementsLoadCompleted)
                    {
                        foreach (Item item in placement.Items)
                        {
                            item.LoadOnce();
                        }
                    }
                    break;
                case PlacementConflictResolution.Replace:
                    placements[placement.Name] = placement;
                    if (state >= LoadState.PlacementsLoadCompleted)
                    {
                        existP.Unload();
                    }
                    break;
                case PlacementConflictResolution.Ignore:
                    break;
                case PlacementConflictResolution.Throw:
                default:
                    throw new ArgumentException($"A placement named {placement.Name} already exists");
            }
        }
        else
        {
            placements.Add(placement.Name, placement);
        }

        // if the final placement ending up in the profile is the newly added one, it may need to be loaded to catch up.
        if (state >= LoadState.PlacementsLoadCompleted && placements[placement.Name] == placement)
        {
            placement.LoadOnce();
        }
    }

    public void AddPlacements(IEnumerable<Placement> placements, PlacementConflictResolution conflictResolution = PlacementConflictResolution.MergeKeepingNew)
    {
        foreach (Placement placement in placements)
        {
            AddPlacement(placement, conflictResolution);
        }
    }
}
