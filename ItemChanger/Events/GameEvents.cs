using System;
using System.Collections.Generic;
using ItemChanger.Events.Args;
using UnityEngine.SceneManagement;

namespace ItemChanger.Events;

public static class GameEvents
{
    /// <summary>
    /// Called after persistent items reset.
    /// </summary>
    public static event Action OnPersistentUpdate
    {
        add => onPersistentUpdateSubscribers.Add(value);
        remove => onPersistentUpdateSubscribers.Remove(value);
    }
    private static readonly List<Action> onPersistentUpdateSubscribers = [];

    /// <summary>
    /// Called after semipersistent data resets. Semi-persistent resets occur less frequently than persistent resets, and
    /// are only triggered by certain events, such as resting.
    /// </summary>
    public static event Action OnSemiPersistentUpdate
    {
        add => onSemiPersistentUpdateSubscribers.Add(value);
        remove => onSemiPersistentUpdateSubscribers.Remove(value);
    }
    private static readonly List<Action> onSemiPersistentUpdateSubscribers = [];

    /// <summary>
    /// Called immediately prior to loading a new scene, including both additive loads and full transitions.
    /// Hosts which modify which scene will be loaded must do so before invoking this event.
    /// </summary>
    public static event Action<BeforeSceneLoadedEventArgs> BeforeNextSceneLoaded
    {
        add => beforeNextSceneLoadedSubscribers.Add(value);
        remove => beforeNextSceneLoadedSubscribers.Remove(value);
    }
    private static readonly List<
        Action<BeforeSceneLoadedEventArgs>
    > beforeNextSceneLoadedSubscribers = [];

    /// <summary>
    /// Called whenever a new scene is loaded, including both additive scene loads and full scene transitions.
    /// </summary>
    public static event Action<SceneLoadedEventArgs> OnNextSceneLoaded
    {
        add => onNextSceneLoadedSubscribers.Add(value);
        remove => onNextSceneLoadedSubscribers.Remove(value);
    }
    private static readonly List<Action<SceneLoadedEventArgs>> onNextSceneLoadedSubscribers = [];

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
    public static void RemoveSceneEdit(string sceneName, Action<Scene> action)
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
        SceneLoadedEventArgs args = new SceneLoadedEventArgs(to);
        InvokeHelper.InvokeList(args, onNextSceneLoadedSubscribers);
        if (sceneEdits.TryGetValue(to.name, out List<Action<Scene>>? list))
        {
            InvokeHelper.InvokeList(to, list);
        }
    }

    public class Invoker
    {
        internal Invoker() { }

        public void NotifyPersistentUpdate() =>
            InvokeHelper.InvokeList(onPersistentUpdateSubscribers);

        public void NotifySemiPersistentUpdate() =>
            InvokeHelper.InvokeList(onSemiPersistentUpdateSubscribers);

        public void NotifyBeforeNextSceneLoaded(BeforeSceneLoadedEventArgs args) =>
            InvokeHelper.InvokeList(args, beforeNextSceneLoadedSubscribers);
    }
}
