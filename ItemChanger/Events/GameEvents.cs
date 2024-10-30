using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace ItemChanger;

/// <summary>
/// The main class in ItemChanger for organizing events. Some specific events are defined in AbstractPlacement and AbstractItem instead.
/// </summary>
public static class GameEvents
{
    /// <summary>
    /// Called immediately prior to the BeginSceneTransition routine. If transition overrides take place through ItemChanger, these are applied before the event is invoked.
    /// </summary>
    public static event Action<Transition>? OnBeginSceneTransition;

    /// <summary>
    /// Called before GameManager.StartNewGame.
    /// </summary>
    public static event Action? BeforeStartNewGame;

    /// <summary>
    /// Called after GameManager.StartNewGame.
    /// </summary>
    public static event Action? AfterStartNewGame;

    /// <summary>
    /// Called after ItemChanger hooks, which occurs either when ItemChanger settings are created or when ItemChanger settings are loaded from a save file.
    /// </summary>
    public static event Action? OnItemChangerHook;

    /// <summary>
    /// Called after ItemChanger unhooks, which occurs when ItemChanger settings are nulled on returning to menu.
    /// </summary>
    public static event Action? OnItemChangerUnhook;

    /// <summary>
    /// Called on starting or continuing a save.
    /// <br/>If continuing or starting with a custom start, it is called before GM.ContinueGame.
    /// <br/>If starting with the base start, it is called before GM.StartNewGame.
    /// </summary>
    public static event Action? OnEnterGame;

    /// <summary>
    /// Called after persistent items reset, on every active scene change.
    /// </summary>
    public static event Action? OnPersistentUpdate;

    /// <summary>
    /// Called on every active scene change with the new scene as parameter.
    /// </summary>
    public static event Action<Scene>? OnSceneChange;

    /// <summary>
    /// Called after semipersistent data resets, i.e. on bench, death, special cutscenes, etc.
    /// </summary>
    public static event Action? OnSemiPersistentUpdate;

    /// <summary>
    /// The action will be invoked whenever sceneName becomes the name of the active scene.
    /// </summary>
    public static void AddSceneChangeEdit(string sceneName, Action<Scene> action)
    {
        if (activeSceneChangeEdits.ContainsKey(sceneName))
        {
            activeSceneChangeEdits[sceneName] += action;
        }
        else
        {
            activeSceneChangeEdits[sceneName] = action;
        }
    }

    /// <summary>
    /// Removes the action from the scene-specific active scene hook.
    /// </summary>
    public static void RemoveSceneChangeEdit(string sceneName, Action<Scene> action)
    {
        if (activeSceneChangeEdits.ContainsKey(sceneName))
        {
            activeSceneChangeEdits[sceneName] -= action;
        }
    }

    /*
     *************************************************************************************
     Public API above. Implementations below.
     *************************************************************************************
    */

    private static readonly Dictionary<string, Action<Scene>?> activeSceneChangeEdits = new();

    //internal static void Hook()
    //{
    //    UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnActiveSceneChanged;
    //    On.GameManager.StartNewGame += BeforeStartNewGameHook;
    //    On.GameManager.ContinueGame += OnContinueGame;
    //    On.GameManager.ResetSemiPersistentItems += OnResetSemiPersistentItems;
    //    try
    //    {
    //        OnItemChangerHook?.Invoke();
    //    }
    //    catch (Exception e)
    //    {
    //        LogHelper.LogError("Error invoking OnItemChangerHook:\n" + e);
    //    }
    //}

    //internal static void Unhook()
    //{
    //    UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    //    On.GameManager.StartNewGame -= BeforeStartNewGameHook;
    //    On.GameManager.ContinueGame -= OnContinueGame;
    //    On.GameManager.ResetSemiPersistentItems -= OnResetSemiPersistentItems;
    //    try
    //    {
    //        OnItemChangerUnhook?.Invoke();
    //    }
    //    catch (Exception e)
    //    {
    //        LogHelper.LogError("Error invoking OnItemChangerUnhook:\n" + e);
    //    }
    //}

