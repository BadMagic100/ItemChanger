using System;
using UnityEngine.SceneManagement;

namespace ItemChanger.Events.Args;

/// <summary>
/// Provides data for the event that is raised when a scene is loaded.
/// </summary>
/// <param name="scene">The scene that was loaded.</param>
public class SceneLoadedEventArgs(Scene scene) : EventArgs
{
    /// <summary>
    /// The scene the was loaded.
    /// </summary>
    public Scene Scene { get; } = scene;
}
