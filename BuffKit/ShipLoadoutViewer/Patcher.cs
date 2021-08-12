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
                Util.Util.OnGameInitialize += delegate
                {
                    Settings.Settings.Instance.AddEntry("ship loadout viewer", ShipLoadoutViewer.SetShipBarVisibility, true);
                    Settings.Settings.Instance.AddEntry("crew loadout viewer", ShipLoadoutViewer.SetCrewBarVisibility, true);
                    var gridIcons = new List<UnityEngine.Sprite>()
                    {
                        UI.Resources.PilotIcon,
                        UI.Resources.GunnerIcon,
                        UI.Resources.EngineerIcon
                    };
                    var gridLabels = new List<string> { "Show pilot tools", "Show gunner tools", "Show engineer tools" };
                    var toggleGrid = new Settings.ToggleGrid(gridIcons, gridLabels, true);
                    toggleGrid.SetValues(new bool[,]
                    {
                        { true, false, false },
                        { false, true, true },
                        { false, true, true }
                    });
                    Settings.Settings.Instance.AddEntry("crew loadout display", ShipLoadoutViewer.SetCrewBarOptions, toggleGrid);
                    ShipLoadoutViewer.SetCrewBarOptions(toggleGrid);
                };

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
