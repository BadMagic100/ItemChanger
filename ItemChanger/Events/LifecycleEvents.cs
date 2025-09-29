using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ItemChanger.Events;

public static class LifecycleEvents
{

    /// <summary>
    /// Called after ItemChanger hooks, which occurs whenever an ItemChanger profile is created or loaded.
    /// </summary>
    public static event Action OnItemChangerHook { add => _OnItemChangerHookSubscribers.Add(value); remove => _OnItemChangerHookSubscribers.Remove(value); }
    private static readonly List<Action> _OnItemChangerHookSubscribers = [];

    /// <summary>
    /// Called after ItemChanger unhooks, which occurs whenever an ItemChanger profile is disposed.
    /// </summary>
    public static event Action OnItemChangerUnhook { add => _OnItemChangerUnhookSubscribers.Add(value); remove => _OnItemChangerUnhookSubscribers.Remove(value); }
    private static readonly List<Action> _OnItemChangerUnhookSubscribers = [];

    /// <summary>
    /// An event called before creating a new save file in the game (profiles are not yet loaded at this time).
    /// </summary>
    public static event Action BeforeStartNewGame { add => _BeforeStartNewGameSubscribers.Add(value); remove => _BeforeStartNewGameSubscribers.Remove(value); }
    private static readonly List<Action> _BeforeStartNewGameSubscribers = [];

    /// <summary>
    /// An event called after creating a new save file in the game
    /// </summary>
    public static event Action AfterStartNewGame { add => _AfterStartNewGameSubscribers.Add(value); remove => _AfterStartNewGameSubscribers.Remove(value); }
    private static readonly List<Action> _AfterStartNewGameSubscribers = [];

    /// <summary>
    /// An event called before loading in to an existing save file in the game (profiles are not yet loaded at this time).
    /// </summary>
    public static event Action BeforeContinueGame { add => _BeforeContinueGameSubscribers.Add(value); remove => _BeforeContinueGameSubscribers.Remove(value); }
    private static readonly List<Action> _BeforeContinueGameSubscribers = [];

    /// <summary>
    /// An event called after loading in to an existing save file.
    /// </summary>
    public static event Action AfterContinueGame { add => _AfterContinueGameSubscribers.Add(value); remove => _AfterContinueGameSubscribers.Remove(value); }
    private static readonly List<Action> _AfterContinueGameSubscribers = [];

    /// <summary>
    /// An event called just before loading into a new or existing save file. Happens after <see cref="BeforeStartNewGame"/>
    /// </summary>
    public static event Action OnEnterGame { add => _OnEnterGameSubscribers.Add(value); remove => _OnEnterGameSubscribers.Remove(value); }
    private static readonly List<Action> _OnEnterGameSubscribers = [];

    /// <summary>
    /// An event called after it is first safe to give items. Can be called any time after <see cref="OnEnterGame"/>.
    /// </summary>
    /// <remarks>
    /// This event is used by consumers to prevent partially-initialized game state from breaking give effects, so as a general
    /// rule of thumb, it is a good idea to invoke this only after the game is fully initialized (e.g., after complely loading in
    /// to the first gameplay scene).
    /// </remarks>
    public static event Action OnSafeToGiveItems { add => _OnSafeToGiveItemsSubscribers.Add(value); remove => _OnSafeToGiveItemsSubscribers.Remove(value); }
    private static readonly List<Action> _OnSafeToGiveItemsSubscribers = [];

    /// <summary>
    /// An event called as the game is about to exit, but before the profile unloads.
    /// </summary>
    public static event Action? OnLeaveGame { add => _OnLeaveGameSubscribers.Add(value); remove => _OnLeaveGameSubscribers.Remove(value); }
    private static readonly List<Action> _OnLeaveGameSubscribers = [];

    /// <summary>
    /// Used by the active <see cref="ItemChanger.Internal.IItemChangerHost"/> to wire up lifecycle events.
    /// It is the host's responsibility to invoke the events in the order specified by the documentation.
    /// </summary>
    public class Invoker
    {
        internal Invoker() { }

        internal static void InvokeList(List<Action> list, [CallerMemberName] string caller = "")
        {
            foreach (Action a in list)
            {
                try
                {
                    a?.Invoke();
                }
                catch (Exception e)
                {
                    LoggerProxy.LogError($"Error thrown by a subscriber during {caller}:\n{e}");
                }
            }
        }

        internal void NotifyHooked() => InvokeList(_OnItemChangerHookSubscribers);

        internal void NotifyUnhooked() => InvokeList(_OnItemChangerUnhookSubscribers);

        public void NotifyBeforeStartNewGame() => InvokeList(_BeforeStartNewGameSubscribers);

        public void NotifyAfterStartNewGame() => InvokeList(_AfterStartNewGameSubscribers);

        public void NotifyBeforeContinueGame() => InvokeList(_BeforeContinueGameSubscribers);

        public void NotifyAfterContinueGame() => InvokeList(_AfterContinueGameSubscribers);

        public void NotifyOnEnterGame() => InvokeList(_OnEnterGameSubscribers);

        public void NotifyOnSafeToGiveItems() => InvokeList(_OnSafeToGiveItemsSubscribers);

        public void NotifyOnLeaveGame() => InvokeList(_OnLeaveGameSubscribers);
    }
}
