using ItemChanger.Extensions;
using ItemChanger.Tags.Constraints;
using UnityEngine.SceneManagement;

namespace ItemChanger.Tags;

/// <summary>
/// Destroys a game object in the same scene as the attached location when it is replaced with a container.
/// </summary>
[LocationTag]
public class DestroyOnContainerReplaceTag : Tag, IActionOnContainerReplaceTag
{
    /// <summary>
    /// The path to the object to be destroyed
    /// </summary>
    public required string ObjectPath { get; init; }

    /// <inheritdoc/>
    public void OnReplace(Scene scene)
    {
        UnityEngine.Object.Destroy(scene.FindGameObject(ObjectPath));
    }
}
