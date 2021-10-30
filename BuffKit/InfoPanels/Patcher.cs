using HarmonyLib;
using Muse.Goi2.Entity.Vo;
using UnityEngine;

namespace BuffKit.InfoPanels
{
    [HarmonyPatch(typeof(UIManager.UILoadingLobbyState), "Exit")]
    class UILoadingLobbyState_Exit
    {
        private static bool _firstPrepare = true;
        private static void Prepare()
        {
            if (_firstPrepare)
            {
                Util.OnGameInitialize += delegate { GunInfoOverlay.Initialize(); };

                _firstPrepare = false;
            }
        }
    }

    [HarmonyPatch(typeof(UIGunTooltip), "RenderGun")]
    class UIGunTooltip_RenderGun
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
    class UIOverlayPanel_ShowAtScreenPosition
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
    class UIOverlayPanel_Hide
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
    [HarmonyPatch(typeof(UIShipCustomizationScreen), "SetActiveShip")]
    class UIShipCustomizationScreen_SetActiveShip
    {
        private static bool _firstPrepare = true;
        private static void Prepare()
        {
            if (_firstPrepare)
            {
                Util.OnGameInitialize += delegate { ShipStatsPanel.Initialize(); };

                _firstPrepare = false;
            }
        }
        private static void Postfix(ShipViewObject ___currentShip)
        {
            ShipStatsPanel.SetShip(___currentShip.Model);
        }
    }
}