    //private static void OnActiveSceneChanged(Scene from, Scene to)
    //{
    //    if (Ref.Settings == null)
    //    {
    //        return; // Settings is nulled by the API on active scene change to Menu_Title.
    //    }

    //    try
    //    {
    //        Ref.Settings.ResetPersistentItems();
    //        OnPersistentUpdate?.Invoke();
    //    }
    //    catch (Exception e)
    //    {
    //        LogHelper.LogError($"Error during persistent update leaving {from.name} and entering {to.name}:\n{e}");
    //    }

    //    try
    //    {
    //        OnSceneChange?.Invoke(to);
    //    }
    //    catch (Exception e)
    //    {
    //        LogHelper.LogError($"Error during Events.OnSceneChange:\n{e}");
    //    }

    //    try
    //    {
    //        activeSceneChangeEdits?.GetOrDefault(to.name)?.Invoke(to);
    //    }
    //    catch (Exception e)
    //    {
    //        LogHelper.LogError($"Error during local activeSceneChangeEdits leaving {from.name} and entering {to.name}:\n{e}");
    //    }
    //}

    //private static void OnResetSemiPersistentItems(On.GameManager.orig_ResetSemiPersistentItems orig, GameManager self)
    //{
    //    Ref.Settings.ResetSemiPersistentItems();
    //    try
    //    {
    //        OnSemiPersistentUpdate?.Invoke();
    //    }
    //    catch (Exception e)
    //    {
    //        LogHelper.LogError("Error invoking OnSemiPersistentUpdate:\n" + e);
    //    }

    //    orig(self);
    //}

    //private static void DoOnEnterGame()
    //{
    //    try
    //    {
    //        Ref.Settings.Load();
    //    }
    //    catch (Exception e)
    //    {
    //        LogHelper.LogError($"Error loading settings:\n{e}");
    //    }

    //    try
    //    {
    //        OnEnterGame?.Invoke();
    //    }
    //    catch (Exception e)
    //    {
    //        LogHelper.LogError($"Error during Events.OnEnterGame:\n{e}");
    //    }
    //}

    //private static void BeforeStartNewGameHook(On.GameManager.orig_StartNewGame orig, GameManager self, bool permadeathMode, bool bossRushMode)
    //{
    //    try
    //    {
    //        BeforeStartNewGame?.Invoke();
    //    }
    //    catch (Exception e)
    //    {
    //        LogHelper.LogError($"Error in BeforeStartNewGame event:\n{e}");
    //        throw;
    //    }

    //    if (Ref.Settings.Start != null)
    //    {
    //        if (permadeathMode)
    //        {
    //            self.playerData.permadeathMode = 1;
    //        }

    //        Ref.Settings.Start.ApplyToPlayerData(self.playerData);
    //        try
    //        {
    //            typeof(Modding.ModHooks).GetMethod("OnNewGame", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
    //            .Invoke(null, Array.Empty<object>());
    //        }
    //        catch (Exception e)
    //        {
    //            LogHelper.LogError($"Error invoking ModHooks.OnNewGame via reflection:\n{e}");
    //        }

    //        self.ContinueGame();
    //    }
    //    else
    //    {
    //        DoOnEnterGame();
    //        orig(self, permadeathMode, bossRushMode);
    //    }

    //    try
    //    {
    //        AfterStartNewGame?.Invoke();
    //    }
    //    catch (Exception e)
    //    {
    //        LogHelper.LogError($"Error in AfterStartNewGame event:\n{e}");
    //        throw;
    //    }
    //}

    //private static void OnContinueGame(On.GameManager.orig_ContinueGame orig, GameManager self)
    //{
    //    DoOnEnterGame();
    //    orig(self);
    //}
}
