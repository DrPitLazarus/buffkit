using System.Collections.Generic;
using HarmonyLib;

namespace BuffKit.ShipLoadoutViewer
{
    [HarmonyPatch(typeof(UIMatchLobby), "Awake")]
    public class UIMatchLobby_Awake
    {
        private static void Prefix(UIMatchLobby __instance)
        {
            ShipLoadoutViewer.LobbyUIPreBuild(__instance);
        }
        private static void Postfix(UIMatchLobby __instance, List<List<UILobbyCrew>> ___crewElements)
        {
            ShipLoadoutViewer.LobbyUIPostBuild(__instance, ___crewElements);
        }
    }

    [HarmonyPatch(typeof(MatchLobbyView), "Awake")]
    public class MatchLobbyView_Awake
    {
        private static void Prepare()
        {
            ShipLoadoutViewer.CreateLog();
        }
        private static void Postfix(MatchLobbyView __instance)
        {

            __instance.lobbyDataChanged += delegate
            {
                ShipLoadoutViewer.LobbyDataChanged(__instance);
            };
        }
    }

    [HarmonyPatch(typeof(UIManager.UILoadingLobbyState), "Exit")]
    public class UILoadingLobbyState_Exit
    {
        private static void Postfix()
        {
            ShipLoadoutViewer.EnsureDataIsLoaded();
        }
    }
}
