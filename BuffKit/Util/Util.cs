﻿using LitJson;
using Muse.Common;
using Muse.Goi2.Entity;
using Muse.Goi2.Entity.Vo;
using MuseBase.Multiplayer;
using MuseBase.Multiplayer.Photon;
using MuseBase.Multiplayer.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Resources = BuffKit.UI.Resources;

namespace BuffKit
{
    public static class Util
    {
        public delegate void Notify();

        public static event Notify OnGameInitialize;     // Called once before the launcher "Play" button can be pressed
        public static event Notify OnLobbyLoad;          // Called when the game loads to menu and after every match

        // Practice, Pirate Deathmatch 1 ship, Pirate Deathmatch 2+ ships.
        public static readonly List<RegionGameMode> PracticeGameModes = [RegionGameMode.PRACTICE, RegionGameMode.NOVICE_DEATHMATCH, RegionGameMode.PRACTICE_NOVICE_DEATHMATCH];

        public static bool HasModPrivilege(MatchLobbyView mlv)
        {
            try
            {
                return mlv.Moderated && NetworkedPlayer.Local.Privilege >= UserPrivilege.Referee ||
                       NetworkedPlayer.Local.Privilege.IsModerator();
            }
            catch (NullReferenceException)
            {
                return false;
            }
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
                if (csvo.CrewId == crewId)
                    return ShipPreview.GetShipVO(csvo);
            return null;
        }

        public static List<int> PilotSkillIds { get; private set; }
        public static List<int> GunnerSkillIds { get; private set; }
        public static List<int> EngineerSkillIds { get; private set; }
        public static HashSet<int> ShipIds { get; private set; }
        public static HashSet<int> GunIds { get; private set; }
        private static Dictionary<int, Dictionary<string, int>> shipGunSlotLookup;

        public static int GetGunSlotIndex(int shipClass, string gunSlotName)
        {
            return shipGunSlotLookup[shipClass][gunSlotName];
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

            return shipGunIds; //.ToList();
        }

        public static string GetDamageTypeName(DamageType type)
        {
            switch (type)
            {
                case DamageType.FIRE:
                    return "Fire";
                case DamageType.HOOK:
                    return "Flechette";
                case DamageType.LIGHTNING:
                    return "Shatter";
                case DamageType.PIERCING:
                    return "Piercing";
                case DamageType.EXPLOSIVE:
                    return "Explosive";
                case DamageType.IMPACT:
                    return "Impact";
                default:
                    return "Unknown";
                    //case DamageType.ARMORONLY:
                    //    return "Armour only";
            }
        }

        public static string GetTeamName(int teamIndex)
        {
            switch (teamIndex)
            {
                case 0: return "Red";
                case 1: return "Blue";
                case 2: return "Yellow";
                case 3: return "Purple";
                default:
                    throw new ArgumentOutOfRangeException($"Team index value {teamIndex} out of range 0 to 3");
            }
        }
        public static Color GetTeamColor(int teamIndex)
        {
            switch (teamIndex)
            {
                case 0: return Resources.TeamRed;
                case 1: return Resources.TeamBlue;
                case 2: return Resources.TeamYellow;
                case 3: return Resources.TeamPurple;
                default:
                    throw new ArgumentOutOfRangeException($"Team index value {teamIndex} out of range 0 to 3");
            }
        }

        public static void OnLobbyLoadTrigger()
        {
            OnLobbyLoad?.Invoke();
        }

        public static void Initialize()
        {
            SubDataActions.GetShipAndGuns(delegate (JsonData data)
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
                shipGunSlotLookup = new Dictionary<int, Dictionary<string, int>>();
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

                    shipGunSlotLookup[ship.Id] = shipDict;
                }

                OnGameInitialize?.Invoke();
                OnLobbyLoadTrigger();
            });

            PilotSkillIds = new List<int>();
            GunnerSkillIds = new List<int>();
            EngineerSkillIds = new List<int>();

            var skills = CachedRepository.Instance.GetAll<SkillConfig>();
            foreach (var skill in skills)
            {
                if (skill.Public)
                {
                    switch (skill.Type)
                    {
                        case SkillType.Helm:
                            PilotSkillIds.Add(skill.ActivationId);
                            break;
                        case SkillType.Gun:
                            GunnerSkillIds.Add(skill.ActivationId);
                            break;
                        case SkillType.Repair:
                            EngineerSkillIds.Add(skill.ActivationId);
                            break;
                    }
                }
            }
            PilotSkillIds.Sort();
            GunnerSkillIds.Sort();
            EngineerSkillIds.Sort();
        }

        /// <summary>
        /// Queue a toast notification in top-right with title "System Message" and specified message. Can log to the notification panel.
        /// </summary>
        /// <param name="message">Message string. Can use <c>\n</c> for new line.</param>
        /// <param name="showInNotificationPanel">Log notification in the notification panel. Default <c>false</c>.</param>
        public static void SendToastNotification(string message, bool showInNotificationPanel = false)
        {
            var notificationType = showInNotificationPanel ? NotificationType.ServerAlert : NotificationType.ConsoleAlert;
            var messageDictionary = new Dictionary<string, object> {
                { "content", message }
            };
            NotificationDispatch.RaiseNotification(notificationType, messageDictionary);
        }

        /// <summary>
        /// Add a red console message to the chat. Only visible to the player.
        /// </summary>
        /// <param name="message">Message string. Can use <c>\n</c> for new line.</param>
        public static void SendConsoleChatMessage(string message)
        {
            MuseWorldClient.Instance.ChatHandler.AddMessage(ChatMessage.Console(message));
        }

    }
}