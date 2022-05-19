using HarmonyLib;
using UnityEngine;

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
}