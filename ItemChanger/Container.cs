﻿using ItemChanger.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemChanger.Containers;

namespace ItemChanger
{
    /// <summary>
    /// Base class for types which implement creating and fsm-editing item containers.
    /// </summary>
    public abstract class Container
    {
        static Container()
        {
            ResetContainers();
        }

        public const string Unknown = "Unknown";
        public const string Shiny = "Shiny";
        public const string GrubJar = "GrubJar";
        public const string GeoRock = "GeoRock";
        public const string Chest = "Chest";
        public const string Tablet = "Tablet";
        public const string Shop = "Shop";
        public const string Enemy = "Enemy";
        public const string Mimic = "Mimic";

        /// <summary>
        /// Gets the container definition for the given string. Returns null if no such container has been defined.
        /// </summary>
        public static Container GetContainer(string containerType)
        {
            if (string.IsNullOrEmpty(containerType)) return null;
            if (_containers.TryGetValue(containerType, out Container value)) return value;
            return null;
        }

        /// <summary>
        /// Adds or overwrites the container definition in the internal dictionary.
        /// </summary>
        public static void DefineContainer(string containerType, Container container)
        {
            _containers[containerType] = container;
        }

        private static Dictionary<string, Container> _containers;
        internal static void ResetContainers()
        {
            _containers = new Dictionary<string, Container>
            {
                { Shiny, new ShinyContainer() },
                { GrubJar, new GrubJarContainer() },
                { GeoRock, new GeoRockContainer() },
                { Chest, new ChestContainer() },
                { Tablet, new TabletContainer() },
                { Mimic, new MimicContainer() }
            };
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
        public abstract GameObject GetNewContainer(AbstractPlacement placement, IEnumerable<AbstractItem> items, FlingType flingType, Cost cost = null);

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
        }

        /// <summary>
        /// Fsm hook for all container edits. Called on an item with the ContainerInfo component at the start and end of ItemChanger's PlayMakerFSM.OnEnable hook.
        /// </summary>
        public static void OnEnable(PlayMakerFSM fsm)
        {
            var info = fsm.gameObject.GetComponent<ContainerInfo>();
            if (info != null)
            {
                var container = GetContainer(info.containerType);
                if (container == null)
                {
                    ItemChangerMod.instance.LogError($"Unable to find Container definition for {info.containerType}!");
                    return;
                }
                // TODO: add fsmName check here

                var give = info.giveInfo;
                var scene = info.changeSceneInfo;
                var cost = info.costInfo;

                if (give != null && !give.applied)
                {
                    container.AddGiveEffectToFsm(fsm, give);
                    give.applied = true;
                }

                if (scene != null && !scene.applied)
                {
                    container.AddChangeSceneToFsm(fsm, scene);
                    scene.applied = true;
                }

                if (cost != null && !cost.applied)
                {
                    container.AddCostToFsm(fsm, cost);
                    cost.applied = true;
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
}