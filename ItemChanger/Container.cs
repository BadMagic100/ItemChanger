﻿using System;
using System.Collections.Generic;

namespace ItemChanger;

/// <summary>
/// Base class for types which implement creating and fsm-editing item containers.
/// </summary>
public abstract class Container
{
    public const string Unknown = "Unknown";
    public const string GenericPickup = "GenericPickup";

    /// <summary>
    /// Gets the container definition for the given string. Returns null if no such container has been defined.
    /// </summary>
    public static Container? GetContainer(string containerType)
    {
        if (string.IsNullOrEmpty(containerType))
        {
            return null;
        }

        if (_containers.TryGetValue(containerType, out Container value))
        {
            return value;
        }

        return null;
    }

    /// <summary>
    /// Adds or overwrites the container definition in the internal dictionary.
    /// </summary>
    public static void DefineContainer(Container container)
    {
        _containers[container.Name] = container;
    }

    /// <summary>
    /// Adds or overwrites the container definition in the internal dictionary.
    /// </summary>
    public static Container DefineContainer<T>() where T : Container, new()
    {
        Container c = new T();
        return _containers[c.Name] = c;
    }


    private readonly static Dictionary<string, Container> _containers = new();

    public static bool SupportsAll(string containerName, bool mustSupportInstantiate, bool mustSupportCost, bool mustSupportSceneChange)
    {
        return GetContainer(containerName) is Container c
            && (!mustSupportInstantiate || c.SupportsInstantiate)
            && (!mustSupportCost || c.SupportsCost)
            && (!mustSupportSceneChange || c.SupportsSceneChange);
    }

    /// <summary>
    /// The unique name of the container.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Returns true if the container has a natural way to present a cost.
    /// For example, Container.Shiny supports cost through the YN dialogue box.
    /// </summary>
    public virtual bool SupportsCost => false;

    /// <summary>
    /// Returns true if the container has a natural way to start a scene transition after giving items.
    /// </summary>
    public virtual bool SupportsSceneChange => false;

    /// <summary>
    /// Returns true if the container has a natural way to be dropped from midair.
    /// </summary>
    public virtual bool SupportsDrop => false;

    /// <summary>
    /// Returns true if the container can be instantiated. For some containers, this value is variable in which objects were preloaded.
    /// </summary>
    public virtual bool SupportsInstantiate => false;

    /// <summary>
    /// Produces a new object of this container type.
    /// </summary>
    /// <exception cref="NotImplementedException">The container does not support instantiation.</exception>
    public virtual GameObject GetNewContainer(ContainerInfo info)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Puts the container in the same position and hierarchy of the target, up to the elevation correction in y.
    /// </summary>
    public virtual void ApplyTargetContext(GameObject obj, GameObject target, float elevation)
    {
        if (target.transform.parent != null)
        {
            obj.transform.SetParent(target.transform.parent);
        }

        obj.transform.position = target.transform.position;
        obj.transform.localPosition = target.transform.localPosition;
        obj.transform.Translate(new(0, -elevation));
        obj.SetActive(target.activeSelf);
        obj.transform.SetPositionZ(0);
    }

    /// <summary>
    /// Puts the container in the specified position, up to the elevation correction in y.
    /// </summary>
    public virtual void ApplyTargetContext(GameObject obj, float x, float y, float elevation)
    {
        obj.transform.position = new Vector2(x, y - elevation);
        obj.SetActive(true);
    }

    /// <summary>
    /// Fsm hook for all container edits. Called on an item with the ContainerInfo component at the start and end of ItemChanger's PlayMakerFSM.OnEnable hook.
    /// </summary>
    public static void OnEnable(PlayMakerFSM fsm)
    {
        ContainerInfo? info = ContainerInfo.FindContainerInfo(fsm.gameObject);
        if (info != null)
        {
            var container = GetContainer(info.ContainerType);
            if (container == null)
            {
                LogError($"Unable to find Container definition for {info.ContainerType}!");
                return;
            }

            var give = info.GiveInfo;
            var scene = info.ChangeSceneInfo;
            var cost = info.CostInfo;

            if (give != null && !give.Applied)
            {
                container.AddGiveEffectToFsm(fsm, give);
                give.Applied = true;
            }

            if (scene != null && !scene.applied)
            {
                container.AddChangeSceneToFsm(fsm, scene);
                scene.applied = true;
            }

            if (cost != null && !cost.Applied)
            {
                container.AddCostToFsm(fsm, cost);
                cost.Applied = true;
            }
        }
    }

    /// <summary>
    /// Called during the container fsm hook to set up the container to give items.
    /// </summary>
    public virtual void AddGiveEffectToFsm(PlayMakerFSM fsm, ContainerGiveInfo info) { }

    /// <summary>
    /// Called during the container fsm hook to set up the container to change scene.
    /// </summary>
    public virtual void AddChangeSceneToFsm(PlayMakerFSM fsm, ChangeSceneInfo info) { }

    /// <summary>
    /// Called during the container fsm hook to set up the container to have a cost.
    /// </summary>
    public virtual void AddCostToFsm(PlayMakerFSM fsm, CostInfo info) { }
}
