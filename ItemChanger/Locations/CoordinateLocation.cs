using ItemChanger.Containers;
using ItemChanger.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ItemChanger.Locations;

/// <summary>
/// Location which places a container at a specified coordinate position.
/// </summary>
public class CoordinateLocation : PlaceableLocation
{
    public required float X { get; init; }
    public required float Y { get; init; }
    public required float Z { get; init; }

    protected override void DoLoad()
    {
        GameEvents.AddSceneEdit(UnsafeSceneName, OnActiveSceneChanged);
    }

    protected override void DoUnload()
    {
        GameEvents.RemoveSceneEdit(UnsafeSceneName, OnActiveSceneChanged);
    }

    protected void OnActiveSceneChanged(Scene to)
    {
        if (!Managed && to.name == UnsafeSceneName)
        {
            base.GetContainer(out Container container, out ContainerInfo info);
            PlaceContainer(container, info);
        }
    }

    /// <inheritdoc/>
    public override void PlaceContainer(Container container, ContainerInfo info)
    {
        GameObject obj = container.GetNewContainer(info);
        container.ApplyTargetContext(obj, new Vector3(X, Y, Z), Vector3.zero);
        if (!obj.activeSelf)
        {
            obj.SetActive(true);
        }
    }
}
