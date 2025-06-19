using BuffKit.Settings;
using HarmonyLib;
using Muse.Goi2.Entity;
using Muse.Goi2.Entity.Vo;
using Muse.Icarus.Coop.WorldMap;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BuffKit.ShipLoadoutViewer
{
    [HarmonyPatch]
    class ShipLoadoutViewer
    {
        public static void LobbyUIPreBuild(UIMatchLobby uiml)
        {
            // Edit the "Sample Crew" object to have a loadout panel
            var crewUI = uiml.sampleCrew;
            Object.Destroy(crewUI.GetComponent<LayoutElement>());                   // Instead of setting the LayoutElement preferredHeight just delete it
            // Add min-width to class and profile icons so they don't squish.
            crewUI.transform.GetChild(1).GetChild(0).GetComponent<LayoutElement>().minWidth = 25;
            crewUI.transform.GetChild(1).GetChild(2).GetComponent<LayoutElement>().minWidth = 25;
        }

        class ShipLoadoutBars
        {
            public UILobbyShipLoadoutBar shipBar;
            public UILobbyCrewLoadoutBar[] crewBars;

            public ShipLoadoutBars(UILobbyCrew crew)
            {
                crewBars = new UILobbyCrewLoadoutBar[4];
                for (int i = 0; i < 4; i++)
                {
                    var obCrewSlot = crew.transform.GetChild(i + 1);
                    UILobbyCrewLoadoutBar.Build(obCrewSlot, out crewBars[i]);
                }

                var header = UILobbyShipLoadoutBar.Build(crew.transform, out shipBar);
                header.transform.SetSiblingIndex(1);
            }

            public void DisplayShip(CrewEntity crew)
            {
                shipBar.DisplayShip(crew.HasCaptain ? MatchLobbyView.Instance.GetShipVO(crew.Id) : null);
            }
            public void DisplayLoadouts(CrewEntity crew)
            {
                for (var i = 0; i < 4; i++)
                {
                    var slot = crew.Slots[i];
                    crewBars[i].DisplayItems(slot.PlayerEntity);
                }
            }
            public void ShowShipBar() { shipBar.gameObject.SetActive(true); }
            public void HideShipBar() { shipBar.gameObject.SetActive(false); }
            public void ShowCrewBars() { foreach (var b in crewBars) b.SetVisible(true); }
            public void HideCrewBars() { foreach (var b in crewBars) b.SetVisible(false); }
        }

        static List<GameObject> crewProfileButtons;
        static List<List<ShipLoadoutBars>> loadoutBars;                         // Access by [column][row]
        static Dictionary<UILobbyCrew, ShipLoadoutBars> crewToLoadoutBar;
        public static void LobbyUIPostBuild(List<List<UILobbyCrew>> uimlCrewElements)
        {
            // Fill crewProfileButtons
            crewProfileButtons = new List<GameObject>();
            foreach (var column in uimlCrewElements)
                foreach (var crew in column)
                    for (var i = 0; i < 4; i++)
                        crewProfileButtons.Add(crew.transform.GetChild(i + 1).GetChild(2).gameObject);
            loadoutBars = new List<List<ShipLoadoutBars>>();
            crewToLoadoutBar = new Dictionary<UILobbyCrew, ShipLoadoutBars>();
            // crewElements is a 2-wide, 4-tall list
            // For each crew element, find the loadout bar and add to loadoutBars in the same order
            foreach (var column in uimlCrewElements)
            {
                var currentCol = new List<ShipLoadoutBars>();
                foreach (var crew in column)
                {
                    var bars = new ShipLoadoutBars(crew);
                    currentCol.Add(bars);
                    crewToLoadoutBar.Add(crew, bars);
                }
                loadoutBars.Add(currentCol);
            }
            // Apply faction display setting on startup.
            SetFactionIconVisibility(UIMatchLobby_Awake.FactionIconsVisible);
        }

        static bool _paintShipBars = true;
        static bool _paintGunBars = true;

        public static void PaintLoadoutBars(MatchLobbyView mlv)
        {
            if (!_paintShipBars && !_paintGunBars) return;
            if (mlv == null) return;
            // Update all UILobbyShipLoadoutBar in loadoutBars with ship data
            // Loop logic came from UIMatchLobby.PaintCrews
            int[] array = new int[loadoutBars.Count];
            for (int i = 0; i < mlv.Crews.Count; i++)
            {
                List<CrewEntity> list = mlv.Crews[i];
                int num = i % loadoutBars.Count;
                for (int j = 0; j < list.Count; j++)
                {
                    CrewEntity crewData = list[j];
                    if (array[num] < loadoutBars[num].Count)
                    {
                        if (_paintShipBars)
                            loadoutBars[num][array[num]].DisplayShip(crewData);
                        if (_paintGunBars)
                            loadoutBars[num][array[num]].DisplayLoadouts(crewData);
                        array[num]++;
                    }
                }
            }
        }

        public static void MarkShipBarsForRedraw()
        {
            // This is called every time LoadGunTextures loads a texture
            // Should result in icon textures being drawn as soon as they are loaded instead of on next PaintLoadoutBars call
            foreach (var barList in loadoutBars)
                foreach (var bar in barList)
                    bar.shipBar.MarkForRedraw = true;
        }
        public static void MarkCrewBarsForRedraw()
        {
            foreach (var barList in loadoutBars)
                foreach (var bar in barList)
                    foreach (var crewBar in bar.crewBars)
                        crewBar.MarkForRedraw = true;
        }

        public static void SetShipBarVisibility(bool isVisible)
        {
            _paintShipBars = isVisible;
            MuseLog.Info($"Setting ship bar visibility to {isVisible}");
            foreach (var barList in loadoutBars)
                foreach (var bar in barList)
                    if (isVisible)
                        bar.ShowShipBar();
                    else
                        bar.HideShipBar();
            //bar.shipBar.gameObject.SetActive(isVisible);
            PaintLoadoutBars(MatchLobbyView.Instance);
        }
        public static void SetCrewBarVisibility(bool isVisible)
        {
            _paintGunBars = isVisible;
            MuseLog.Info($"Setting crew bar visibility to {isVisible}");
            foreach (var barList in loadoutBars)
                foreach (var bar in barList)
                    if (isVisible)
                        bar.ShowCrewBars();
                    else
                        bar.HideCrewBars();
            PaintLoadoutBars(MatchLobbyView.Instance);
        }

        public static void SetFactionIconVisibility(bool isVisible)
        {
            UIMatchLobby_Awake.FactionIconsVisible = isVisible;
            MuseLog.Info($"Setting faction icon visibility to {isVisible}.");
            foreach (var barList in loadoutBars)
            {
                foreach (var bar in barList)
                {
                    foreach (var crewBar in bar.crewBars)
                    {
                        crewBar.SetFactionIconVisibility(isVisible);
                    }
                }
            }
            PaintLoadoutBars(MatchLobbyView.Instance);
        }

        public static void SetCrewBarOptions(ToggleGrid value)
        {
            UILobbyCrewLoadoutBar.SetEnabledToolSlotCount(value.Values);
            PaintLoadoutBars(MatchLobbyView.Instance);
        }

        public static void SetCrewProfileButtonVisibility(bool isVisible)
        {
            foreach (var btn in crewProfileButtons)
                btn.SetActive(isVisible);
        }

        public static void SetCrewLoadoutDisplaySeparator(bool isVisible)
        {
            foreach (var barList in loadoutBars)
                foreach (var bar in barList)
                    foreach (var crewBar in bar.crewBars)
                        crewBar.SetSeparatorVisibility(isVisible);
        }



        private static readonly Dictionary<int, int> _playerFactionPairs = [];

        public static Sprite GetPlayerFactionSprite(int playerId)
        {
            if (!_playerFactionPairs.ContainsKey(playerId)) return null;

            var factionId = _playerFactionPairs[playerId];
            return WorldMapFactionManager.GetFactionIconSprite(factionId, true);
        }

        /// <summary>
        /// Takes a <c>playerId</c> and checks if it's already added to <c>_playerFactionPairs</c>. 
        /// If not, call <c>GetUserProfile</c>, set the faction ID, and mark for redraw.
        /// The value of the pair is -1 when waiting for the API call to complete.
        /// </summary>
        /// <param name="playerId"></param>
        public static void DisplayPlayerFaction(int playerId)
        {
            if (_playerFactionPairs.ContainsKey(playerId)) return;

            MuseLog.Info($"Fetching faction ID for player ID {playerId}...");
            _playerFactionPairs[playerId] = -1;

            AccountActions.GetUserProfile(playerId,
                (UserProfile userProfile) =>
                {
                    _playerFactionPairs[playerId] = userProfile.FactionId;
                    MarkCrewBarsForRedraw();
                }
            );
        }

        /// <summary>
        /// Updates the faction ID for a player when the profile panel is activated and mark for redraw if in a lobby.
        /// </summary>
        /// <param name="user"></param>
        [HarmonyPatch(typeof(UIProfilePanel), nameof(UIProfilePanel.Instance.Activate))]
        [HarmonyPostfix]
        private static void UIProfilePanel_Activate_UpdateData(UserProfile user)
        {
            if (_playerFactionPairs.ContainsKey(user.Id) && _playerFactionPairs[user.Id] == user.FactionId)
            {
                return;
            }

            MuseLog.Info($"Player ID {user.Id} has a new faction {user.FactionId}!");
            _playerFactionPairs[user.Id] = user.FactionId;
            if (_paintGunBars && UIMatchLobby_Awake.FactionIconsVisible && MatchLobbyView.Instance != null)
            {
                MarkCrewBarsForRedraw();
            }
        }
    }
}
