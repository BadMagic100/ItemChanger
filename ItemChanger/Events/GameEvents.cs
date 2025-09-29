using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace ItemChanger.Events;

public static class GameEvents
{
    /// <summary>
    /// Called after persistent items reset.
    /// </summary>
    public static event Action OnPersistentUpdate { add => onPersistentUpdateSubscribers.Add(value); remove => onPersistentUpdateSubscribers.Remove(value); }
    private static readonly List<Action> onPersistentUpdateSubscribers = [];

    /// <summary>
    /// Called after semipersistent data resets. Semi-persistent resets occur less frequently than persistent resets, and
    /// are only triggered by certain events, such as resting.
    /// </summary>
    public static event Action OnSemiPersistentUpdate { add => onSemiPersistentUpdateSubscribers.Add(value); remove => onSemiPersistentUpdateSubscribers.Remove(value); }
    private static readonly List<Action> onSemiPersistentUpdateSubscribers = [];

    /// <summary>
    /// Called immediately prior to beginning a scene transition. If transition overrides take place through ItemChanger, 
    /// these are applied before the event is invoked.
    /// </summary>
    /// <remarks>
    /// This event only applies to games which have discrete scenes.
    /// </remarks>
    public static event Action<Transition> OnBeginSceneTransition { add => onBeginSceneTransitionSubscribers.Add(value); remove => onBeginSceneTransitionSubscribers.Remove(value); }
    private static readonly List<Action<Transition>> onBeginSceneTransitionSubscribers = [];

    /// <summary>
    /// Called whenever a new scene is loaded, including both additive scene loads and full scene transitions.
    /// </summary>
    public static event Action<Scene> OnNextSceneLoaded { add => onNextSceneLoadedSubscribers.Add(value); remove => onNextSceneLoadedSubscribers.Remove(value); }
    private static readonly List<Action<Scene>> onNextSceneLoadedSubscribers = [];

    /// <summary>
    /// Registers a scene edit to be invoked whenever sceneName is loaded.
    /// </summary>
    public static void AddSceneEdit(string sceneName, Action<Scene> action)
    {
        if (sceneEdits.ContainsKey(sceneName))
        {
            sceneEdits[sceneName].Add(action);
        }
        else
        {
            sceneEdits[sceneName] = [action];
        }
    }

    /// <summary>
    /// Removes the action from the scene-specific active scene hook.
    /// </summary>
    public static void RemoveSceneChangeEdit(string sceneName, Action<Scene> action)
    {
        if (sceneEdits.TryGetValue(sceneName, out List<Action<Scene>>? list))
        {
            list.Remove(action);
        }
    }

    /*
     *************************************************************************************
     Public API above. Implementations below.
     *************************************************************************************
    */

    private static readonly Dictionary<string, List<Action<Scene>>> sceneEdits = [];

    internal static void Hook()
    {
        SceneManager.sceneLoaded += InvokeSceneLoadedEvent;
    }

    internal static void Unhook()
    {
        SceneManager.sceneLoaded -= InvokeSceneLoadedEvent;
    }

    private static void InvokeSceneLoadedEvent(Scene to, LoadSceneMode _)
    {
        InvokeHelper.InvokeList(to, onNextSceneLoadedSubscribers);
        if (sceneEdits.TryGetValue(to.name, out List<Action<Scene>>? list))
        {
            InvokeHelper.InvokeList(to, list);
        }
    }

    public class Invoker
    {
        internal Invoker() { }

        public void NotifyPersistentUpdate() => InvokeHelper.InvokeList(onPersistentUpdateSubscribers);

        public void NotifySemiPersistentUpdate() => InvokeHelper.InvokeList(onSemiPersistentUpdateSubscribers);

        public void NotifyOnBeginSceneTransition(Transition target) => InvokeHelper.InvokeList(target, onBeginSceneTransitionSubscribers);
    }
}
