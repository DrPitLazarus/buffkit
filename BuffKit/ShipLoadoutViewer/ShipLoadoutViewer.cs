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
            var loadoutPanel = new GameObject("Loadout Panel");
            loadoutPanel.transform.parent = crewUI.transform;

            loadoutPanel.AddComponent<UILobbyShipLoadoutBar>();
        }

        static List<List<UILobbyShipLoadoutBar>> loadoutBars;                       // Access by [column][row]
        static Dictionary<UILobbyCrew, UILobbyShipLoadoutBar> crewToLoadoutBar;
        public static void LobbyUIPostBuild(List<List<UILobbyCrew>> uimlCrewElements)
        {
            loadoutBars = new List<List<UILobbyShipLoadoutBar>>();
            crewToLoadoutBar = new Dictionary<UILobbyCrew, UILobbyShipLoadoutBar>();
            // crewElements is a 2-wide, 4-tall list
            // For each crew element, find the loadout bar and add to loadoutBars in the same order
            foreach (var column in uimlCrewElements)
            {
                var currentCol = new List<UILobbyShipLoadoutBar>();
                foreach (var crew in column)
                {
                    var bar = crew.gameObject.transform.GetComponentInChildren<UILobbyShipLoadoutBar>();
                    bar.Build();
                    currentCol.Add(bar);
                    crewToLoadoutBar.Add(crew, bar);
                }
                loadoutBars.Add(currentCol);
            }
        }

        public static void PaintLoadoutBars(MatchLobbyView mlv)
        {
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
                        loadoutBars[num][array[num]].DisplayShip(crewData.HasCaptain ? mlv.GetShipVO(crewData.Id) : null);
                        array[num]++;
                    }
                }
            }
        }

        public static Dictionary<int, Texture2D> gunIcons;
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
                    log.LogInfo($"  Loaded icon texture for {gunItem.NameText.En}");
                    MarkBarsForRedraw();
                }, 0, false);
            }
        }
        static void MarkBarsForRedraw()
        {
            // This is called every time LoadGunTextures loads a texture
            // Should result in icon textures being drawn as soon as they are loaded instead of on next PaintLoadoutBars call
            foreach (var barList in loadoutBars)
                foreach (var bar in barList)
                    bar.MarkForRedraw = true;
        }

        public static void SetBarVisibility(bool isVisible)
        {
            foreach (var barList in loadoutBars)
                foreach (var bar in barList)
                    bar.gameObject.SetActive(isVisible);
        }
    }
}
