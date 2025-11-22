using System;
using System.Collections.Generic;
using ItemChanger.Enums;
using ItemChanger.Events.Args;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace ItemChanger.Events;

public sealed class GameEvents
{
    private readonly UnityAction<Scene, LoadSceneMode> sceneLoadedHandler;

    /// <summary>
    /// Initializes a new instance with its scene handlers configured.
    /// </summary>
    public GameEvents()
    {
        sceneLoadedHandler = (scene, _) => InvokeSceneLoadedEvent(scene);
    }

    /// <summary>
    /// Called after persistent items reset.
    /// </summary>
    public event Action OnPersistentUpdate
    {
        add => onPersistentUpdateSubscribers.Add(value);
        remove => onPersistentUpdateSubscribers.Remove(value);
    }
    private readonly List<Action> onPersistentUpdateSubscribers = [];

    /// <summary>
    /// Called after semipersistent data resets. Semi-persistent resets occur less frequently than persistent resets, and
    /// are only triggered by certain events, such as resting.
    /// </summary>
    public event Action OnSemiPersistentUpdate
    {
        add => onSemiPersistentUpdateSubscribers.Add(value);
        remove => onSemiPersistentUpdateSubscribers.Remove(value);
    }
    private readonly List<Action> onSemiPersistentUpdateSubscribers = [];

    /// <summary>
    /// Called immediately prior to loading a new scene, including both additive loads and full transitions.
    /// Hosts which modify which scene will be loaded must do so before invoking this event.
    /// </summary>
    public event Action<BeforeSceneLoadedEventArgs> BeforeNextSceneLoaded
    {
        add => beforeNextSceneLoadedSubscribers.Add(value);
        remove => beforeNextSceneLoadedSubscribers.Remove(value);
    }
    private readonly List<Action<BeforeSceneLoadedEventArgs>> beforeNextSceneLoadedSubscribers = [];

    /// <summary>
    /// Called whenever a new scene is loaded, including both additive scene loads and full scene transitions.
    /// </summary>
    public event Action<SceneLoadedEventArgs> OnNextSceneLoaded
    {
        add => onNextSceneLoadedSubscribers.Add(value);
        remove => onNextSceneLoadedSubscribers.Remove(value);
    }
    private readonly List<Action<SceneLoadedEventArgs>> onNextSceneLoadedSubscribers = [];

    /// <summary>
    /// Registers a scene edit to be invoked whenever sceneName is loaded.
    /// </summary>
    public void AddSceneEdit(string sceneName, Action<Scene> action)
    {
        if (sceneEdits.TryGetValue(sceneName, out List<Action<Scene>>? sceneSpecificEdits))
        {
            sceneSpecificEdits.Add(action);
        }
        else
        {
            sceneEdits[sceneName] = [action];
        }
    }

    /// <summary>
    /// Removes the action from the scene-specific active scene hook.
    /// </summary>
    public void RemoveSceneEdit(string sceneName, Action<Scene> action)
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

    private readonly Dictionary<string, List<Action<Scene>>> sceneEdits = [];

    internal static void Hook(GameEvents events)
    {
        SceneManager.sceneLoaded += events.sceneLoadedHandler;
    }

    internal static void Unhook(GameEvents events)
    {
        SceneManager.sceneLoaded -= events.sceneLoadedHandler;
    }

    private void InvokeSceneLoadedEvent(Scene to)
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
        private readonly ItemChangerProfile profile;
        private readonly GameEvents events;

        internal Invoker(ItemChangerProfile profile, GameEvents events)
        {
            this.profile = profile;
            this.events = events;
        }

        public void NotifyPersistentUpdate()
        {
            profile.ResetPersistentItems(Persistence.Persistent);
            InvokeHelper.InvokeList(events.onPersistentUpdateSubscribers);
        }

        public void NotifySemiPersistentUpdate()
        {
            profile.ResetPersistentItems(Persistence.SemiPersistent);
            InvokeHelper.InvokeList(events.onSemiPersistentUpdateSubscribers);
        }

        public void NotifyBeforeNextSceneLoaded(BeforeSceneLoadedEventArgs args) =>
            InvokeHelper.InvokeList(args, events.beforeNextSceneLoadedSubscribers);
    }
}
