using HarmonyLib;

namespace BuffKit.Minimap
{
    [HarmonyPatch(typeof(UIManager.UIGamePlayState), "HandleOverlayToggles")]
    internal class Patcher
    {
        private static bool Prefix(bool showOverlays = true)
        {
            if (IcarusInput.ScoreLogHeld)
            {
                UIScoreboard.Activated = true;
                MapController.Instance.Disabled();
                UIOverlayDisplay.Deactivate();
            }
            else
            {
                UIScoreboard.Activated = false;
                if (IcarusInput.MapHeld)
                {
                    MapController.Instance.Full();
                    UIOverlayDisplay.Deactivate();
                }
                else
                {
                    if (MapController.Instance.MinimapEnabled && NetworkedPlayer.Local.IsSpectator)
                    {
                        MapController.Instance.Minimap();
                    }
                    else
                    {
                        MapController.Instance.Disabled();
                    }
                    
                    if (showOverlays)
                    {
                        UIOverlayDisplay.Activate();
                    }
                    else
                    {
                        UIOverlayDisplay.Deactivate();
                    }
                }
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(Mission), "Start")]
    internal class Mission_Start
    {
        private static void Postfix()
        {
            MapController.Initialize();
            if (MapController.Instance.MinimapEnabled)
            {
                MapController.Instance.Minimap();
            }
            else
            {
                MapController.Instance.Disabled();
            }
        }
    }
}
