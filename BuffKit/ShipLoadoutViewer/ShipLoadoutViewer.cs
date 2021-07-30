using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Muse.Goi2.Entity;
using Muse.Goi2.Entity.Vo;

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
            crewUI.GetComponent<LayoutElement>().preferredHeight = 145;
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
            log.LogInfo($"Displaying loadout bars, active state: {loadoutBars[0][0].gameObject.activeInHierarchy}");
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

        public static Dictionary<int, Dictionary<string, int>> shipAndGunSlotToIndex;
        public static Dictionary<int, Texture2D> gunIcons;
        static List<GunItem> allGunItems;
        static bool dataLoaded = false;
        public static void LoadShipAndGunData()
        {
            /*
             * This method has 3 tasks
             *  1. Fill out shipAndGunSlotToIndex for sorting guns into the correct slot
             *  2. Fill out allGunItems for loading gun icon textures
             *  3. Load all gun textures
             * Task 1 & 2 are performed once.
             * Task 3 is performed every time the game connects to the lobby (UILoadingLobbyState.Exit)
             * TODO: Find a method that's only called once, but where SubDataActions can be used to load data.
             */
            if (!dataLoaded)
            {
                // Fill out shipAndGunSlotToIndex and allGunItems once

                gunIcons = new Dictionary<int, Texture2D>();
                shipAndGunSlotToIndex = new Dictionary<int, Dictionary<string, int>>();
                allGunItems = new List<GunItem>();

                SubDataActions.GetShipAndGuns(delegate (LitJson.JsonData data)
                {
                    // Fill allGunItems list with each GunItem available to the player
                    var allGunsJsonData = data["allguns"];
                    var allGunIdSet = new HashSet<int>();
                    for (int i = 0; i < allGunsJsonData.Count; i++) allGunIdSet.Add((int)allGunsJsonData[i]);
                    foreach (var g in allGunIdSet) allGunItems.Add(CachedRepository.Instance.Get<GunItem>(g));
                    // Load the gun icon textures
                    LoadGunTextures();

                    // Fill allShipModels list with each ShipModel available to the player
                    var allShipsJsonData = data["allships"];
                    var allShipIdSet = new HashSet<int>();
                    for (int i = 0; i < allShipsJsonData.Count; i++) allShipIdSet.Add((int)allShipsJsonData[i]);
                    var allShipModels = new List<ShipModel>();
                    foreach (var s in allShipIdSet) allShipModels.Add(CachedRepository.Instance.Get<ShipModel>(s));
                    // For each ShipModel create a dictionary from each gun slot name to its corresponding index in the ship loadout order
                    foreach (var ship in allShipModels)
                    {
                        // Logic from UINewShipState.MainMode
                        var gunSlots = new List<ShipSlotViewObject>();
                        foreach (string text in
                            from p in ship.Slots
                            where p.Value.SlotType == Muse.Goi2.Entity.ShipPartSlotType.GUN
                            select p.Key)
                        {
                            ShipSlotViewObject item = new ShipSlotViewObject
                            {
                                Name = text,
                                Size = ship.Slots[text].SlotSize,
                                GunId = 0
                            };
                            gunSlots.Add(item);
                        }
                        var sortedSlots = (from slot in gunSlots
                                           orderby slot.Size descending,
                                           ship.Slots[slot.Name].Position.Z descending,
                                           ship.Slots[slot.Name].Position.X descending,
                                           ship.Slots[slot.Name].Position.Y,
                                           slot.Name
                                           select slot).ToList();
                        var shipDict = new Dictionary<string, int>();

                        for (int i = 0; i < sortedSlots.Count; i++)
                            shipDict[sortedSlots[i].Name] = i;

                        shipAndGunSlotToIndex[ship.Id] = shipDict;
                    }
                    dataLoaded = true;
                });
            }
            else
                LoadGunTextures();
        }
        static void LoadGunTextures()
        {
            // Load the actual icons
            log.LogInfo("Loading gun icon textures");
            foreach (var g in allGunItems)
            {
                MuseBundleStore.Instance.LoadObject<Texture2D>(g.GetIcon(), delegate (Texture2D t)
                {
                    gunIcons[g.Id] = t;
                    log.LogInfo($"  Loaded icon texture for {g.NameText.En}");
                }, 0, false);
            }
        }
    }
}
