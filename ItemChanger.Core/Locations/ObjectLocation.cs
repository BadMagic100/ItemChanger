using System;
using ItemChanger.Containers;
using ItemChanger.Extensions;
using ItemChanger.Tags;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ItemChanger.Locations;

public class ObjectLocation : ContainerLocation, IReplaceableLocation
{
    public required string ObjectName { get; init; }
    public required Vector3 Correction { get; init; }

    protected override void DoLoad()
    {
        ItemChangerHost.Singleton.GameEvents.AddSceneEdit(UnsafeSceneName, OnSceneLoaded);
    }

    protected override void DoUnload()
    {
        ItemChangerHost.Singleton.GameEvents.RemoveSceneEdit(UnsafeSceneName, OnSceneLoaded);
    }

    protected virtual void OnSceneLoaded(Scene to)
    {
        base.GetContainer(out Container container, out ContainerInfo info);
        if (container.Name == OriginalContainerType && container.SupportsModifyInPlace)
        {
            ModifyContainerInPlace(to, container, info);
        }
        else
        {
            ReplaceWithContainer(to, container, info);
        }
    }

    protected virtual void ModifyContainerInPlace(
        Scene scene,
        Container container,
        ContainerInfo info
    )
    {
        GameObject target = FindObject(scene, ObjectName);
        container.ModifyContainerInPlace(target, info);
    }

    public virtual void ReplaceWithContainer(Scene scene, Container container, ContainerInfo info)
    {
        GameObject target = FindObject(scene, ObjectName);
        GameObject newContainer = container.GetNewContainer(info);
        container.ApplyTargetContext(newContainer, target, Correction);
        UnityEngine.Object.Destroy(target);
        foreach (IActionOnContainerReplaceTag tag in GetTags<IActionOnContainerReplaceTag>())
        {
            tag.OnReplace(scene);
        }
    }

    protected static GameObject FindObject(Scene scene, string objectName)
    {
        GameObject? candidate;
        if (!objectName.Contains("/"))
        {
            candidate = scene.FindGameObjectByName(objectName);
        }
        else
        {
            candidate = scene.FindGameObject(objectName);
        }

        if (candidate == null)
        {
            throw new ArgumentException($"{objectName} does not exist in {scene.name}");
        }
        return candidate;
    }
}
