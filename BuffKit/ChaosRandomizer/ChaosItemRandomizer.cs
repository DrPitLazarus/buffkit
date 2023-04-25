using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BuffKit.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;
using Resources = BuffKit.UI.Resources;
using Util = BuffKit.Util;
using Muse.Goi2.Entity;

namespace BuffKit.ChaosRandomizer
{
    public class ChaosItemRandomizer
    {

        private Dictionary<string, string> _friendlyGunNames = new Dictionary<string, string>
        {
            // Skirmish Heavy Guns
            { "Manticore Heavy Hwacha", "Hwacha" },
            { "Lumberjack Heavy Mortar", "Lumberjack" },
            { "Typhon Heavy Flak Cannon Mk. I", "Flak Mk.I" },
            { "Nemesis Heavy Carronade", "Nemesis" },
            { "Hellhound Heavy Twin Carronade", "Hellhound" },
            { "Minotaur Heavy Cannon", "Minotaur" },
            { "Typhon Heavy Flak Cannon [Mk. II]", "Flak Mk.II" },
            { "Roaring Tiger Heavy Detonator [Mk. S]", "Detonator" },
            // Skirmish light guns
            { "Whirlwind Light Gatling Gun", "Gatling" },
            { "Hades Light Cannon", "Hades" },
            { "Mercury Field Gun", "Mercury" },
            { "Aten Lens Array [Mk. S]", "Aten" },
            { "Scylla Double-Barreled Mortar", "Mortar" },
            { "Banshee Light Rocket Carousel", "Banshee" },
            { "Echidna Light Flak Cannon", "Light Flak" },
            { "Artemis Light Rocket Launcher", "Artemis" },
            { "Seraph Tempest Missiles [Mk. S]", "Tempest" },
            { "Barking Dog Light Carronade", "Light Carronade" },
            { "Dragon Tongue Light Flamethrower", "Flamer" },
            { "Javelin Light Harpoon Gun", "Harpoon" },
            { "Phobos Light Mine Launcher", "Mine Launcher" },
            { "Beacon Flare Gun", "Flare Gun" },
            // Co-op Heavy Guns
            { "Roaring Tiger Heavy Detonator", "Detonator Mk. I" },
            { "Immortal Gaze Heavy Accelerator", "Accelerator Mk. I" },
            { "Roaring Tiger Heavy Detonator [Mk. II]", "Detonator Mk. II" },
            { "Immortal Gaze Heavy Accelerator [Mk. II]", "Accelerator Mk. II" },
            // Co-op Light Guns
            { "Februus Weaponized Coil", "Februus Mk. I" },
            { "Kalakuta Gas Mortar", "Gas Mortar Mk. I" },
            { "Seraph Tempest Missiles", "Tempest Mk. I" },
            { "Aten Lens Array", "Aten Mk. I" },
            { "Februus Weaponized Coil [Mk. II]", "Februus Mk. II" },
            { "Aten Lens Array [Mk. II]", "Aten Mk. II" },
            { "Kalakuta Gas Mortar [Mk. II]", "Gas Mortar Mk. II" },
            { "Seraph Tempest Missiles [Mk. II]", "Tempest Mk. II" },
        };

        private List<string> _shipNames = new List<string>();
        private Dictionary<string, bool> _shipCanUseHeavyGuns = new Dictionary<string, bool>();
        private Dictionary<GameType, List<string>> _lightGuns = new Dictionary<GameType, List<string>>();
        private Dictionary<GameType, List<string>> _heavyGuns = new Dictionary<GameType, List<string>>();

        private string TryGetFriendlyGunName(string gunName)
        {
            if (_friendlyGunNames.ContainsKey(gunName))
                return _friendlyGunNames[gunName];
            return gunName;
        }

        private ChaosItemRandomizer()
        {
            foreach (var gameType in new GameType[] { GameType.Skirmish, GameType.Coop })
            {
                _lightGuns.Add(gameType, new List<string>());
                _heavyGuns.Add(gameType, new List<string>());
            }
            foreach (var id in Util.ShipIds)
            {
                var ship = CachedRepository.Instance.Get<ShipModel>(id);

                var shipName = ship.NameText.En;
                _shipNames.Add(shipName);

                var canUseHeavyGuns = false;
                foreach (var kvp in ship.Slots)
                {
                    var v = kvp.Value;
                    if (v.SlotType == ShipPartSlotType.GUN && v.SlotSize == ShipPartSlotSize.MEDIUM)
                        canUseHeavyGuns = true;
                }
                _shipCanUseHeavyGuns.Add(shipName, canUseHeavyGuns);
            }
            foreach (var id in Util.GunIds)
            {
                var gun = CachedRepository.Instance.Get<GunItem>(id);
                // Some guns are available in only Skirmish or only Coop, and some are in both
                // Check GameType flags
                var skirmishValid = (gun.GameType & GameType.Skirmish) == GameType.Skirmish;
                var allianceValid = (gun.GameType & GameType.Coop) == GameType.Coop;
                // If the gun is light add it to lightGuns, otherwise add it to heavyGuns
                var isLight = gun.Size == ShipPartSlotSize.SMALL;
                if (skirmishValid)
                    (isLight ? _lightGuns : _heavyGuns)[GameType.Skirmish].Add(TryGetFriendlyGunName(gun.NameText.En));
                if (allianceValid)
                    (isLight ? _lightGuns : _heavyGuns)[GameType.Coop].Add(TryGetFriendlyGunName(gun.NameText.En));
                // Output to log if any gun name doesn't have a friendly alternative
                if (!_friendlyGunNames.ContainsKey(gun.NameText.En))
                    MuseLog.InfoFormat("No friendly gun entry for \'{0}\'", new object[]
                    {
                        gun.NameText.En
                    });
            }
        }

