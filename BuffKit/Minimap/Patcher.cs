using HarmonyLib;
using UnityEngine;

namespace BuffKit.Minimap
{
    
    [HarmonyPatch(typeof(UIManager.UIGamePlayState), "HandleOverlayToggles")]
    class UIGamePlayState_HandleOverlayToggles
    {
        private static bool Prefix(bool showOverlays)
        {
            if (MapController.Initialized && MapController.Instance.MinimapEnabled)
            {
                if (IcarusInput.ScoreLogHeld)
                {
                    UIScoreboard.Activated = true;
                    UIMapDisplay.Deactivate();
                    MapController.Instance.Disabled();
                    UIOverlayDisplay.Deactivate();
                }
                else
                {
                    UIScoreboard.Activated = false;
                    if (IcarusInput.MapHeld)
                    {
                        UIMapDisplay.Activate();
                        MapController.Instance.Full();
                        UIOverlayDisplay.Deactivate();
                    }
                    else
                    {
                        MapController.Instance.Minimap();
                        if (showOverlays)
                            UIOverlayDisplay.Activate();
                        else
                            UIOverlayDisplay.Deactivate();
                    }
                }
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Mission), "Start")]
    class Mission_Start
    {
        private static void Postfix()
        {
            if (NetworkedPlayer.Local.IsSpectator)
            {
                Mission.Instance.gameObject.AddComponent<MapController>();
                MapController.Instance.Disabled();
            }
        }
    }

    [HarmonyPatch(typeof(Mission), "OnDisable")]
    class Mission_OnDisable
    {
        private static void Postfix()
        {
            if (MapController.Initialized)
                Object.Destroy(MapController.Instance);
        }
    }
}
