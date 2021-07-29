using System.Collections.Generic;
using System.Linq;

namespace BuffKit.AnnounceChanges
{
    static class AnnounceChangesUtil
    {
        static string TeamToString(int teamIndex)
        {
            switch (teamIndex)
            {
                case 0: return "Red";
                case 1: return "Blue";
                case 2: return "Yellow";
                case 3: return "Purple";
                default: return $"[Unknown team {teamIndex}]";
            }
        }
        public static string TeamShipToString(int teamIndex, int shipIndex)
        {
            return $"{TeamToString(teamIndex)} {shipIndex + 1}";
        }
        public static string TeamShipToStringShort(int teamIndex, int shipIndex)
        {
            return $"{TeamToString(teamIndex).First()}{shipIndex + 1}";
        }
        public static string GunStringListToString(List<string> gunList)
        {
            string s = "";
            for (int i = 0; i < gunList.Count; i++)
                s += (i > 0 ? " " : "") + (i + 1) + ":" + gunList[i];
            return s;
        }
        public static string GunStringShort(string gunName)
        {
            switch (gunName)
            {
                /* Light Guns */
                case "Artemis Light Rocket Launcher":
                    return "Artemis";
                case "Echidna Light Flak Cannon":
                    return "Flak";
                case "Whirlwind Light Gatling Gun":
                    return "Gatling";
                case "Dragon Tongue Light Flamethrower":
                    return "Flamer";
                case "Barking Dog Light Carronade":
                    return "Carronade";
                case "Javelin Light Harpoon Gun":
                    return "Harpoon";
                case "Beacon Flare Gun":
                    return "Flare";
                case "Mercury Field Gun":
                    return "Mercury";
                case "Scylla Double-Barreled Mortar":
                    return "Mortar";
                case "Banshee Light Rocket Carousel":
                    return "Banshee";
                case "Phobos Light Mine Launcher":
                    return "Mine";
                case "Hades Light Cannon":
                    return "Hades";
                case "Seraph Tempest Missiles [Mk. S]":
                    return "Tempest";
                case "Aten Lens Array [Mk. S]":
                    return "Laser";
                /* Heavy Guns */
                case "Typhon Heavy Flak Cannon Mk. I":
                    return "Flak1";
                case "Manticore Heavy Hwacha":
                    return "Hwacha";
                case "Hellhound Heavy Twin Carronade":
                    return "Hellhound";
                case "Lumberjack Heavy Mortar":
                    return "Lumberjack";
                case "Minotaur Heavy Cannon":
                    return "Minotaur";
                case "Typhon Heavy Flak Cannon [Mk. II]":
                    return "Flak2";
                case "Nemesis Heavy Carronade":
                    return "Nemesis";
                case "Roaring Tiger Heavy Detonator [Mk. S]":
                    return "Detonator";
                default:
                    return gunName;
            }
        }
        public static string GunStringListToStringShort(List<string> gunList)
        {
            string s = "";
            for (int i = 0; i < gunList.Count; i++)
                s += (i > 0 ? "/" : "") + GunStringShort(gunList[i]);
            return s;
        }
        public static string ClassAndGunsToString(string shipClass, List<string> gunList)
        {
            return $"{shipClass}({GunStringListToString(gunList)})";
        }
        public static string ClassAndGunsToStringShort(string shipClass, List<string> gunList)
        {
            return $"{shipClass}:{GunStringListToStringShort(gunList)}";
        }
    }

