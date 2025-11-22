using ItemChanger.Containers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ItemChanger.Locations;

/// <summary>
/// Location which places a container at a specified coordinate position.
/// </summary>
public class CoordinateLocation : PlaceableLocation
{
    /// <summary>World-space X coordinate for placement.</summary>
    public required float X { get; init; }

    /// <summary>World-space Y coordinate for placement.</summary>
    public required float Y { get; init; }

    /// <summary>World-space Z coordinate for placement.</summary>
    public float Z { get; init; }

    /// <summary>
    /// Hooks the scene edit that handles placement.
    /// </summary>
    protected override void DoLoad()
    {
        ItemChangerHost.Singleton.GameEvents.AddSceneEdit(UnsafeSceneName, OnActiveSceneChanged);
    }

    /// <summary>
    /// Removes the placement scene edit.
    /// </summary>
    protected override void DoUnload()
    {
        ItemChangerHost.Singleton.GameEvents.RemoveSceneEdit(UnsafeSceneName, OnActiveSceneChanged);
    }

    /// <summary>
    /// Places a container when the target scene becomes active and the location is unmanaged.
    /// </summary>
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