        public bool DoRandomize(int itemCount, GameType gamemode, out List<string> output)
        {
            var rng = new Random();
            output = new List<string>();
            // Initial checks
            if (itemCount > 10 || itemCount < 1)
            {
                return false;
            }
            if (gamemode != GameType.Skirmish && gamemode != GameType.Coop)
            {
                return false;
            }

            // Determine how many items per category
            var itemsPerCategory = new int[] { 1, 1, 1 };
            if (itemCount == 1)
            {
                // Only 1 item requested, so only 1 category has any items
                var category = rng.Next(3);
                itemsPerCategory[0] = category == 0 ? 1 : 0;
                itemsPerCategory[1] = category == 1 ? 1 : 0;
                itemsPerCategory[2] = category == 2 ? 1 : 0;
            }
            else if (itemCount == 2)
            {
                // Only 2 items requested, so only 1 category has no items
                var notCategory = rng.Next(3);
                itemsPerCategory[0] = notCategory == 0 ? 0 : 1;
                itemsPerCategory[1] = notCategory == 1 ? 0 : 1;
                itemsPerCategory[2] = notCategory == 2 ? 0 : 1;
            }
            for (var i = 3; i < itemCount; i++)
            {
                // 3+ items requested, so for each item over 3, increment category item count by 1
                itemsPerCategory[rng.Next(3)]++;
            }

            // Select ships
            var shipList = GetShips(itemsPerCategory[0], rng);

            // Determine if heavy guns can be used
            bool canUseHeavyGuns = false;
            if (itemsPerCategory[0] == 0)
                // No ship specified, so any ship is allowed - heavy guns can be used
                canUseHeavyGuns = true;
            else
                // If any ship specified can use heavy guns, heavy guns are allowed
                foreach (var ship in shipList)
                    if (_shipCanUseHeavyGuns[ship])
                        canUseHeavyGuns = true;
            // If heavy guns are not allowed, replace any heavy guns with light
            if (!canUseHeavyGuns)
            {
                itemsPerCategory[1] += itemsPerCategory[2];
                itemsPerCategory[2] = 0;
            }

            // Select light guns
            var lgunList = GetLightGuns(itemsPerCategory[1], gamemode, rng);
            // Select heavy guns
            var hgunList = GetHeavyGuns(itemsPerCategory[2], gamemode, rng);

            // Build output string
            output = new List<string>();
            if (itemsPerCategory[0] > 0)
                output.Add(GetListString("Ship", shipList));
            if (itemsPerCategory[1] > 0)
                output.Add(GetListString("Light Gun", lgunList));
            if (itemsPerCategory[2] > 0)
                output.Add(GetListString("Heavy Gun", hgunList));

            return true;
        }

        private string GetListString(string prefix, List<string> items)
        {
            var str = prefix;
            if (items.Count != 1)
                str += "s";
            str += ": " + String.Join(", ", items.ToArray());
            return str;
        }


        private List<string> GetShips(int count, Random rng)
        {
            return GetRandomEntries(_shipNames, count, rng);
        }

        private List<string> GetLightGuns(int count, GameType gamemode, Random rng)
        {
            return GetRandomEntries(_lightGuns[gamemode], count, rng);
        }

        private List<string> GetHeavyGuns(int count, GameType gamemode, Random rng)
        {
            return GetRandomEntries(_heavyGuns[gamemode], count, rng);
        }

        private List<string> GetRandomEntries(List<string> source, int count, Random rng)
        {
            Func<int, int, bool> doPick = delegate (int num, int den)
            {
                return rng.NextDouble() <= (double)num / den;
            };

            var lst = new List<string>();
            var itemsToPick = count;
            var itemsLeft = source.Count;
            while (itemsToPick > 0 && itemsLeft > 0)
            {
                if (doPick(itemsToPick, itemsLeft))
                {
                    lst.Add(source[itemsLeft - 1]);
                    itemsToPick--;
                }
                itemsLeft--;
            }
            if (itemsToPick != 0)
                MuseLog.InfoFormat("GetRandomEntries did not return enough items.");
            return lst;
        }


        public static ChaosItemRandomizer Instance;

        public static void Initialize()
        {
            Instance = new ChaosItemRandomizer();
        }
    }
}
