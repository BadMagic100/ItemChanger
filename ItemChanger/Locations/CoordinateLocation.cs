using ItemChanger.Events;
using ItemChanger.Internal;
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
        GameEvents.RemoveSceneChangeEdit(UnsafeSceneName, OnActiveSceneChanged);
    }

    public void OnActiveSceneChanged(Scene to)
    {
        if (!Managed && to.name == UnsafeSceneName)
        {
            base.GetContainer(out GameObject obj, out string containerType);
            PlaceContainer(obj, containerType);
        }
    }

    public override void PlaceContainer(GameObject obj, string containerType)
    {
        ItemChangerHost.Singleton.ContainerRegistry.GetContainer(containerType)!
            .ApplyTargetContext(obj, new Vector3(X, Y, Z), Vector3.zero);
        if (!obj.activeSelf)
        {
            obj.SetActive(true);
        }
    }
}
