using System;
using System.Collections.Generic;

namespace ItemChanger.Events;

/// <summary>
/// Exposes host lifecycle event hooks such as entering/leaving the game or starting a new save.
/// </summary>
public sealed class LifecycleEvents
{
    /// <summary>
    /// Called after ItemChanger hooks, which occurs whenever an ItemChanger profile is created or loaded.
    /// </summary>
    public event Action OnItemChangerHook
    {
        add => onItemChangerHookSubscribers.Add(value);
        remove => onItemChangerHookSubscribers.Remove(value);
    }
    private readonly List<Action> onItemChangerHookSubscribers = [];

    /// <summary>
    /// Called after ItemChanger unhooks, which occurs whenever an ItemChanger profile is disposed.
    /// </summary>
    public event Action OnItemChangerUnhook
    {
        add => onItemChangerUnhookSubscribers.Add(value);
        remove => onItemChangerUnhookSubscribers.Remove(value);
    }
    private readonly List<Action> onItemChangerUnhookSubscribers = [];

    /// <summary>
    /// An event called before creating a new save file in the game (profiles are not yet loaded at this time).
    /// </summary>
    public event Action BeforeStartNewGame
    {
        add => beforeStartNewGameSubscribers.Add(value);
        remove => beforeStartNewGameSubscribers.Remove(value);
    }
    private readonly List<Action> beforeStartNewGameSubscribers = [];

    /// <summary>
    /// An event called after creating a new save file in the game
    /// </summary>
    public event Action AfterStartNewGame
    {
        add => afterStartNewGameSubscribers.Add(value);
        remove => afterStartNewGameSubscribers.Remove(value);
    }
    private readonly List<Action> afterStartNewGameSubscribers = [];

    /// <summary>
    /// An event called before loading in to an existing save file in the game (profiles are not yet loaded at this time).
    /// </summary>
    public event Action BeforeContinueGame
    {
        add => beforeContinueGameSubscribers.Add(value);
        remove => beforeContinueGameSubscribers.Remove(value);
    }
    private readonly List<Action> beforeContinueGameSubscribers = [];

    /// <summary>
    /// An event called after loading in to an existing save file.
    /// </summary>
    public event Action AfterContinueGame
    {
        add => afterContinueGameSubscribers.Add(value);
        remove => afterContinueGameSubscribers.Remove(value);
    }
    private readonly List<Action> afterContinueGameSubscribers = [];

    /// <summary>
    /// An event called just before loading into a new or existing save file. Happens after <see cref="BeforeStartNewGame"/>
    /// </summary>
    public event Action OnEnterGame
    {
        add => onEnterGameSubscribers.Add(value);
        remove => onEnterGameSubscribers.Remove(value);
    }
    private readonly List<Action> onEnterGameSubscribers = [];

    /// <summary>
    /// An event called after it is first safe to give items. Can be called any time after <see cref="OnEnterGame"/>.
    /// </summary>
    /// <remarks>
    /// This event is used by consumers to prevent partially-initialized game state from breaking give effects, so as a general
    /// rule of thumb, it is a good idea to invoke this only after the game is fully initialized (e.g., after complely loading in
    /// to the first gameplay scene).
    /// </remarks>
    public event Action OnSafeToGiveItems
    {
        add => onSafeToGiveItemsSubscribers.Add(value);
        remove => onSafeToGiveItemsSubscribers.Remove(value);
    }
    private readonly List<Action> onSafeToGiveItemsSubscribers = [];

    /// <summary>
    /// An event called as the game is about to exit, but before the profile unloads.
    /// </summary>
    public event Action OnLeaveGame
    {
        add => onLeaveGameSubscribers.Add(value);
        remove => onLeaveGameSubscribers.Remove(value);
    }
    private readonly List<Action> onLeaveGameSubscribers = [];

    /// <summary>
    /// Used by the active <see cref="ItemChanger.ItemChangerHost"/> to wire up lifecycle events.
    /// It is the host's responsibility to invoke the events in the order specified by the documentation.
    /// </summary>
    public class Invoker
    {
        private readonly LifecycleEvents events;

        /// <summary>
        /// Creates an invoker bound to the given lifecycle events instance.
        /// </summary>
        internal Invoker(LifecycleEvents events)
        {
            this.events = events;
        }

        internal void NotifyHooked() =>
            InvokeHelper.InvokeList(events.onItemChangerHookSubscribers);

        internal void NotifyUnhooked() =>
            InvokeHelper.InvokeList(events.onItemChangerUnhookSubscribers);

        /// <summary>
        /// Signals that a new game is about to start.
        /// </summary>
        public void NotifyBeforeStartNewGame() =>
            InvokeHelper.InvokeList(events.beforeStartNewGameSubscribers);

        /// <summary>
        /// Signals that a new game has just started.
        /// </summary>
        public void NotifyAfterStartNewGame() =>
            InvokeHelper.InvokeList(events.afterStartNewGameSubscribers);

        /// <summary>
        /// Signals that an existing game is about to be continued.
        /// </summary>
        public void NotifyBeforeContinueGame() =>
            InvokeHelper.InvokeList(events.beforeContinueGameSubscribers);

        /// <summary>
        /// Signals that an existing game has been continued.
        /// </summary>
        public void NotifyAfterContinueGame() =>
            InvokeHelper.InvokeList(events.afterContinueGameSubscribers);

        /// <summary>
        /// Signals that the player is entering the game world.
        /// </summary>
        public void NotifyOnEnterGame() => InvokeHelper.InvokeList(events.onEnterGameSubscribers);

        /// <summary>
        /// Signals that it is safe to give items.
        /// </summary>
        public void NotifyOnSafeToGiveItems() =>
            InvokeHelper.InvokeList(events.onSafeToGiveItemsSubscribers);

        /// <summary>
        /// Signals that the player is leaving the game.
        /// </summary>
        public void NotifyOnLeaveGame() => InvokeHelper.InvokeList(events.onLeaveGameSubscribers);
    }
}
