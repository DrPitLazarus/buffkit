using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BuffKit.ToggleMatchUI
{
    /// <summary>
    /// Initializes the ToggleUIController when mission starts. Gets destroyed when the mission ends.
    /// </summary>
    [HarmonyPatch(typeof(UIManager.UIMatchBlockState), nameof(UIManager.UIMatchBlockState.Exit))] // Normal match start.
    [HarmonyPatch(typeof(UIManager.UILoadingBlockState), nameof(UIManager.UILoadingBlockState.Exit))] // Join running match.
    class Mission_Start
    {
        private static void Postfix()
        {
            if (!ToggleUIController.Initialized)
            {
                Mission.Instance.gameObject.AddComponent<ToggleUIController>();
            }
        }
    }

    // Following patches hide various UI elements when ShowUI is off.

    // Hide hit markers(?)
    [HarmonyPatch(typeof(UIOverlayHitIndicator), nameof(UIOverlayHitIndicator.Refresh))]
    class UIOverlayHitIndicator_Refresh
    {
        private static bool Prefix(UIOverlayHitIndicator __instance)
        {
            if (__instance.gameObject.activeSelf)
            {
                if (ToggleUIController.Initialized && ToggleUIController.ShowUI)
                    return true;
                __instance.gameObject.SetActive(false);
                return false;
            }
            return true;
        }
    }

    // Hide hit markers
    [HarmonyPatch(typeof(UIOverlayManager), nameof(UIOverlayManager.UpdateHitPoint))]
    class UIOverlayManager_UpdateHitPoint
    {
        private static void Postfix(UITransform uiTransform, HitPoint hit)
        {
            if (!ToggleUIController.Initialized || ToggleUIController.ShowUI) return;
            uiTransform.DeactivateIfActivated(0f);
        }
    }

    // Hide Alliance spotted ship
    [HarmonyPatch(typeof(UIOverlayManager), nameof(UIOverlayManager.UpdateHighlightSpotting))]
    class UIOverlayManager_UpdateHighlightSpotting
    {
        private static bool Prefix(UITransform[] ___radarBlips, UIGenericHealthBar[] ___shipHealth)
        {
            if (!ToggleUIController.Initialized || ToggleUIController.ShowUI) return true;
            foreach (var blip in ___radarBlips)
                blip.Deactivate(0f);
            foreach (var health in ___shipHealth)
                health.Deactivate();
            return false;
        }
    }
    // Hide Skirmish spotted ship
    [HarmonyPatch(typeof(UIOverlayManager), nameof(UIOverlayManager.UpdateBracketSpotting))]
    class UIOverlayManager_UpdateBracketSpotting
    {
        private static bool Prefix(UITransform[] ___radarBlips, UIGenericHealthBar[] ___shipHealth)
        {
            if (!ToggleUIController.Initialized || ToggleUIController.ShowUI) return true;
            foreach (var blip in ___radarBlips)
                blip.Deactivate(0f);
            foreach (var health in ___shipHealth)
                health.Deactivate();
            return false;
        }
    }
    // Hide Alliance captain-marked ship
    [HarmonyPatch(typeof(UIOverlayManager), nameof(UIOverlayManager.UpdateHealthBar))]
    class UIOverlayManager_UpdateHealthBar
    {
        private static bool Prefix(Ship ship, UIGenericHealthBar shipHealth, Color highlightColor)
        {
            if (!ToggleUIController.Initialized || ToggleUIController.ShowUI) return true;
            shipHealth.Deactivate();
            return false;
        }
    }

    // Prevents error spam when on gun
    [HarmonyPatch(typeof(UIReticle), nameof(UIReticle.SetPosition))]
    class UIReticle_SetPosition
    {
        private static bool Prefix(Vector3 worldPoint)
        {
            if (!ToggleUIController.Initialized || ToggleUIController.ShowUI) return true;
            return false;
        }
    }

    [HarmonyPatch(typeof(UIRepairComponentView), nameof(UIRepairComponentView.LateUpdate))]
    class UIRepairComponentView_LateUpdate
    {
        private static bool Prefix(UIRepairComponentView __instance)
        {
            if (!UIRepairComponentView.instance.root.Activated || NetworkedPlayer.Local == null || NetworkedPlayer.Local.CurrentShip == null || LocalCharacterMotion.Instance == null ||
                // MODIFIED SECTION.
                !ToggleUIController.ShowUI
                // END MODIFIED SECTION.
                )
            {
                __instance.HideInspector();
                __instance.lastInRangeRepairable = null;
                __instance.inRangeRepairable = null;
                __instance.inRangeHelm = null;
                __instance.hullComponent = null;
                // MODIFIED SECTION.
                if (!ToggleUIController.ShowUI)
                {
                    // Hide all repair indicators.
                    __instance.DrawIndicators([]);
                }
                // END MODIFIED SECTION.
                return false;
            }
            if (__instance.hullComponent == null || !__instance.hullComponent.enabled || !__instance.hullComponent.gameObject.activeInHierarchy)
            {
                __instance.reAcquireHull = true;
            }
            if (__instance.reAcquireHull)
            {
                IList<Repairable> repairables = NetworkedPlayer.Local.CurrentShip.Repairables;
                for (int i = 0; i < repairables.Count; i++)
                {
                    if (repairables[i].Type == LocalShipPartType.Hull)
                    {
                        __instance.hullComponent = repairables[i] as Hull;
                        if (__instance.hullComponent != null)
                        {
                            __instance.reAcquireHull = false;
                            break;
                        }
                    }
                }
            }
            if (LocalCharacterMotion.Instance.OnRepair != null)
            {
                __instance.inRangeRepairable = LocalCharacterMotion.Instance.OnRepair.GetComponent<Repairable>();
            }
            else
            {
                __instance.inRangeRepairable = null;
                if (LocalCharacterMotion.Instance.OnHelm != null)
                {
                    __instance.inRangeHelm = LocalCharacterMotion.Instance.OnHelm.GetComponent<UsablePart>();
                }
                else
                {
                    __instance.inRangeHelm = null;
                }
            }
            IList<Repairable> repairables2 = NetworkedPlayer.Local.CurrentShip.Repairables;
            IEnumerable<Repairable> enumerable = ((!(__instance.inRangeRepairable == null)) ? repairables2.Where((Repairable r) => r != __instance.inRangeRepairable) : repairables2);
            __instance.DrawIndicators(enumerable.Where((Repairable r) => r.NormalizedHealth < RepairComponentView.DISPLAY_THRESHOLD).ToList<Repairable>());
            if (UIManager.CharacterInputMode == CharacterInputMode.Player)
            {
                __instance.DrawInspector(__instance.inRangeRepairable ?? __instance.inRangeHelm);
            }
            else
            {
                __instance.HideInspector();
            }
            __instance.lastInRangeRepairable = __instance.inRangeRepairable;

            return false;
        }
    }

    [HarmonyPatch(typeof(UIShipDetailsView), nameof(UIShipDetailsView.DrawComponentIndicators))]
    class UIShipDetailsView_DrawComponentIndicators
    {
        private static bool Prefix()
        {
            if (!ToggleUIController.ShowUI)
            {
                UIShipDetailsView.HideComponentIndicators(0);
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(UIShipDetailsView), nameof(UIShipDetailsView.DrawCrewToolInspectors))]
    class UIShipDetailsView_DrawCrewToolInspectors
    {
        private static bool Prefix(IList<NetworkedPlayer> players, CrewToolInspector[] ___inspectorCache)
        {
            if (!ToggleUIController.ShowUI)
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
    [HarmonyPatch(typeof(UIShipDetailsView), nameof(UIShipDetailsView.Activate))]
    class UIShipDetailsView_Activate
    {
        private static bool Prefix()
        {
            return ToggleUIController.ShowUI;
        }
    }

    [HarmonyPatch(typeof(UIShipProfileView), nameof(UIShipProfileView.UpdateSideIndicators))]
    class UIShipProfileView_UpdateSideIndicators
    {
        private static bool Prefix(ShipProfileIndicator[] indicators, IEnumerable<Ship> ships)
        {
            if (ToggleUIController.ShowUI)
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