using System;

namespace ItemChanger.Events.Args;

/// <summary>
/// Provides data for the event that occurs before a scene is loaded.
/// </summary>
/// <param name="targetScene">The name of the scene that will be loaded.</param>
public class BeforeSceneLoadedEventArgs(string targetScene) : EventArgs
{
    /// <summary>
    /// The name of the scene that will be loaded.
    /// </summary>
    public string TargetScene => targetScene;
}
