using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BuffKit.ToggleMatchUI
{
    // Create component
    [HarmonyPatch(typeof(Mission), "Start")]
    class Mission_Start
    {
        private static void Postfix()
        {
            Mission.Instance.gameObject.AddComponent<ToggleUIController>();
        }
    }

    // Destroy component
    [HarmonyPatch(typeof(Mission), "OnDisable")]
    class Mission_OnDisable
    {
        private static void Postfix()
        {
            if (ToggleUIController.Initialized)
                Object.Destroy(ToggleUIController.Instance);
        }
    }

    // Hide hitmarkers(?)
    [HarmonyPatch(typeof(UIOverlayHitIndicator), "Refresh")]
    class UIOverlayHitIndicator_Refresh
    {
        private static bool Prefix(UIOverlayHitIndicator __instance)
        {
            if (__instance.gameObject.activeSelf)
            {
                if (ToggleUIController.Initialized && ToggleUIController.Instance.ShowUI)
                    return true;
                __instance.gameObject.SetActive(false);
                return false;
            }
            return true;
        }
    }

    // Hide hitmarkers
    [HarmonyPatch(typeof(UIOverlayManager), "UpdateHitPoint")]
    class UIOverlayManager_UpdateHitPoint
    {
        private static void Postfix(UITransform uiTransform, HitPoint hit)
        {
            if (!ToggleUIController.Initialized || ToggleUIController.Instance.ShowUI) return;
            uiTransform.DeactivateIfActivated(0f);
        }
    }

    // Hide Alliance spotted ship
    [HarmonyPatch(typeof(UIOverlayManager), "UpdateHighlightSpotting")]
    class UIOverlayManager_UpdateHighlightSpotting
    {
        private static bool Prefix(UITransform[] ___radarBlips, UIGenericHealthBar[] ___shipHealth)
        {
            if (!ToggleUIController.Initialized || ToggleUIController.Instance.ShowUI) return true;
            foreach (var blip in ___radarBlips)
                blip.Deactivate(0f);
            foreach (var health in ___shipHealth)
                health.Deactivate();
            return false;
        }
    }
    // Hide Skirmish spotted ship
    [HarmonyPatch(typeof(UIOverlayManager), "UpdateBracketSpotting")]
    class UIOverlayManager_UpdateBracketSpotting
    {
        private static bool Prefix(UITransform[] ___radarBlips, UIGenericHealthBar[] ___shipHealth)
        {
            if (!ToggleUIController.Initialized || ToggleUIController.Instance.ShowUI) return true;
            foreach (var blip in ___radarBlips)
                blip.Deactivate(0f);
            foreach (var health in ___shipHealth)
                health.Deactivate();
            return false;
        }
    }
    // Hide Alliance captain-marked ship
    [HarmonyPatch(typeof(UIOverlayManager), "UpdateHealthBar")]
    class UIOverlayManager_UpdateHealthBar
    {
        private static bool Prefix(Ship ship, UIGenericHealthBar shipHealth, Color highlightColor)
        {
            if (!ToggleUIController.Initialized || ToggleUIController.Instance.ShowUI) return true;
            shipHealth.Deactivate();
            return false;
        }
    }

    // Prevents error spam when on gun
    [HarmonyPatch(typeof(UIReticle), "SetPosition")]
    class UIReticle_SetPosition
    {
        private static bool Prefix(Vector3 worldPoint)
        {
            if (!ToggleUIController.Initialized || ToggleUIController.Instance.ShowUI) return true;
            return false;
        }
    }

    [HarmonyPatch(typeof(UIRepairComponentView), "LateUpdate")]
    class UIRepairComponentView_LateUpdate
    {
        private static bool Prefix(UIRepairComponentView __instance, UITransform ___root, ref Repairable ___lastInRangeRepairable, ref Repairable ___inRangeRepairable,
            ref UsablePart ___inRangeHelm, ref Hull ___hullComponent, ref bool ___reAcquireHull)
        {
            var privateMethodBindingFlag = BindingFlags.NonPublic | BindingFlags.Instance;

            if (!___root.Activated || NetworkedPlayer.Local == null || NetworkedPlayer.Local.CurrentShip == null || LocalCharacterMotion.Instance == null || !ToggleUIController.Instance.ShowUI)
            {
                var methodHideInspector = __instance.GetType().GetMethod("HideInspector", privateMethodBindingFlag);
                methodHideInspector.Invoke(__instance, new object[] { });
                ___lastInRangeRepairable = null;
                ___inRangeRepairable = null;
                ___inRangeHelm = null;
                ___hullComponent = null;
                return false;
            }
            if (___hullComponent == null || !___hullComponent.enabled || !___hullComponent.gameObject.activeInHierarchy)
            {
                ___reAcquireHull = true;
            }
            if (___reAcquireHull)
            {
                IList<Repairable> repairables = NetworkedPlayer.Local.CurrentShip.Repairables;
                for (int i = 0; i < repairables.Count; i++)
                {
                    if (repairables[i].Type == LocalShipPartType.Hull)
                    {
                        ___hullComponent = (repairables[i] as Hull);
                        if (___hullComponent != null)
                        {
                            ___reAcquireHull = false;
                            break;
                        }
                    }
                }
            }
            if (LocalCharacterMotion.Instance.OnRepair != null)
            {
                ___inRangeRepairable = LocalCharacterMotion.Instance.OnRepair.GetComponent<Repairable>();
            }
            else
            {
                ___inRangeRepairable = null;
                if (LocalCharacterMotion.Instance.OnHelm != null)
                {
                    ___inRangeHelm = LocalCharacterMotion.Instance.OnHelm.GetComponent<UsablePart>();
                }
                else
                {
                    ___inRangeHelm = null;
                }
            }
            var rep = ___inRangeRepairable;
            IList<Repairable> repairables2 = NetworkedPlayer.Local.CurrentShip.Repairables;
            IEnumerable<Repairable> source = (!(___inRangeRepairable == null)) ? (from r in repairables2
                                                                                  where r != rep
                                                                                  select r) : repairables2;
            var methodDrawIndicators = __instance.GetType().GetMethod("DrawIndicators", privateMethodBindingFlag);
            methodDrawIndicators.Invoke(__instance, new object[] { (from r in source
                                 where r.NormalizedHealth < RepairComponentView.DISPLAY_THRESHOLD
                                 select r).ToList<Repairable>() });
            if (UIManager.CharacterInputMode == CharacterInputMode.Player)
            {
                var methodDrawInspector = __instance.GetType().GetMethod("DrawInspector", privateMethodBindingFlag);
                methodDrawInspector.Invoke(__instance, new object[] { ___inRangeRepairable ?? ___inRangeHelm });
            }
            else
            {
                var methodHideInspector = __instance.GetType().GetMethod("HideInspector", privateMethodBindingFlag);
                methodHideInspector.Invoke(__instance, new object[] { });
            }
            ___lastInRangeRepairable = ___inRangeRepairable;

            return false;
        }
    }

    [HarmonyPatch(typeof(UIShipDetailsView), "DrawComponentIndicators")]
    class UIShipDetailsView_DrawComponentIndicators
    {
        private static bool Prefix()
        {
            if (!ToggleUIController.Instance.ShowUI)
            {
                UIShipDetailsView.HideComponentIndicators(0);
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(UIShipDetailsView), "DrawCrewToolInspectors")]
    class UIShipDetailsView_DrawCrewToolInspectors
    {
        private static bool Prefix(IList<NetworkedPlayer> players, CrewToolInspector[] ___inspectorCache)
        {
            if (!ToggleUIController.Instance.ShowUI)
            {
                for (int i = 0; i < ___inspectorCache.Length; i++)
                {
                    var crewToolInspector = ___inspectorCache[i];
                    for (int j = 0; j < crewToolInspector.tools.Length; j++)
                    {
                        if (crewToolInspector.tools[j].Activated)
                            crewToolInspector.tools[j].Deactivate(0f);
                    }
                    crewToolInspector.group.Deactivate(0f);
                }
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(UIShipDetailsView), "Activate")]
    class UIShipDetailsView_Activate
    {
        private static bool Prefix()
        {
            return ToggleUIController.Instance.ShowUI;
        }
    }

    [HarmonyPatch(typeof(UIShipProfileView), "UpdateSideIndicators")]
    class UIShipProfileView_UpdateSideIndicators
    {
        private static bool Prefix(ShipProfileIndicator[] indicators, IEnumerable<Ship> ships)
        {
            if (ToggleUIController.Instance.ShowUI)
                return true;
            var i = 0;
            while (i < indicators.Length)
            {
                if (indicators[i].root.Activated)
                {
                    indicators[i].root.Deactivate(0f);
                }
                i++;
            }
            return false;
        }
    }
}