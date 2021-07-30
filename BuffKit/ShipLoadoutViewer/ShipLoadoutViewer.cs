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
            var crewUI = uiml.sampleCrew;
            crewUI.GetComponent<LayoutElement>().preferredHeight = 145;
            var loadoutPanel = new GameObject("Loadout Panel");
            loadoutPanel.transform.parent = crewUI.transform;

            var bar = loadoutPanel.AddComponent<UILobbyShipLoadoutBar>();
        }

        static List<List<UILobbyShipLoadoutBar>> loadoutBars;       // Access by [column][row]
        public static void LobbyUIPostBuild(UIMatchLobby uiml, List<List<UILobbyCrew>> uimlCrewElements)
        {
            loadoutBars = new List<List<UILobbyShipLoadoutBar>>();
            // crewElements is a 2-wide, 4-tall list
            // For each crew element, find the loadout bar and add to loadoutBars in the same order
            foreach(var column in uimlCrewElements)
            {
                var currentCol = new List<UILobbyShipLoadoutBar>();
                foreach(var crew in column)
                {
                    var bar = crew.gameObject.transform.GetComponentInChildren<UILobbyShipLoadoutBar>();
                    bar.Build(log);
                    currentCol.Add(bar);
                }
                loadoutBars.Add(currentCol);
            }
        }

        public static void LobbyDataChanged(MatchLobbyView mlv)
        {
            EnsureDataIsLoaded();
            // Update all UILobbyShipLoadoutBar in loadoutBars with ship data
            // Loop logic came from UIMatchLobby.PaintCrews - check there if this breaks
            int[] array = new int[loadoutBars.Count];
            for(int i = 0; i < mlv.Crews.Count; i++)
            {
                List<CrewEntity> list = mlv.Crews[i];
                int num = i % loadoutBars.Count;
                for(int j = 0; j < list.Count; j++)
                {
                    CrewEntity crewData = list[j];
                    if(array[num] < loadoutBars[num].Count)
                    {
                        loadoutBars[num][array[num]].DisplayShip(crewData.HasCaptain ? GetShipVOFromCrewId(mlv, crewData.Id) : null);
                        array[num]++;
                    }
                }
            }
        }
        /* Util functions */

        public static Dictionary<int, Dictionary<string, int>> shipAndGunSlotToIndex = new Dictionary<int, Dictionary<string, int>>();
        public static Dictionary<int, Texture2D> gunIcons = new Dictionary<int, Texture2D>();
        public static bool dataLoaded = false;
        public static void EnsureDataIsLoaded()
        {
            dataLoaded = gunIcons.Count > 0;
            if (!dataLoaded)
            {
                log.LogInfo("Trying to load data");
                SubDataActions.GetShipAndGuns(delegate (LitJson.JsonData data)
                {
                    var allGunsData = data["allguns"];
                    var allGuns = new HashSet<int>();
                    for (int i = 0; i < allGunsData.Count; i++) allGuns.Add((int)allGunsData[i]);
                    var allGunItems = new List<GunItem>();
                    foreach (var g in allGuns) allGunItems.Add(CachedRepository.Instance.Get<GunItem>(g));

                    var str = "allGuns:";
                    foreach (var g in allGunItems)
                    {
                        str += $"\n  {g.Id} : {g.Name}|{g.NameText.En}";
                        MuseBundleStore.Instance.LoadObject<Texture2D>(g.GetIcon(), delegate (Texture2D t)
                        {
                            gunIcons[g.Id] = t;
                            log.LogInfo($"Loaded icon for {g.NameText.En}");
                        }, 0, false);
                    }
                    log.LogInfo(str);

                    var allShipsData = data["allships"];
                    var allShips = new HashSet<int>();
                    for (int i = 0; i < allShipsData.Count; i++) allShips.Add((int)allShipsData[i]);
                    var allShipModels = new List<ShipModel>();
                    foreach (var s in allShips) allShipModels.Add(CachedRepository.Instance.Get<ShipModel>(s));
                    foreach(var ship in allShipModels)
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

                    str = "allShips:";
                    foreach (var s in allShipModels) str += $"\n  {s.Id} : {s.NameText.En}";
                    log.LogInfo(str);
                });
            }
        }
        static ShipViewObject GetShipVOFromCrewId(MatchLobbyView mlv, string crewId)
        {
            foreach (var csvo in mlv.CrewShips)
                if (csvo.CrewId == crewId) return ShipPreview.GetShipVO(csvo);
            return null;
        }
    }
}
