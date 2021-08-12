using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;

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
                Util.Util.OnGameInitialize += delegate
                {
                    GunInfoOverlay.CreateLog();
                    GunInfoOverlay.CreatePanel();
                    GunInfoOverlay.ListAllGunDetails();
                };

                _firstPrepare = false;
            }
        }
    }
}
