using System;
using System.Linq;
using HarmonyLib;
using Muse.Goi2.Entity;
using Muse.Goi2.Entity.Vo;

namespace BuffKit.GunSelection
{
    [HarmonyPatch(typeof(UIManager.UINewShipState), "OpenGunSelectionPopup")]
    class UIManager_UINewShipState_OpenGunSelectionPopup
    {
        private static bool _useCustom = true;

        private static bool _firstPrepare = true;
        private static void Prepare()
        {
            if (_firstPrepare)
            {
                Util.Util.OnGameInitialize += delegate
                {
                    UIGunSelection.Initialize();
                    Settings.Settings.Instance.AddEntry<bool>("gun selection panel", delegate (bool v)
                    {
                        _useCustom = v;
                        if (!v)
                            UIGunSelection.Instance.Activated = false;
                    }, _useCustom);
                };

                _firstPrepare = false;
            }
        }
        private static bool Prefix(ShipSlotViewObject slot,
            UIManager.UINewShipState __instance,
            ShipDataController ___shipDataController)
        {
            if (!_useCustom) return true;

            if (___shipDataController.LockGuns)
            {
                return false;
            }
            var currentSlot = slot;
            if (currentSlot != null)
            {
                Func<GunItem, bool> criteria = (GunItem gun) => gun.Size == currentSlot.Size && (NetworkedPlayer.Local.GameType & gun.GameType) > (GameType)0;

                var availableGuns = (from uitem in NetworkedPlayer.Inventory.GetByType(ItemType.GUN)
                                     where criteria((GunItem)uitem.Item)
                                     select uitem.Item.Id).ToList();

                if (availableGuns.Count > 0)
                {
                    UIGunSelection.Instance.DisplayGunSelection(currentSlot.Size, delegate (int gunId)
                    { ___shipDataController.EquipCurrentShip(currentSlot.Name, gunId); }, availableGuns);
                }
            }
            else
            {
                MuseLog.Error("Something's null", null);
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(UIPageFrame), "TryHideOverlay")]
    class UIPageFrame_TryHideOverlay
    {
        private static void Postfix(ref bool __result)
        {
            __result = __result || UIGunSelection.Instance.TryHide();
        }
    }

}
