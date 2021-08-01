using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Muse.Common;
using Muse.Goi2.Entity;
using Muse.Goi2.Entity.Vo;
using MuseBase.Multiplayer.Unity;
using MuseBase.Multiplayer.Photon;

namespace BuffKit.Util
{
    public static class Util
    {
        public static bool HasModPrivilege(MatchLobbyView mlv)
        {
            return mlv.Moderated && NetworkedPlayer.Local.Privilege >= UserPrivilege.Referee ||
                   NetworkedPlayer.Local.Privilege.IsModerator();
        }
        
        public static void TrySendMessage(string message, string channel = "match")
        {
            MuseWorldClient.Instance.ChatHandler.TrySendMessage(message, channel);
        }

		public static void ForceSendMessage(string msg, string channel = "match")
		{
            // Note: This might break for PMs
			if (string.IsNullOrEmpty(msg))
			{
				return;
			}
            MuseWorldClient.Instance.Client.SendChatMessage(msg, channel);
		}

		public static string GetHierarchyPath(this Transform t)
        {
            string s = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                s = t.name + "/" + s;
            }
            return s;
        }

        public static ShipViewObject GetShipVO(this MatchLobbyView mlv, string crewId)
        {
            foreach (var csvo in mlv.CrewShips)
                if (csvo.CrewId == crewId) return ShipPreview.GetShipVO(csvo);
            return null;
        }

        public delegate void Notify();
        public static event Notify OnGameInitialize;
        public static event Notify OnLobbyLoad;

        public static HashSet<int> ShipIds { get; private set; }
        public static HashSet<int> GunIds { get; private set; }
        private static Dictionary<int, Dictionary<string, int>> _shipGunSlotLookup;
        public static int GetGunSlotIndex(int shipClass, string gunSlotName)
        {
            return _shipGunSlotLookup[shipClass][gunSlotName];
        }
        public static int[] GetSortedGunIds(ShipModel shipModel, IList<ShipSlotViewObject> gunSlots)
        {
            var shipGunIds = new int[shipModel.GunSlots];
            for (var i = 0; i < shipModel.GunSlots; i++) shipGunIds[i] = -1;
            var shipClass = shipModel.Id;
            foreach (var slot in gunSlots)
            {
                var slotIndex = GetGunSlotIndex(shipClass, slot.Name);
                shipGunIds[slotIndex] = slot.GunId;
            }
            return shipGunIds;//.ToList();
        }

        public static void _OnLobbyLoadTrigger() { OnLobbyLoad?.Invoke(); }
        public static void _Initialize()
        {
            SubDataActions.GetShipAndGuns(delegate (LitJson.JsonData data)
            {
                var allGunsJsonData = data["allguns"];
                GunIds = new HashSet<int>();
                for (int i = 0; i < allGunsJsonData.Count; i++) GunIds.Add((int)allGunsJsonData[i]);

                var allShipsJsonData = data["allships"];
                ShipIds = new HashSet<int>();
                for (int i = 0; i < allShipsJsonData.Count; i++) ShipIds.Add((int)allShipsJsonData[i]);
                var allShipModels = new List<ShipModel>();
                foreach (var s in ShipIds) allShipModels.Add(CachedRepository.Instance.Get<ShipModel>(s));
                // For each ShipModel create a dictionary from each gun slot name to its corresponding index in the ship loadout order
                _shipGunSlotLookup = new Dictionary<int, Dictionary<string, int>>();
                foreach (var ship in allShipModels)
                {
                    // Logic from UINewShipState.MainMode
                    var gunSlots = new List<ShipSlotViewObject>();
                    foreach (string text in
                        from p in ship.Slots
                        where p.Value.SlotType == ShipPartSlotType.GUN
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

                    _shipGunSlotLookup[ship.Id] = shipDict;
                }
                OnGameInitialize?.Invoke();
                _OnLobbyLoadTrigger();
            });
        }
    }
}