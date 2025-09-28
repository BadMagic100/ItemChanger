using System;

namespace ItemChanger.Events;

public static class LifecycleEvents
{

    /// <summary>
    /// Called after ItemChanger hooks, which occurs whenever an ItemChanger profile is created or loaded.
    /// </summary>
    public static event Action? OnItemChangerHook;

    /// <summary>
    /// Called after ItemChanger unhooks, which occurs whenever an ItemChanger profile is disposed.
    /// </summary>
    public static event Action? OnItemChangerUnhook;

    /// <summary>
    /// An event called before creating a new save file in the game (profiles are not yet loaded at this time).
    /// </summary>
    public static event Action? BeforeStartNewGame;

    /// <summary>
    /// An event called after creating a new save file in the game
    /// </summary>
    public static event Action? AfterStartNewGame;

    /// <summary>
    /// An event called before loading in to an existing save file in the game (profiles are not yet loaded at this time).
    /// </summary>
    public static event Action? BeforeContinueGame;

    /// <summary>
    /// An event called after loading in to an existing save file.
    /// </summary>
    public static event Action? AfterContinueGame;

    /// <summary>
    /// An event called just before loading into a new or existing save file. Happens after <see cref="BeforeStartNewGame"/>
    /// </summary>
    public static event Action? OnEnterGame;

    /// <summary>
    /// An event called after it is first safe to give items. Can be called any time after <see cref="OnEnterGame"/>.
    /// </summary>
    /// <remarks>
    /// This event is used by consumers to prevent partially-initialized game state from breaking give effects, so as a general
    /// rule of thumb, it is a good idea to invoke this only after the game is fully initialized (e.g., after complely loading in
    /// to the first gameplay scene).
    /// </remarks>
    public static event Action? OnSafeToGiveItems;

    /// <summary>
    /// An event called as the game is about to exit, but before the profile unloads.
    /// </summary>
    public static event Action? OnLeaveGame;

    /// <summary>
    /// Used by the active <see cref="ItemChanger.Internal.IItemChangerHost"/> to wire up lifecycle events.
    /// It is the host's responsibility to invoke the events in the order specified by the documentation.
    /// </summary>
    public class Invoker
    {
        internal Invoker() { }

        internal void NotifyHooked() => OnItemChangerHook?.Invoke();

        internal void NotifyUnhooked() => OnItemChangerUnhook?.Invoke();

        public void NotifyBeforeStartNewGame() => BeforeStartNewGame?.Invoke();

        public void NotifyAfterStartNewGame() => AfterStartNewGame?.Invoke();

        public void NotifyBeforeContinueGame() => BeforeContinueGame?.Invoke();

        public void NotifyAfterContinueGame() => AfterContinueGame?.Invoke();

        public void NotifyOnEnterGame() => OnEnterGame?.Invoke();

        public void NotifyOnSafeToGiveItems() => OnSafeToGiveItems?.Invoke();

        public void NotifyOnLeaveGame() => OnLeaveGame?.Invoke();
    }
}
