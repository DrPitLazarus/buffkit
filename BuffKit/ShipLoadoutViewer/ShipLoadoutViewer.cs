using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Muse.Goi2.Entity;
using static BuffKit.Util.Util;

namespace BuffKit.ShipLoadoutViewer
{
    class ShipLoadoutViewer
    {
        static BepInEx.Logging.ManualLogSource log;
        public static void CreateLog()
        {
            if (log == null)
            {
                log = BepInEx.Logging.Logger.CreateLogSource("shiploadoutviewer");
            }
        }


        public static void LobbyUIPreBuild(UIMatchLobby uiml)
        {
            // Edit the "Sample Crew" object to have a loadout panel
            var crewUI = uiml.sampleCrew;
            Object.Destroy(crewUI.GetComponent<LayoutElement>());                   // Instead of setting the LayoutElement preferredHeight just delete it
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
                    crewBars[i].DisplayItems(slot.PlayerEntity, _crewToolSettings);
                }
            }
            public void ShowShipBar() { shipBar.gameObject.SetActive(true); }
            public void HideShipBar() { shipBar.gameObject.SetActive(false); }
            public void ShowCrewBars() { foreach (var b in crewBars) b.gameObject.SetActive(true); }
            public void HideCrewBars() { foreach (var b in crewBars) b.gameObject.SetActive(false); }
            /*
             * Add checks for when to paint bars based on the show/hide status (in Paint...)
             * Call (Paint...) when setting changes to true
             */
        }

        static List<List<ShipLoadoutBars>> loadoutBars;                         // Access by [column][row]
        static Dictionary<UILobbyCrew, ShipLoadoutBars> crewToLoadoutBar;
        public static void LobbyUIPostBuild(List<List<UILobbyCrew>> uimlCrewElements)
        {
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

        public static Dictionary<int, Texture2D> gunIcons;
        public static Dictionary<int, Texture2D> skillIcons;
        public static void LoadGunTextures()
        {
            // Load the actual icons
            log.LogInfo("Loading gun icon textures");
            gunIcons = new Dictionary<int, Texture2D>();
            foreach (var gunId in GunIds)
            {
                var gunItem = CachedRepository.Instance.Get<GunItem>(gunId);
                MuseBundleStore.Instance.LoadObject<Texture2D>(gunItem.GetIcon(), delegate (Texture2D t)
                {
                    gunIcons[gunId] = t;
                    //log.LogInfo($"  Loaded icon texture for gun: {gunItem.NameText.En}");
                    MarkShipBarsForRedraw();
                }, 0, false);
            }
        }
        public static void LoadSkillTextures()
        {
            log.LogInfo("Loading skill icon textures");
            skillIcons = new Dictionary<int, Texture2D>();
            var allSkills = CachedRepository.Instance.GetAll<SkillConfig>();
            foreach (var sk in allSkills)
            {
                var id = sk.ActivationId;
                MuseBundleStore.Instance.LoadObject<Texture2D>(sk.GetIcon(), delegate (Texture2D t)
                {
                    if (t != null)
                    {
                        skillIcons[id] = t;
                        //log.LogInfo($"  Loaded icon texture for skill: {sk.NameText.En}");
                    }
                }, 0, false);
            }
        }
        static void MarkShipBarsForRedraw()
        {
            // This is called every time LoadGunTextures loads a texture
            // Should result in icon textures being drawn as soon as they are loaded instead of on next PaintLoadoutBars call
            foreach (var barList in loadoutBars)
                foreach (var bar in barList)
                    bar.shipBar.MarkForRedraw = true;
        }
        static void MarkCrewBarsForRedraw()
        {
            foreach (var barList in loadoutBars)
                foreach (var bar in barList)
                    foreach (var crewBar in bar.crewBars)
                        crewBar.MarkForRedraw = true;
        }

        public static void SetShipBarVisibility(bool isVisible)
        {
            log.LogInfo($"Setting ship bar visibility to {isVisible}");
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
            log.LogInfo($"Setting crew bar visibility to {isVisible}");
            foreach (var barList in loadoutBars)
                foreach (var bar in barList)
                    if (isVisible)
                        bar.ShowCrewBars();
                    else
                        bar.HideCrewBars();
            PaintLoadoutBars(MatchLobbyView.Instance);
        }

        private static bool[,] _crewToolSettings;
        public static void SetCrewBarOptions(Settings.ToggleGrid value)
        {
            _crewToolSettings = value.Values;
            UILobbyCrewLoadoutBar.SetEnabledToolSlotCount(_crewToolSettings);
            PaintLoadoutBars(MatchLobbyView.Instance);
        }

    }
}
