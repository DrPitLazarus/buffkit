﻿using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BuffKit.ChaosRandomizer
{
    public class ChaosRandomizer
    {
        BepInEx.Logging.ManualLogSource log;
        private ChaosRandomizer()
        {
            log = BepInEx.Logging.Logger.CreateLogSource("chaosrandomizer");
            CreatePanel();
        }
        public void EnterLobby(MatchLobbyView mlv)
        {
            log.LogInfo("Entered lobby");
            _obPanel.SetActive(Util.Util.HasModPrivilege(mlv));
            SetInfoLabel(string.Empty);
            _canAnnounce = false;
        }
        public void ExitLobby(MatchLobbyView mlv)
        {
            log.LogInfo("Exited lobby");
            HidePanelContent();
            ClearPanelContent();
            _obPanel.SetActive(false);
        }

        private GameObject _obPanel;
        private GameObject _obContent;
        private GameObject _obAssignedPlayers;
        private GameObject _obUnassignedPlayers;
        private List<UIChaosShipDisplay> _listAssignedShipDisplays;
        private List<TextMeshProUGUI> _listUnassignedPlayerLabels;
        private TextMeshProUGUI _lInfo;                                 // Info label giving feedback to user
        private GameObject _obAssignedPlayersHeading;
        private GameObject _obUnassignedPlayersHeading;
        private bool _canAnnounce;
        private void CreatePanel()
        {
            var parent = GameObject.Find("/Menu UI/Standard Canvas/Common Elements").transform;
            _obPanel = UI.Builder.BuildPanel(parent);
            _obPanel.name = "Chaos Randomizer Panel";
            var rt = _obPanel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, .5f);
            rt.anchorMax = new Vector2(1, .5f);
            rt.pivot = new Vector2(1, .5f);
            var csf = _obPanel.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var hlg = _obPanel.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(7, 7, 7, 7);
            hlg.spacing = 7;

            var obBtnShowHide = UI.Builder.BuildButton(_obPanel.transform, delegate
            {
                if (_isShown)
                    HidePanelContent();
                else
                    ShowPanelContent();
            }, "");
            _lShowHide = obBtnShowHide.GetComponentInChildren<TextMeshProUGUI>();
            var le = obBtnShowHide.AddComponent<LayoutElement>();
            le.preferredWidth = 30;
            le.preferredHeight = 30;
            _isShown = false;

            _obContent = new GameObject("content");
            _obContent.transform.SetParent(_obPanel.transform, false);
            var vlg = _obContent.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 5;
            vlg.padding = new RectOffset(5, 5, 5, 5);

            UI.Builder.BuildLabel(_obContent.transform, out _lInfo, UI.Resources.FontGaldeanoRegular, TextAnchor.MiddleCenter, 13);
            SetInfoLabel(string.Empty);

            var obPlayerLists = new GameObject("player lists");
            obPlayerLists.transform.SetParent(_obContent.transform, false);
            hlg = obPlayerLists.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 5;
            hlg.childForceExpandHeight = false;

            _obAssignedPlayers = new GameObject("assigned players");
            _obAssignedPlayers.transform.SetParent(obPlayerLists.transform, false);
            vlg = _obAssignedPlayers.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 1;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            vlg.childAlignment = TextAnchor.UpperCenter;
            _listAssignedShipDisplays = new List<UIChaosShipDisplay>();

            _obUnassignedPlayers = new GameObject("unassigned players");
            _obUnassignedPlayers.transform.SetParent(obPlayerLists.transform, false);
            vlg = _obUnassignedPlayers.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 1;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            vlg.childAlignment = TextAnchor.MiddleLeft;
            _listUnassignedPlayerLabels = new List<TextMeshProUGUI>();

            _obAssignedPlayersHeading = UI.Builder.BuildLabel(_obAssignedPlayers.transform, out var lAssignedTitle, UI.Resources.FontGaldeanoRegular, TextAnchor.MiddleCenter, 13);
            lAssignedTitle.text = "Assigned Ships";
            _obUnassignedPlayersHeading = UI.Builder.BuildLabel(_obUnassignedPlayers.transform, out var lUnassignedTitle, UI.Resources.FontGaldeanoRegular, TextAnchor.MiddleCenter, 13);
            lUnassignedTitle.text = "Unassigned Players";

            var obButtons = new GameObject("buttons");
            obButtons.transform.SetParent(_obContent.transform, false);
            hlg = obButtons.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 5;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleCenter;

            var obBtnRandomize = UI.Builder.BuildButton(obButtons.transform, delegate { Randomize(MatchLobbyView.Instance); }, "Randomize");
            le = obBtnRandomize.AddComponent<LayoutElement>();
            le.preferredWidth = 130;
            le.preferredHeight = 30;

            var obBtnAnnounce = UI.Builder.BuildButton(obButtons.transform, AnnouncePlayers, "Announce");
            le = obBtnAnnounce.AddComponent<LayoutElement>();
            le.preferredWidth = 130;
            le.preferredHeight = 30;

            _obPanel.AddComponent<GraphicRaycaster>();
            HidePanelContent();
            ClearPanelContent();
            _obPanel.SetActive(false);
        }

        private bool _isShown;
        private TextMeshProUGUI _lShowHide;
        private void ShowPanelContent()
        {
            _isShown = true;
            _lShowHide.text = ">>";
            _obContent.SetActive(true);
        }
        private void HidePanelContent()
        {
            _isShown = false;
            _lShowHide.text = "<<";
            _obContent.SetActive(false);
        }
        public bool TryHidePanel()
        {
            if (!_isShown) return false;
            HidePanelContent();
            return true;
        }
        private void ClearPanelContent()
        {
            _unassignedPlayers = new List<string>();
            _assignedShips = new List<LobbyShip>();
            foreach (var v in _listAssignedShipDisplays)
                v.Hide();
            foreach (var v in _listUnassignedPlayerLabels)
                v.gameObject.SetActive(false);
            _obAssignedPlayersHeading.SetActive(false);
            _obUnassignedPlayersHeading.SetActive(false);
        }

        private void Randomize(MatchLobbyView mlv)
        {
            log.LogInfo($"Chaos randomizer button pressed");
            var allPlayers = new List<string>();
            foreach (var crew in mlv.ValidCrews)
                foreach (var player in crew.CrewMembers)
                    allPlayers.Add(player.Name.Substring(0, player.Name.Length - 5));

            // Add fake players (for testing)
            //for (var i = 0; i < 20; i++)
            //    allPlayers.Add($"player {i}");

            if (Randomize(allPlayers, mlv))
            {
                DisplayPlayers();
                SetInfoLabel(string.Empty);
                _canAnnounce = true;
            }
        }

        private List<string> _unassignedPlayers;
        private List<LobbyShip> _assignedShips;
        private bool Randomize(List<string> allPlayers, MatchLobbyView mlv)
        {
            var requiredShipCount = mlv.FlatCrews.Count;

            log.LogInfo($"Randomize invoked with {allPlayers.Count} players, requesting {requiredShipCount} out");
            var playerCount = allPlayers.Count;
            if (playerCount < requiredShipCount)
            {
                SetInfoLabel($"Not enough players in lobby: need {requiredShipCount} but only have {_unassignedPlayers.Count}");
                return false;
            }
            // Shuffle allPlayers into unassignedPlayers
            _unassignedPlayers = new List<string>();
            var rnd = new System.Random();
            for (var i = 0; i < playerCount; i++)
            {
                var ind = rnd.Next(allPlayers.Count);
                _unassignedPlayers.Add(allPlayers[ind]);
                allPlayers.RemoveAt(ind);
            }
            // Take as many players as required
            _assignedShips = new List<LobbyShip>();
            var teamShipCounter = new int[mlv.TeamCount];
            foreach (var crew in mlv.FlatCrews)
            {
                var crewTeam = crew.Team;
                var ship = new LobbyShip { crew1 = _unassignedPlayers[0], crew2 = _unassignedPlayers[1], team = crewTeam, teamShip = teamShipCounter[crewTeam] };
                teamShipCounter[crewTeam]++;
                _unassignedPlayers.RemoveAt(0);
                _unassignedPlayers.RemoveAt(0);
                _assignedShips.Add(ship);
            }
            return true;
        }

        private void DisplayPlayers()
        {
            log.LogInfo("Displaying players");
            var assignedCount = _assignedShips.Count;
            var unassignedCount = _unassignedPlayers.Count;

            // Build assigned player labels if required
            for (var i = _listAssignedShipDisplays.Count; i < assignedCount; i++)
            {
                var obShipDisplay = new GameObject("ship display");
                obShipDisplay.transform.SetParent(_obAssignedPlayers.transform, false);
                _listAssignedShipDisplays.Add(obShipDisplay.AddComponent<UIChaosShipDisplay>());
            }
            // Fill assigned player labels
            for (var i = 0; i < assignedCount; i++)
            {
                var shipData = _assignedShips[i];
                _listAssignedShipDisplays[i].SetValues(shipData.team, shipData.teamShip, shipData.crew1, shipData.crew2);
            }
            // Hide any extra labels
            for (var i = assignedCount; i < _listAssignedShipDisplays.Count; i++)
            {
                _listAssignedShipDisplays[i].Hide();
            }

            // Build unassigned player labels if required
            for (var i = _listUnassignedPlayerLabels.Count; i < unassignedCount; i++)
            {
                UI.Builder.BuildLabel(_obUnassignedPlayers.transform, out var label, UI.Resources.FontGaldeanoRegular, TextAnchor.MiddleLeft, 13);
                _listUnassignedPlayerLabels.Add(label);
            }
            // Fill unassigned player labels
            for (var i = 0; i < unassignedCount; i++)
            {
                _listUnassignedPlayerLabels[i].text = _unassignedPlayers[i];
                _listUnassignedPlayerLabels[i].gameObject.SetActive(true);
            }
            // Hide any extra labels
            for (var i = unassignedCount; i < _listUnassignedPlayerLabels.Count; i++)
            {
                _listUnassignedPlayerLabels[i].gameObject.SetActive(false);
            }
            // Show heading labels
            _obAssignedPlayersHeading.SetActive(true);
            _obUnassignedPlayersHeading.SetActive(true);
        }

        private void SetInfoLabel(string text)
        {
            log.LogInfo($"Set info label text to {(text != string.Empty ? text : "[empty]")}");
            _lInfo.text = text;
            _lInfo.gameObject.SetActive(text != string.Empty);
        }

        private void AnnouncePlayers()
        {
            if (_canAnnounce)
            {
                SetInfoLabel(string.Empty);
                var sb = new StringBuilder();
                sb.Append("Randomized lobby positions");
                foreach (var ship in _assignedShips)
                {
                    sb.Append($"\n{Util.Util.GetTeamName(ship.team)} {ship.teamShip + 1}: {ship.crew1}, {ship.crew2}");
                }
                Util.Util.ForceSendMessage(sb.ToString());
            }
            else
            {
                SetInfoLabel("Nothing to announce");
            }
        }

        private struct LobbyShip
        {
            public int team;
            public int teamShip;
            public string crew1;
            public string crew2;
        }

        private class UIChaosShipDisplay : MonoBehaviour
        {
            private TextMeshProUGUI _lShip, _lCrew1, _lCrew2;
            private Image _background;
            private void Awake()
            {
                var hlg = gameObject.AddComponent<HorizontalLayoutGroup>();
                hlg.spacing = 1;
                hlg.childForceExpandHeight = false;
                hlg.childForceExpandWidth = false;

                var obShip = new GameObject("ship");
                obShip.transform.SetParent(transform, false);
                var le = obShip.AddComponent<LayoutElement>();
                le.preferredWidth = 20;
                le.preferredHeight = 29;
                _background = obShip.AddComponent<Image>();
                hlg = obShip.AddComponent<HorizontalLayoutGroup>();
                hlg.childAlignment = TextAnchor.MiddleCenter;
                hlg.childForceExpandHeight = false;
                hlg.childForceExpandWidth = false;

                var obShipLabel = new GameObject("label");
                obShipLabel.transform.SetParent(obShip.transform, false);
                _lShip = obShipLabel.AddComponent<TextMeshProUGUI>();
                _lShip.font = UI.Resources.FontGaldeanoRegular;
                _lShip.fontSize = 13;
                _lShip.alignment = TextAlignmentOptions.Center;

                var obCrew = new GameObject("crew");
                obCrew.transform.SetParent(transform, false);
                var vlg = obCrew.AddComponent<VerticalLayoutGroup>();
                vlg.spacing = 1;
                vlg.padding = new RectOffset(1, 1, 1, 1);
                vlg.childForceExpandHeight = false;

                var obCrew1 = new GameObject("crew 1");
                obCrew1.transform.SetParent(obCrew.transform, false);
                _lCrew1 = obCrew1.AddComponent<TextMeshProUGUI>();
                _lCrew1.font = UI.Resources.FontGaldeanoRegular;
                _lCrew1.fontSize = 13;

                var obCrew2 = new GameObject("crew 2");
                obCrew2.transform.SetParent(obCrew.transform, false);
                _lCrew2 = obCrew2.AddComponent<TextMeshProUGUI>();
                _lCrew2.font = UI.Resources.FontGaldeanoRegular;
                _lCrew2.fontSize = 13;
            }
            public void Hide()
            {
                gameObject.SetActive(false);
            }
            public void SetValues(int team, int ship, string crew1, string crew2)
            {
                _background.color = Util.Util.GetTeamColor(team);
                _lShip.text = $"{ship + 1}";
                _lCrew1.text = crew1;
                _lCrew2.text = crew2;
                gameObject.SetActive(true);
            }
        }


        public static ChaosRandomizer Instance { get; private set; }
        public static void Initialize()
        {
            Instance = new ChaosRandomizer();
        }
    }
}
