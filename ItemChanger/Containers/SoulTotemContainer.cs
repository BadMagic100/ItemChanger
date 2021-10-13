﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger.Components;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Internal;
using ItemChanger.Items;
using ItemChanger.Util;
using UnityEngine;

namespace ItemChanger.Containers
{
    public class SoulTotemContainer : Container
    {
        public static readonly Color WasEverObtainedColor = new(1f, 213f / 255f, 0.5f);

        public class SoulTotemInfo : MonoBehaviour
        {
            public SoulTotemSubtype type;
        }

        public override string Name => Container.Totem;
        public override bool SupportsInstantiate => ObjectCache.SoulTotemPreloader.PreloadLevel != PreloadLevel.None;

        public override GameObject GetNewContainer(AbstractPlacement placement, IEnumerable<AbstractItem> items, FlingType flingType, Cost cost = null)
        {
            SoulTotemSubtype type = items.OfType<SoulTotemItem>().FirstOrDefault()?.soulTotemSubtype ?? SoulTotemSubtype.B;
            GameObject totem = ObjectCache.SoulTotem(type);
            type = totem.AddComponent<SoulTotemInfo>().type = ObjectCache.SoulTotemPreloader.GetPreloadedTotemType(type);
            totem.name = GetNewSoulTotemName(placement);
            
            if (ShrinkageFactor.TryGetValue(type, out var k))
            {
                var t = totem.transform;
                t.localScale = new Vector3(t.localScale.x * k, t.localScale.y * k, t.localScale.z);
            }

            totem.AddComponent<DropIntoPlace>();
            totem.GetComponent<BoxCollider2D>().isTrigger = false; // some rocks only have trigger colliders


            ContainerInfo info = totem.AddComponent<ContainerInfo>();
            info.containerType = Container.Totem;
            info.giveInfo = new()
            {
                items = items,
                flingType = flingType,
                placement = placement,
            };

            return totem;
        }

