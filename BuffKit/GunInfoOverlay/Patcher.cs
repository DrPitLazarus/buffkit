using HarmonyLib;
using UnityEngine;

namespace BuffKit.GunInfoOverlay
{
    [HarmonyPatch(typeof(UIManager.UILoadingLobbyState), "Exit")]
    public class UILoadingLobbyState_Exit
    {
        private static bool _firstPrepare = true;
        private static void Prepare()
        {
            if (_firstPrepare)
            {
                Util.Util.OnGameInitialize += delegate { GunInfoOverlay.Initialize(); };

                _firstPrepare = false;
            }
        }
    }

    [HarmonyPatch(typeof(UIGunTooltip), "RenderGun")]
    public class UIGunTooltip_RenderGun
    {
        private static bool Prefix(GunItemInfo info, string additionalTipText)
        {
            if (info != null && !string.IsNullOrEmpty(info.name))
            {
                return GunInfoOverlay.DisplayGun(info);
            }
            else
                return true;
        }
    }
    [HarmonyPatch(typeof(UIOverlayPanel), "ShowAtScreenPosition")]
    public class UIOverlayPanel_ShowAtScreenPosition
    {
        private static bool Prefix(UIOverlayPanel __instance, Vector3 position, Vector2? pivot, float fade)
        {
            if (__instance is UIGunTooltip)
            {
                return GunInfoOverlay.ShowAtScreenPosition(position, pivot);
            }
            else
            {
                return true;
            }
        }
    }
    [HarmonyPatch(typeof(UIOverlayPanel), "Hide")]
    public class UIOverlayPanel_Hide
    {
        private static bool Prefix(UIOverlayPanel __instance)
        {
            if (__instance is UIGunTooltip)
            {
                return GunInfoOverlay.Hide();
            }
            else
            {
                return true;
            }
        }
    }
}