    /* Each change that might have occured */
    interface Change { string GetDetails(); string GetDetailsShort(); }
    public class ChangeShipName : Change
    {
        int team; int ship; string prevName; string newName;
        public ChangeShipName(int team, int ship, string prevName, string newName)
        {
            this.team = team;
            this.ship = ship;
            this.prevName = prevName;
            this.newName = newName;
        }
        public string GetDetails()
        {
            return $"{AnnounceChangesUtil.TeamShipToString(team, ship)} changed name from {prevName} to {newName}";
        }
        public string GetDetailsShort()
        {
            return $"{AnnounceChangesUtil.TeamShipToStringShort(team, ship) } changed name";
        }
        public override string ToString()
        {
            return GetDetails();
        }
    }
    public class ChangeShipClass : Change
    {
        int team; int ship; string prevClass; string newClass; List<string> newGuns;
        public ChangeShipClass(int team, int ship, string prevClass, string newClass, List<string> newGuns)
        {
            this.team = team;
            this.ship = ship;
            this.prevClass = prevClass;
            this.newClass = newClass;
            this.newGuns = newGuns;
        }
        public string GetDetails()
        {
            return $"{AnnounceChangesUtil.TeamShipToString(team, ship)} changed ship from {prevClass} to {AnnounceChangesUtil.ClassAndGunsToString(newClass, newGuns)}";
        }
        public string GetDetailsShort()
        {
            return $"{AnnounceChangesUtil.TeamShipToStringShort(team, ship)} changed ship to {AnnounceChangesUtil.ClassAndGunsToStringShort(newClass, newGuns)}";
        }
        public override string ToString()
        {
            return GetDetails();
        }
    }
    public class ChangeGun : Change
    {
        int team; int ship; List<string> prevGuns; List<string> newGuns;
        public ChangeGun(int team, int ship, List<string> prevGuns, List<string> newGuns)
        {
            this.team = team;
            this.ship = ship;
            this.prevGuns = prevGuns;
            this.newGuns = newGuns;
        }
        public string GetDetails()
        {
            List<int> dirtyIndices = new List<int>();
            for (int i = 0; i < prevGuns.Count; i++)
                if (!prevGuns[i].Equals(newGuns[i])) dirtyIndices.Add(i);
            string strFrom = "guns ";
            string strTo = "";
            for (int i = 0; i < dirtyIndices.Count; i++)
            {
                strFrom += (i > 0 ? ", " : "") + (dirtyIndices[i] + 1);
                strTo += (i > 0 ? ", " : "") + newGuns[dirtyIndices[i]];
            }
            return $"{AnnounceChangesUtil.TeamShipToString(team, ship)} changed {strFrom} to {strTo}";
        }
        public string GetDetailsShort()
        {
            return $"{AnnounceChangesUtil.TeamShipToStringShort(team, ship)} changed guns to {AnnounceChangesUtil.GunStringListToStringShort(newGuns)}";
        }
        public override string ToString()
        {
            return GetDetails();
        }
    }
    public class ChangeCaptainMoved : Change
    {
        int teamPrev; int shipPrev; int teamNew; int shipNew;
        public ChangeCaptainMoved(int teamPrev, int shipPrev, int teamNew, int shipNew)
        {
            this.teamPrev = teamPrev;
            this.shipPrev = shipPrev;
            this.teamNew = teamNew;
            this.shipNew = shipNew;
        }
        public string GetDetails()
        {
            return $"{AnnounceChangesUtil.TeamShipToString(teamPrev, shipPrev)} moved to {AnnounceChangesUtil.TeamShipToString(teamNew, shipNew)}";
        }
        public string GetDetailsShort()
        {
            return $"{AnnounceChangesUtil.TeamShipToStringShort(teamPrev, shipPrev)} moved to {AnnounceChangesUtil.TeamShipToStringShort(teamNew, shipNew)}";
        }
        public override string ToString()
        {
            return GetDetails();
        }
    }
    public class ChangeCaptainJoined : Change
    {
        int team; int ship; string shipClass; List<string> guns;
        public ChangeCaptainJoined(int team, int ship, string shipClass, List<string> guns)
        {
            this.team = team;
            this.ship = ship;
            this.shipClass = shipClass;
            this.guns = guns;
        }
        public string GetDetails()
        {
            return $"{AnnounceChangesUtil.TeamShipToString(team, ship)} got a new pilot with a {AnnounceChangesUtil.ClassAndGunsToString(shipClass, guns)}";
        }
        public string GetDetailsShort()
        {
            return $"{AnnounceChangesUtil.TeamShipToStringShort(team, ship)} now has {AnnounceChangesUtil.ClassAndGunsToStringShort(shipClass, guns)}";
        }
        public override string ToString()
        {
            return GetDetails();
        }
    }
    public class ChangeCaptainLeft : Change
    {
        int team; int ship;
        public ChangeCaptainLeft(int team, int ship)
        {
            this.team = team; this.ship = ship;
        }
        public string GetDetails()
        {
            return $"{AnnounceChangesUtil.TeamShipToString(team, ship)} lost a captain";
        }
        public string GetDetailsShort()
        {
            return $"{AnnounceChangesUtil.TeamShipToStringShort(team, ship)} lost a captain";
        }
        public override string ToString()
        {
            return GetDetails();
        }
    }
}
