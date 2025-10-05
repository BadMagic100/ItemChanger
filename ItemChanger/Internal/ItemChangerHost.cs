using ItemChanger.Containers;
# nullable enable

using ItemChanger.Events;
using ItemChanger.Logging;
using System;

namespace ItemChanger.Internal;

/// <summary>
/// Represents a game-specific host mod which acts as a static liason between ItemChanger and the game. This includes
/// hooking game-specific events and providing a <see cref="Finder"/> registry, containers, and logging for generic
/// implementations.
/// </summary>
public abstract class ItemChangerHost
{
    private static ItemChangerHost? singleton;
    /// <summary>
    /// The global host singleton. A host must be created before using any ItemChanger functionality.
    /// </summary>
    public static ItemChangerHost Singleton => singleton
        ?? throw new InvalidOperationException("Cannot use singleton host before a host is created.");

    /// <summary>
    /// The logger to be used by ItemChanger to emit log output to the game.
    /// </summary>
    public abstract ILogger Logger { get; }

    private ItemChangerProfile? activeProfile;
    /// <summary>
    /// The active ItemChanger profile, if one is active.
    /// </summary>
    /// <remarks>
    /// Many ItemChanger classes assume that they are only accessed from within the context of a profile.
    /// With the exception of the Host and Profile APIs, callers should assume that an active profile is
    /// required to use any ItemChanger APIs safely.
    /// </remarks>
    public ItemChangerProfile? ActiveProfile
    {
        get => activeProfile;
        set
        {
            if (value != null && activeProfile != null)
            {
                throw new InvalidOperationException("Cannot overwrite an active profile.");
            }
            activeProfile = value;
        }
    }

    /// <summary>
    /// Registry of container type definitions for this host.
    /// </summary>
    public abstract ContainerRegistry ContainerRegistry { get; }

    /// <summary>
    /// Allows hosts to hook game-specific code to invoke the generic itemchanger event system.
    /// </summary>
    /// <param name="lifecycleInvoker">Invoker for ItemChanger lifecycle events such as starting a new game.</param>
    /// <param name="gameInvoker">Invoker for game events such as scene transitions.</param>
    public abstract void PrepareEvents(LifecycleEvents.Invoker lifecycleInvoker, GameEvents.Invoker gameInvoker);

    /// <summary>
    /// Allows hosts to undo hooks set in <see cref="PrepareEvents(LifecycleEvents.Invoker, GameEvents.Invoker)"/>
    /// </summary>
    /// <param name="lifecycleInvoker">Invoker for ItemChanger lifecycle events such as starting a new game.</param>
    /// <param name="gameInvoker">Invoker for game events such as scene transitions.</param>
    public abstract void UnhookEvents(LifecycleEvents.Invoker lifecycleInvoker, GameEvents.Invoker gameInvoker);

    /// <summary>
    /// Initializes the host as the global singleton host.
    /// </summary>
    /// <exception cref="InvalidOperationException">If another host has already been created.</exception>
    public ItemChangerHost()
    {
        if (singleton != null)
        {
            throw new InvalidOperationException("Cannot create multiple hosts");
        }
        singleton = this;
    }

    /// <summary>
    /// This is intented as a temporary stopgap to allow test instances to clean up the singleton host.
    /// It will not be sufficient if any tests run in parallel.
    /// </summary>
    protected void DetachSingleton()
    {
        singleton = null;
    }
}