        /// <remarks>
        /// Soul totems are implemented as follows:
        /// - The totem type is the type of the first SoulTotemItem in the items list, or B if no SoulTotemItems were included. It is also B if Soul Totem preloads were reduced.
        /// - The number of hits of the totem is the sum of the number of hits of each of the SoulTotemItems. If any of the SoulTotemItems have a negative number of hits, the totem is infinite.
        /// - The totem spawns non-soul items as shinies on the first hit. If the totem is depleted but has items, it will still spawn items on the first hit, but not give soul.
        /// - Soul items also count as given after the first hit. If all of the items on the soul totem have been obtained at least once, the totem becomes tinted orange.
        /// - The number of hits left on the totem is saved by its PersistentIntItem component.
        /// </remarks>
        public override void AddGiveEffectToFsm(PlayMakerFSM fsm, ContainerGiveInfo info)
        {
            FsmState init = fsm.GetState("Init");
            FsmState hit = fsm.GetState("Hit");
            FsmState far = fsm.GetState("Far");
            FsmState close = fsm.GetState("Close");
            FsmState checkIfNail = fsm.GetState("Check if Nail");

            FsmBool activated = fsm.FsmVariables.FindFsmBool("Activated");
            FsmInt value = fsm.FsmVariables.FindFsmInt("Value");
            
            if (init.Transitions.Length < 2) // modify PoP fsm to match usual fsm
            {
                FsmGameObject emitter = fsm.FsmVariables.FindFsmGameObject("Emitter");
                FsmOwnerDefault emitterOwnerDefault = new() { GameObject = emitter, OwnerOption = OwnerDefaultOption.SpecifyGameObject, };
                FsmGameObject glower = fsm.FsmVariables.FindFsmGameObject("Glower");
                FsmOwnerDefault self = new();
                FsmGameObject hero = fsm.FsmVariables.FindFsmGameObject("Hero");
                FsmFloat distance = fsm.FsmVariables.FindFsmFloat("Distance");

                FsmState depleted = new(fsm.Fsm)
                {
                    Name = "Depleted",
                    Transitions = Array.Empty<FsmTransition>(),
                    Actions = new FsmStateAction[]
                    {
                        new SetCollider{ gameObject = self, active = false, },
                        new DestroyObject{ gameObject = emitter, delay = 0, detachChildren = false, },
                        new DestroyObject{ gameObject = glower, delay = 0, detachChildren = false, },
                        new ActivateGameObject{ gameObject = emitterOwnerDefault, activate = false, recursive = false, resetOnExit = false, everyFrame = false },
                        new SetBoolValue{ boolVariable = activated, boolValue = true, everyFrame = false },
                        new SetParticleEmission{ gameObject = emitterOwnerDefault, emission = false },
                        new GetDistance{ gameObject = self, target = hero,  },
                        far.GetFirstActionOfType<GetMaterialColor>(),
                        far.GetFirstActionOfType<EaseColor>(),
                        far.GetFirstActionOfType<SetMaterialColor>(),
                    },
                };
                fsm.AddState(depleted);

                FsmState meshRendererOff = new(fsm.Fsm)
                {
                    Name = "Mesh Renderer Off",
                    Transitions = new[] { new FsmTransition { FsmEvent = FsmEvent.Finished, ToFsmState = depleted, ToState = depleted.Name } },
                    Actions = new FsmStateAction[]
                    {
                        new SetSpriteRenderer{ gameObject = self, active = false },
                    },
                };
                fsm.AddState(meshRendererOff);

                init.AddTransition(FsmEvent.GetFsmEvent("DEPLETED"), meshRendererOff);
                hit.AddTransition(FsmEvent.GetFsmEvent("DEPLETED"), depleted);

                hit.Actions = new FsmStateAction[] // put the hit actions in the order of a normal soul totem fsm
                {
                    hit.Actions[0],
                    hit.Actions[1],
                    hit.Actions[6],
                    hit.Actions[7],
                    hit.Actions[2],
                    hit.Actions[8],
                    hit.Actions[3],
                    hit.Actions[4],
                    hit.Actions[9],
                    hit.Actions[5],
                };
            }

            PersistentIntItem pii = fsm.GetComponent<PersistentIntItem>();
            if (pii == null)
            {
                void OnAwake(On.PersistentIntItem.orig_Awake orig, PersistentIntItem self)
                {
                    if (self.persistentIntData == null) self.persistentIntData = new() { id = self.gameObject.name, sceneName = self.gameObject.scene.name, semiPersistent = self.semiPersistent };
                    orig(self);
                }
                On.PersistentIntItem.Awake += OnAwake;
                pii = fsm.gameObject.AddComponent<PersistentIntItem>();
                On.PersistentIntItem.Awake -= OnAwake;
            }
            PersistentIntData pid = pii.persistentIntData ??= new();
            pid.id = fsm.gameObject.name;
            pid.sceneName = fsm.gameObject.scene.name;

            FsmBool spawnedItems = fsm.AddFsmBool("Spawned Items", false);
            FsmState firstHit = new(fsm.Fsm)
            {
                Name = "First Hit?",
                Transitions = new[] { new FsmTransition { FsmEvent = FsmEvent.Finished, ToFsmState = hit, ToState = hit.Name, } },
                Actions = new FsmStateAction[]
                {
                    new BoolTest
                    {
                        boolVariable = spawnedItems,
                        isTrue = FsmEvent.Finished,
                        isFalse = null,
                    },
                    new Lambda(InstantiateShiniesAndGiveEarly),
                },
            };
            fsm.AddState(firstHit);
            checkIfNail.Transitions.First(t => t.EventName == "DAMAGED").SetToState(firstHit);


            bool shouldBeInfinite = info.items.OfType<SoulTotemItem>().Any(i => i.hitCount < 0);
            fsm.FsmVariables.GetFsmInt("Value").Value = 0;
            fsm.GetState("Reset?").GetFirstActionOfType<SetIntValue>().intValue.Value = 0;
            fsm.GetState("Reset").GetFirstActionOfType<SetIntValue>().intValue.Value = 0;

            if (shouldBeInfinite)
            {
                // PoP totems, or fake PoP totems, should be infinite
                init.RemoveTransitionsTo("Mesh Renderer Off");
                hit.RemoveTransitionsTo("Depleted");
            }

            init.RemoveActionsOfType<BoolTest>();
            init.RemoveActionsOfType<IntCompare>();
            init.AddLastAction(new DelegateBoolTest(DepletedTest, FsmEvent.GetFsmEvent("DEPLETED"), FsmEvent.Finished));

            if (info.items.All(i => i.WasEverObtained()))
            {
                fsm.GetState("Close").GetFirstActionOfType<EaseColor>().toValue = WasEverObtainedColor;
            }

            hit.Actions = new[]
            {
                hit.Actions[0], // AudioPlayerOneShotSingle soul_totem_slash
                hit.Actions[1], // SetMaterialColor Glower (1,1,1,1)
                hit.Actions[2], // SpawnObjectFromGlobalPool Strike Nail R
                hit.Actions[3], // SpawnObjectFromGlobalPool White Flash R
                hit.Actions[4], // SendEventByName Camera EnemyKillShake
                new DelegateBoolTest(() => value.Value <= 0, FsmEvent.GetFsmEvent("DEPLETED"), null),
                hit.Actions[5], // FlingObjectsFromGlobalPool Soul Orb R
                hit.Actions[6], // SetProperty -- // TODO: what happens here?
                hit.Actions[7], // IntOperator subtract 1 from Value
                hit.Actions[8], // IntCompare Value <= 0 then DEPLETED else null
                hit.Actions[9], // Wait 0.25f then FINISHED
            };

            bool DepletedTest()
            {
                if (info.items.Any(i => !i.IsObtained())) return false;
                if (activated.Value) return true;
                return value.Value <= 0;
            }

            void InstantiateShiniesAndGiveEarly()
            {
                GiveInfo gi = new()
                {
                    Container = Container.Totem,
                    FlingType = info.flingType,
                    Transform = fsm.transform,
                    MessageType = MessageType.Corner,
                };
                GameObject itemParent = new("Item parent");
                itemParent.transform.position = fsm.transform.position;

                foreach (AbstractItem item in info.items)
                {
                    if (!item.IsObtained())
                    {
                        if (item is SoulTotemItem totemItem) value.Value += totemItem.hitCount;

                        if (item.GiveEarly(Container.Totem))
                        {
                            item.Give(info.placement, gi);
                        }
                        else
                        {
                            GameObject shiny = ShinyUtility.MakeNewShiny(info.placement, item, info.flingType);
                            ShinyUtility.PutShinyInContainer(itemParent, shiny);
                        }
                    }
                }

                foreach (Transform t in itemParent.transform)
                {
                    t.gameObject.SetActive(true);
                }
                info.placement.AddVisitFlag(VisitState.Opened);
                spawnedItems.Value = true;
            }
        }

