using System.Collections.Generic;
using HarmonyLib;

namespace BuffKit.ShipLoadoutViewer
{
    [HarmonyPatch(typeof(UIMatchLobby), "Awake")]
    public class UIMatchLobby_Awake
    {
        private static void Prepare()
        {
            ShipLoadoutViewer.CreateLog();
        }
        private static void Prefix(UIMatchLobby __instance)
        {
            ShipLoadoutViewer.LobbyUIPreBuild(__instance);
        }
        private static void Postfix(List<List<UILobbyCrew>> ___crewElements)
        {
            ShipLoadoutViewer.LobbyUIPostBuild(___crewElements);
        }
    }

    [HarmonyPatch(typeof(UIMatchLobby), "SetData")]
    public class UIMatchLobby_PaintCrews
    {
        private static void Postfix(UIMatchLobby __instance, MatchLobbyView ___mlv)
        {
            ShipLoadoutViewer.PaintLoadoutBars(___mlv);
        }
    }

    [HarmonyPatch(typeof(UIManager.UILoadingLobbyState), "Exit")]
    public class UILoadingLobbyState_Exit
    {
        private static void Postfix()
        {
            ShipLoadoutViewer.LoadShipAndGunData();
        }
    }
}
