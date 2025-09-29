using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace ItemChanger;

public static class GameEvents
{
    /// <summary>
    /// Called after persistent items reset.
    /// </summary>
    public static event Action OnPersistentUpdate { add => _OnPersistentUpdateSubscribers.Add(value); remove => _OnPersistentUpdateSubscribers.Remove(value); }
    private static readonly List<Action> _OnPersistentUpdateSubscribers = [];

    /// <summary>
    /// Called after semipersistent data resets. Semi-persistent resets occur less frequently than persistent resets, and
    /// are only triggered by certain events, such as resting.
    /// </summary>
    public static event Action OnSemiPersistentUpdate { add => _OnSemiPersistentUpdateSubscribers.Add(value); remove => _OnSemiPersistentUpdateSubscribers.Remove(value); }
    private static readonly List<Action> _OnSemiPersistentUpdateSubscribers = [];

    /// <summary>
    /// Called immediately prior to beginning a scene transition. If transition overrides take place through ItemChanger, 
    /// these are applied before the event is invoked.
    /// </summary>
    /// <remarks>
    /// This event only applies to games which have discrete scenes.
    /// </remarks>
    public static event Action<Transition> OnBeginSceneTransition { add => _OnBeginSceneTransitionSubscribers.Add(value); remove => _OnBeginSceneTransitionSubscribers.Remove(value); }
    private static readonly List<Action<Transition>> _OnBeginSceneTransitionSubscribers = [];

    /// <summary>
    /// Called whenever a new scene is loaded, including both additive scene loads and full scene transitions.
    /// </summary>
    public static event Action<Scene> OnNextSceneLoaded { add => _OnNextSceneLoadedSubscribers.Add(value); remove => _OnNextSceneLoadedSubscribers.Remove(value); }
    private static readonly List<Action<Scene>> _OnNextSceneLoadedSubscribers = [];

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
        foreach (Action<Scene> a in _OnNextSceneLoadedSubscribers)
        {
            try
            {
                a?.Invoke(to);
            }
            catch (Exception e)
            {
                LoggerProxy.LogError($"Error thrown by a global subscriber during {nameof(InvokeSceneLoadedEvent)} for scene {to.name}:\n{e}");
            }
        }
        if (sceneEdits.TryGetValue(to.name, out List<Action<Scene>>? list))
        {
            foreach (Action<Scene> a in _OnNextSceneLoadedSubscribers)
            {
                try
                {
                    a?.Invoke(to);
                }
                catch (Exception e)
                {
                    LoggerProxy.LogError($"Error thrown by a local subscriber during {nameof(InvokeSceneLoadedEvent)} for scene {to.name}:\n{e}");
                }
            }
        }
    }

    public class Invoker
    {
        internal Invoker() { }

        public void NotifyPersistentUpdate()
        {
            foreach (Action a in _OnPersistentUpdateSubscribers)
            {
                try
                {
                    a?.Invoke();
                }
                catch (Exception e)
                {
                    LoggerProxy.LogError($"Error thrown by a subscriber during {nameof(NotifyPersistentUpdate)}:\n{e}");
                }
            }
        }

        public void NotifySemiPersistentUpdate()
        {
            foreach (Action a in _OnSemiPersistentUpdateSubscribers)
            {
                try
                {
                    a?.Invoke();
                }
                catch (Exception e)
                {
                    LoggerProxy.LogError($"Error thrown by a subscriber during {nameof(NotifySemiPersistentUpdate)}:\n{e}");
                }
            }
        }

        public void NotifyOnBeginSceneTransition(Transition target)
        {
            foreach (Action<Transition> a in _OnBeginSceneTransitionSubscribers)
            {
                try
                {
                    a?.Invoke(target);
                }
                catch (Exception e)
                {
                    LoggerProxy.LogError($"Error thrown by a subscriber during {nameof(NotifyOnBeginSceneTransition)} for transition {target}:\n{e}");
                }
            }
        }
    }
}
