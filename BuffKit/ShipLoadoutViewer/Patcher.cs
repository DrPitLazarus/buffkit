using System.Collections.Generic;
using HarmonyLib;

namespace BuffKit.ShipLoadoutViewer
{
    [HarmonyPatch(typeof(UIMatchLobby), "Awake")]
    public class UIMatchLobby_Awake
    {
        private static bool _firstPrepare = true;
        private static void Prepare()
        {
            if (_firstPrepare)
            {
                ShipLoadoutViewer.CreateLog();
                Util.Util.OnLobbyLoad += ShipLoadoutViewer.LoadGunTextures;
                Util.Util.OnLobbyLoad += ShipLoadoutViewer.LoadSkillTextures;
                Util.Util.OnGameInitialize += delegate { Settings.Settings.Instance.AddEntry("ship loadout viewer", ShipLoadoutViewer.SetShipBarVisibility, true); };
                Util.Util.OnGameInitialize += delegate { Settings.Settings.Instance.AddEntry("crew loadout viewer", ShipLoadoutViewer.SetCrewBarVisibility, true); };
                _firstPrepare = false;
            }
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
}