        /*
             * normal totem layout
                hit.Actions[0], // AudioPlayerOneShotSingle soul_totem_slash
                hit.Actions[1], // SetMaterialColor Glower (1,1,1,1)
                hit.Actions[2], // SpawnObjectFromGlobalPool Strike Nail R
                hit.Actions[3], // SpawnObjectFromGlobalPool White Flash R
                hit.Actions[4], // SendEventByName Camera EnemyKillShake
                hit.Actions[5], // FlingObjectsFromGlobalPool Soul Orb R
                hit.Actions[6], // SetProperty -- // TODO: what happens here?
                hit.Actions[7], // IntOperator subtract 1 from Value
                hit.Actions[8], // IntCompare Value <= 0 then DEPLETED else null
                hit.Actions[9], // Wait 0.25f then FINISHED
            * pop totem layout
                hit.Actions[0], // AudioPlayerOneShotSingle soul_totem_slash
                hit.Actions[1], // SetMaterialColor Glower (1,1,1,1)
                hit.Actions[2], // SendEventByName Camera EnemyKillShake    
                hit.Actions[3], // SetProperty -- // TODO: what happens here?
                hit.Actions[4], // IntOperator subtract 1 from Value
                hit.Actions[5], // Wait 0.25f then FINISHED
                hit.Actions[6], // SpawnObjectFromGlobalPool Strike Nail R
                hit.Actions[7], // SpawnObjectFromGlobalPool White Flash R
                hit.Actions[8], // FlingObjectsFromGlobalPool Soul Orb R
                hit.Actions[9], // IntCompare Value <= 0 then DEPLETED else null
            */

        public override void ApplyTargetContext(GameObject obj, float x, float y, float elevation)
        {
            base.ApplyTargetContext(obj, x, y, elevation);
            obj.transform.Translate(new(0, Elevation[GetSoulTotemSubtype(obj)], 0.005f));
        }

        public override void ApplyTargetContext(GameObject obj, GameObject target, float elevation)
        {
            base.ApplyTargetContext(obj, target, elevation);
            obj.transform.Translate(new(0, Elevation[GetSoulTotemSubtype(obj)], 0.005f));
        }

        public static string GetNewSoulTotemName(AbstractPlacement placement)
        {
            return $"Soul Totem-{placement.Name}";
        }

        public static SoulTotemSubtype GetSoulTotemSubtype(GameObject totem)
        {
            if (totem.GetComponent<SoulTotemInfo>() is SoulTotemInfo totemInfo && totemInfo != null)
            {
                return totemInfo.type;
            }
            else return SoulTotemSubtype.B;
        }

        public static readonly Dictionary<SoulTotemSubtype, int> HitCount = new()
        {
            [SoulTotemSubtype.A] = 5,
            [SoulTotemSubtype.B] = 3,
            [SoulTotemSubtype.C] = 3,
            [SoulTotemSubtype.D] = 5,
            [SoulTotemSubtype.E] = 5,
            [SoulTotemSubtype.F] = 5,
            [SoulTotemSubtype.G] = 5,
            [SoulTotemSubtype.Palace] = 5,
            [SoulTotemSubtype.PathOfPain] = -1,
        };


        public static readonly Dictionary<SoulTotemSubtype, float> ShrinkageFactor = new()
        {
            [SoulTotemSubtype.D] = 0.7f,
            [SoulTotemSubtype.E] = 0.7f,
            [SoulTotemSubtype.Palace] = 0.8f,
            [SoulTotemSubtype.PathOfPain] = 0.7f,
        };

        public static readonly Dictionary<SoulTotemSubtype, float> Elevation = new()
        {
            [SoulTotemSubtype.A] = 0.5f,
            [SoulTotemSubtype.B] = -0.1f,
            [SoulTotemSubtype.C] = -0.1f,
            // Some elevation values adjusted from the originals to account for the shrinkage.
            [SoulTotemSubtype.D] = 1.3f - 0.5f,
            [SoulTotemSubtype.E] = 1.2f - 0.5f,
            [SoulTotemSubtype.F] = 0.8f,
            [SoulTotemSubtype.G] = 0.2f,
            [SoulTotemSubtype.Palace] = 1.3f - 0.3f,
            [SoulTotemSubtype.PathOfPain] = 1.5f - 0.9f,
        };
    }
}
