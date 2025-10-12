using System;
using System.Collections.Generic;

namespace ItemChanger.Events;

public static class LifecycleEvents
{
    /// <summary>
    /// Called after ItemChanger hooks, which occurs whenever an ItemChanger profile is created or loaded.
    /// </summary>
    public static event Action OnItemChangerHook
    {
        add => onItemChangerHookSubscribers.Add(value);
        remove => onItemChangerHookSubscribers.Remove(value);
    }
    private static readonly List<Action> onItemChangerHookSubscribers = [];

    /// <summary>
    /// Called after ItemChanger unhooks, which occurs whenever an ItemChanger profile is disposed.
    /// </summary>
    public static event Action OnItemChangerUnhook
    {
        add => onItemChangerUnhookSubscribers.Add(value);
        remove => onItemChangerUnhookSubscribers.Remove(value);
    }
    private static readonly List<Action> onItemChangerUnhookSubscribers = [];

    /// <summary>
    /// An event called before creating a new save file in the game (profiles are not yet loaded at this time).
    /// </summary>
    public static event Action BeforeStartNewGame
    {
        add => beforeStartNewGameSubscribers.Add(value);
        remove => beforeStartNewGameSubscribers.Remove(value);
    }
    private static readonly List<Action> beforeStartNewGameSubscribers = [];

    /// <summary>
    /// An event called after creating a new save file in the game
    /// </summary>
    public static event Action AfterStartNewGame
    {
        add => afterStartNewGameSubscribers.Add(value);
        remove => afterStartNewGameSubscribers.Remove(value);
    }
    private static readonly List<Action> afterStartNewGameSubscribers = [];

    /// <summary>
    /// An event called before loading in to an existing save file in the game (profiles are not yet loaded at this time).
    /// </summary>
    public static event Action BeforeContinueGame
    {
        add => beforeContinueGameSubscribers.Add(value);
        remove => beforeContinueGameSubscribers.Remove(value);
    }
    private static readonly List<Action> beforeContinueGameSubscribers = [];

    /// <summary>
    /// An event called after loading in to an existing save file.
    /// </summary>
    public static event Action AfterContinueGame
    {
        add => afterContinueGameSubscribers.Add(value);
        remove => afterContinueGameSubscribers.Remove(value);
    }
    private static readonly List<Action> afterContinueGameSubscribers = [];

    /// <summary>
    /// An event called just before loading into a new or existing save file. Happens after <see cref="BeforeStartNewGame"/>
    /// </summary>
    public static event Action OnEnterGame
    {
        add => onEnterGameSubscribers.Add(value);
        remove => onEnterGameSubscribers.Remove(value);
    }
    private static readonly List<Action> onEnterGameSubscribers = [];

    /// <summary>
    /// An event called after it is first safe to give items. Can be called any time after <see cref="OnEnterGame"/>.
    /// </summary>
    /// <remarks>
    /// This event is used by consumers to prevent partially-initialized game state from breaking give effects, so as a general
    /// rule of thumb, it is a good idea to invoke this only after the game is fully initialized (e.g., after complely loading in
    /// to the first gameplay scene).
    /// </remarks>
    public static event Action OnSafeToGiveItems
    {
        add => onSafeToGiveItemsSubscribers.Add(value);
        remove => onSafeToGiveItemsSubscribers.Remove(value);
    }
    private static readonly List<Action> onSafeToGiveItemsSubscribers = [];

    /// <summary>
    /// An event called as the game is about to exit, but before the profile unloads.
    /// </summary>
    public static event Action? OnLeaveGame
    {
        add => onLeaveGameSubscribers.Add(value);
        remove => onLeaveGameSubscribers.Remove(value);
    }
    private static readonly List<Action> onLeaveGameSubscribers = [];

    /// <summary>
    /// Used by the active <see cref="ItemChanger.ItemChangerHost"/> to wire up lifecycle events.
    /// It is the host's responsibility to invoke the events in the order specified by the documentation.
    /// </summary>
    public class Invoker
    {
        internal Invoker() { }

        internal void NotifyHooked() => InvokeHelper.InvokeList(onItemChangerHookSubscribers);

        internal void NotifyUnhooked() => InvokeHelper.InvokeList(onItemChangerUnhookSubscribers);

        public void NotifyBeforeStartNewGame() =>
            InvokeHelper.InvokeList(beforeStartNewGameSubscribers);

        public void NotifyAfterStartNewGame() =>
            InvokeHelper.InvokeList(afterStartNewGameSubscribers);

        public void NotifyBeforeContinueGame() =>
            InvokeHelper.InvokeList(beforeContinueGameSubscribers);

        public void NotifyAfterContinueGame() =>
            InvokeHelper.InvokeList(afterContinueGameSubscribers);

        public void NotifyOnEnterGame() => InvokeHelper.InvokeList(onEnterGameSubscribers);

        public void NotifyOnSafeToGiveItems() =>
            InvokeHelper.InvokeList(onSafeToGiveItemsSubscribers);

        public void NotifyOnLeaveGame() => InvokeHelper.InvokeList(onLeaveGameSubscribers);
    }
}
