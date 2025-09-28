using ItemChanger.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace ItemChanger;

public static class GameEvents
{
    /// <summary>
    /// Called after persistent items reset.
    /// </summary>
    public static event Action? OnPersistentUpdate;

    /// <summary>
    /// Called after semipersistent data resets. Semi-persistent resets occur less frequently than persistent resets, and
    /// are only triggered by certain events, such as resting.
    /// </summary>
    public static event Action? OnSemiPersistentUpdate;

    /// <summary>
    /// Called immediately prior to beginning a scene transition. If transition overrides take place through ItemChanger, 
    /// these are applied before the event is invoked.
    /// </summary>
    /// <remarks>
    /// This event only applies to games which have discrete scenes.
    /// </remarks>
    public static event Action<Transition>? OnBeginSceneTransition;

    /// <summary>
    /// Called whenever a new scene is loaded, including both additive scene loads and full scene transitions.
    /// </summary>
    public static event Action<Scene>? OnNextSceneLoaded;

    /// <summary>
    /// Registers a scene edit to be invoked whenever sceneName is loaded.
    /// </summary>
    public static void AddSceneEdit(string sceneName, Action<Scene> action)
    {
        if (sceneEdits.ContainsKey(sceneName))
        {
            sceneEdits[sceneName] += action;
        }
        else
        {
            sceneEdits[sceneName] = action;
        }
    }

    /// <summary>
    /// Removes the action from the scene-specific active scene hook.
    /// </summary>
    public static void RemoveSceneChangeEdit(string sceneName, Action<Scene> action)
    {
        if (sceneEdits.ContainsKey(sceneName))
        {
            sceneEdits[sceneName] -= action;
        }
    }

    /*
     *************************************************************************************
     Public API above. Implementations below.
     *************************************************************************************
    */

    private static readonly Dictionary<string, Action<Scene>?> sceneEdits = new();

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
        OnNextSceneLoaded?.Invoke(to);
        sceneEdits.GetOrDefault(to.name)?.Invoke(to);
    }

    public class Invoker
    {
        internal Invoker() { }

        public void NotifyPersistentUpdate() => OnPersistentUpdate?.Invoke();

        public void NotifySemiPersistenUpdate() => OnSemiPersistentUpdate?.Invoke();

        public void NotifyOnBeginSceneTransition(Transition target) => OnBeginSceneTransition?.Invoke(target);
    }
}
