using HarmonyLib;
using Muse.Goi2.Entity.Vo;
using UnityEngine;

namespace BuffKit.HullDisplay
{
    [HarmonyPatch(typeof(UIShipHealthIndicatorBar), "SetHealthBar")]
    class UILoadingLobbyState_Exit
    {
        private static bool _firstPrepare = true;
        private static void Prepare()
        {
            if (_firstPrepare)
            {
                Util.OnGameInitialize += delegate {  };

                _firstPrepare = false;
            }
        }

        private static void Postfix(UIShipHealthIndicatorBar __instance, float value)
        {

        }
    }
}
