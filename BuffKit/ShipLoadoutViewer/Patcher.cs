using System.Collections.Generic;
using BuffKit.Settings;
using HarmonyLib;
using UnityEngine;
using Resources = BuffKit.UI.Resources;

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
                Resources.RegisterGunTextureCallback(ShipLoadoutViewer.MarkShipBarsForRedraw);
                Resources.RegisterSkillTextureCallback(ShipLoadoutViewer.MarkCrewBarsForRedraw);
                Util.OnGameInitialize += delegate
                {
                    Settings.Settings.Instance.AddEntry("loadout viewer", "ship loadout viewer", ShipLoadoutViewer.SetShipBarVisibility, true);
                    Settings.Settings.Instance.AddEntry("loadout viewer", "crew loadout viewer", ShipLoadoutViewer.SetCrewBarVisibility, true);
                    var gridIcons = new List<Sprite>()
                    {
                        Resources.PilotIcon,
                        Resources.GunnerIcon,
                        Resources.EngineerIcon
                    };
                    var gridLabels = new List<string> { "Show pilot tools", "Show gunner tools", "Show engineer tools" };
                    var toggleGrid = new ToggleGrid(gridIcons, gridLabels, true);
                    toggleGrid.SetValues(new bool[,]
                    {
                        { true, false, false },
                        { false, true, true },
                        { false, true, true }
                    });
                    Settings.Settings.Instance.AddEntry("loadout viewer", "crew loadout display", ShipLoadoutViewer.SetCrewBarOptions, toggleGrid);
                    ShipLoadoutViewer.SetCrewBarOptions(toggleGrid);

                    var lobbyGunTooltipDisplay = new EnumString(
                        typeof(UIShipLoadoutSlot.UIShipLoadoutSlotInfoViewer),
                        (int)UIShipLoadoutSlot.InfoDisplaySetting);
                    Settings.Settings.Instance.AddEntry("loadout viewer", "lobby gun tooltip display", delegate (EnumString enumString)
                     {
                         UIShipLoadoutSlot.InfoDisplaySetting = (UIShipLoadoutSlot.UIShipLoadoutSlotInfoViewer)enumString.SelectedValue;
                     }, lobbyGunTooltipDisplay);
                    Settings.Settings.Instance.AddEntry("loadout viewer", "crew loadout display separator", ShipLoadoutViewer.SetCrewLoadoutDisplaySeparator, false);
                    Settings.Settings.Instance.AddEntry("loadout viewer", "crew profile button visibility", ShipLoadoutViewer.SetCrewProfileButtonVisibility, true);
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
