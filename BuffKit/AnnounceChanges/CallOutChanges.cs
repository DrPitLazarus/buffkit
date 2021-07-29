using System.Collections.Generic;
using System.Linq;
using BepInEx;

namespace BuffKit.AnnounceChanges
{

    public class CallOutChanges
    {

        static BepInEx.Logging.ManualLogSource log;
        public static void CreateLog()
        {
            if (log == null)
            {
                log = BepInEx.Logging.Logger.CreateLogSource("calloutchanges");
            }
        }

        class ShipData
        {
            public int teamIndex;                               // Team number (red = 0, blue = 1, etc)
            public int shipIndex;                               // Index on team (top = 0)
            public int overallIndex;                            // Overall index in match data (allows teams of different size e.g. in practice)
            public bool hasCaptain;
            public string captainName;                          // null if !hasCaptain
            public string shipName;                             // null if !hasCaptain (probably)
            public string shipClass;                            // null if !hasCaptain (probably)
            public List<string> guns = new List<string>();      // Names of guns on ship
            public ShipData(MatchLobbyView mlv, Muse.Goi2.Entity.CrewEntity crewEntity, int indexInTeam, int overallIndex)
            {
                teamIndex = crewEntity.Team;
                shipIndex = indexInTeam;
                this.overallIndex = overallIndex;
                hasCaptain = crewEntity.HasCaptain;
                if (hasCaptain)
                    captainName = crewEntity.Captain.Name;
                shipName = crewEntity.ShipName;
                shipClass = crewEntity.ShipClass;

                var svo = GetShipVOFromCrewId(mlv, crewEntity.CrewId);
                if (crewEntity.HasCaptain)
                {
                    // TODO: Find a solution to get sorted guns that doesn't skip empty gun slots
                    //       Look at ship customisation state entry to see exactly how that does it
                    svo = GetShipVOFromCrewId(mlv, crewEntity.CrewId);
                    var sortedGunSlots = svo.Model.GetSortedSlots(svo.Presets[0].Guns);
                    var sortedguns = (from slot in sortedGunSlots select GunItemInfo.FromGunItem(slot.Gun)).ToList();
                    foreach (var g in sortedguns) guns.Add(g.name);
                }
            }
            public override string ToString()
            {
                string s = "";
                if (!hasCaptain)
                    s += "  [No captain]";
                else
                {
                    s += "  " + shipName + " (" + shipClass + ") :";
                    for (int i = 0; i < guns.Count; i++)
                    {
                        s += "\n   " + (i + 1) + ":" + guns[i];
                    }
                }
                return s;
            }
            public bool HasSameLoadout(ShipData other)
            {
                if (shipClass != other.shipClass) return false;
                if (guns.Count != other.guns.Count) return false;       // May occur if ship is missing guns
                for (int i = 0; i < guns.Count; i++)
                    if (!guns[i].Equals(other.guns[i])) return false;
                return true;
            }
            public bool HasSameCaptain(ShipData other)
            {
                if (captainName == null ^ other.captainName == null) return false;
                if (captainName == null) return true;
                return captainName.Equals(other.captainName);
            }
            public bool HasSameShipname(ShipData other) { return shipName.Equals(other.shipName); }
        }
        class TeamData
        {
            public List<ShipData> ships = new List<ShipData>();
            public override string ToString()
            {
                string s = "";
                for (int i = 0; i < ships.Count; i++)
                    s += "\n Ship " + (i + 1) + ":\n" + ships[i].ToString();
                return s;
            }
        }
        class MatchData
        {
            public List<TeamData> teams = new List<TeamData>();
            public override string ToString()
            {
                string s = "" + teams[0].ships.Count;
                if (teams.Count > 1)
                    for (int i = 1; i < teams.Count; i++)
                        s += "v" + teams[i].ships.Count;
                else
                    s += "v0";
                for (int i = 0; i < teams.Count; i++)
                    s += "\nTeam " + (i + 1) + ":" + teams[i].ToString();
                return s;
            }
            public List<ShipData> GetAllShipData()
            {
                List<ShipData> data = new List<ShipData>();
                foreach (var team in teams)
                    data.AddRange(team.ships);
                return data;
            }
            public Dictionary<string, ShipData> captainNameToShip;
            public void CreateCaptainDictionary()
            {
                captainNameToShip = new Dictionary<string, ShipData>();
                foreach (var s in GetAllShipData())
                    if (s.hasCaptain)
                        captainNameToShip[s.captainName] = s;
            }
        }

        static MatchData matchDataLast;

        public static Muse.Goi2.Entity.Vo.ShipViewObject GetShipVOFromCrewId(MatchLobbyView mlv, string crewId)
        {
            foreach (var csvo in mlv.CrewShips)
                if (csvo.CrewId == crewId) return ShipPreview.GetShipVO(csvo);
            return null;
        }

        public static void LobbyDataChanged(MatchLobbyView mlv)
        {

            if (mlv.Running) return;             // Skip if match running

            MatchData matchDataNew = new MatchData();
            int overallIndex = 0;
            foreach (var team in mlv.Crews)
            {
                TeamData currentTeam = new TeamData();
                int indexInTeam = 0;
                foreach (var currentShipCrew in team)
                {
                    ShipData currentShip = new ShipData(mlv, currentShipCrew, indexInTeam, overallIndex);
                    currentTeam.ships.Add(currentShip);
                    indexInTeam++;
                    overallIndex++;
                }
                matchDataNew.teams.Add(currentTeam);
            }
            matchDataNew.CreateCaptainDictionary();

            log.LogInfo(matchDataNew);

            if (matchDataLast != null)
            {
                // Check for any changes
                var changeList = DetermineChanges(matchDataLast, matchDataNew);
                if (changeList.Count != 0)
                    log.LogInfo("Changes occured");
                string msg = "";
                bool first = true;
                foreach (var c in changeList)
                {
                    if (first)
                        first = false;
                    else
                        msg += ". ";
                    msg += c.GetDetailsShort();
                    log.LogInfo(c.ToString());
                }
                TrySendMessage(msg, "match");
            }

            matchDataLast = matchDataNew;

            // var gunItem = Muse.Goi2.Entity.CachedRepository.Instance.Get<Muse.Goi2.Entity.GunItem>(gunID);
            // var gunItemInfo = GunItemInfo.FromGunItem(gunItem);
        }
        static void TrySendMessage(string message, string channel) { MuseBase.Multiplayer.Unity.MuseWorldClient.Instance.ChatHandler.TrySendMessage(message, channel); }

        /* Helper functions for displaying changes */
        static string TeamToString(int teamIndex)
        {
            switch (teamIndex)
            {
                case 0: return "Red";
                case 1: return "Blue";
                case 2: return "Yellow";
                case 3: return "Purple";
                default: return "[Unknown team " + teamIndex + "]";
            }
        }
        static string TeamShipToString(int teamIndex, int shipIndex) { return TeamToString(teamIndex) + " " + (shipIndex + 1); }
        static string TeamShipToStringShort(int teamIndex, int shipIndex) { return ("" + TeamToString(teamIndex).First()) + (shipIndex + 1); }
        static string GunStringListToString(List<string> gunList)
        {
            string s = "";
            for (int i = 0; i < gunList.Count; i++)
                s += (i > 0 ? " " : "") + (i + 1) + ":" + gunList[i];
            return s;
        }
        static string GunStringShort(string gunName)
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
        static string GunStringListToStringShort(List<string> gunList)
        {
            string s = "";
            for (int i = 0; i < gunList.Count; i++)
                s += (i > 0 ? "/" : "") + GunStringShort(gunList[i]);
            return s;
        }
        static string ClassAndGunsToString(string shipClass, List<string> gunList)
        {
            return shipClass + "(" + GunStringListToString(gunList) + ")";
        }
        static string ClassAndGunsToStringShort(string shipClass, List<string> gunList)
        {
            return shipClass + ":" + GunStringListToStringShort(gunList);
        }
        /* Each change that might have occured */
        interface Change { string GetDetails(); string GetDetailsShort(); }
        class ChangeShipName : Change
        {
            int team; int ship; string prevName; string newName;
            public ChangeShipName(int team, int ship, string prevName, string newName)
            {
                this.team = team;
                this.ship = ship;
                this.prevName = prevName;
                this.newName = newName;
            }
            public string GetDetails() { return TeamShipToString(team, ship) + " changed name from " + prevName + " to " + newName; }
            public string GetDetailsShort() { return TeamShipToStringShort(team, ship) + " changed name"; }
            public override string ToString() { return GetDetails(); }
        }
        class ChangeShipClass : Change
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
            public string GetDetails() { return TeamShipToString(team, ship) + " changed ship from " + prevClass + " to " + ClassAndGunsToString(newClass, newGuns); }
            public string GetDetailsShort()
            {
                return TeamShipToStringShort(team, ship) + " changed ship to " + ClassAndGunsToStringShort(newClass, newGuns);
            }
            public override string ToString() { return GetDetails(); }
        }
        class ChangeGun : Change
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
                return TeamShipToString(team, ship) + " changed " + strFrom + " to " + strTo;
            }
            public string GetDetailsShort()
            {
                return TeamShipToStringShort(team, ship) + " changed guns to " + GunStringListToStringShort(newGuns);
            }
            public override string ToString() { return GetDetails(); }
        }
        class ChangeCaptainMoved : Change
        {
            int teamPrev; int shipPrev; int teamNew; int shipNew;
            public ChangeCaptainMoved(int teamPrev, int shipPrev, int teamNew, int shipNew)
            {
                this.teamPrev = teamPrev;
                this.shipPrev = shipPrev;
                this.teamNew = teamNew;
                this.shipNew = shipNew;
            }
            public string GetDetails() { return TeamShipToString(teamPrev, shipPrev) + " moved to " + TeamShipToString(teamNew, shipNew); }
            public string GetDetailsShort() { return TeamShipToStringShort(teamPrev, shipPrev) + " moved to " + TeamShipToStringShort(teamNew, shipNew); }
            public override string ToString() { return GetDetails(); }
        }
        class ChangeCaptainJoined : Change
        {
            int team; int ship; string shipClass; List<string> guns;
            public ChangeCaptainJoined(int team, int ship, string shipClass, List<string> guns)
            {
                this.team = team;
                this.ship = ship;
                this.shipClass = shipClass;
                this.guns = guns;
            }
            public string GetDetails() { return TeamShipToString(team, ship) + " got a new pilot with a " + ClassAndGunsToString(shipClass, guns); }
            public string GetDetailsShort() { return TeamShipToStringShort(team, ship) + " now has " + ClassAndGunsToStringShort(shipClass, guns); }
            public override string ToString() { return GetDetails(); }
        }
        class ChangeCaptainLeft : Change
        {
            int team; int ship;
            public ChangeCaptainLeft(int team, int ship) { this.team = team; this.ship = ship; }
            public string GetDetails() { return TeamShipToString(team, ship) + " lost a captain"; }
            public string GetDetailsShort() { return TeamShipToStringShort(team, ship) + " lost a captain"; }
            public override string ToString() { return GetDetails(); }
        }
        /* Get a list of all changes (hopefully all the important ones are covered) */
        static List<Change> DetermineChanges(MatchData before, MatchData after)
        {
            List<Change> changes = new List<Change>();

            // Create map of old ships to new ships
            var allOldShips = before.GetAllShipData();
            var allNewShips = after.GetAllShipData();
            int[] newShipToOldShip = new int[allOldShips.Count];
            for (int i = 0; i < newShipToOldShip.Length; i++)
            {
                if (allNewShips[i].hasCaptain)
                {
                    if (allNewShips[i].HasSameCaptain(allOldShips[i]))                              // Captain was in same slot previously          no change
                        newShipToOldShip[i] = i;
                    else if (!before.captainNameToShip.ContainsKey(allNewShips[i].captainName))     // Captain was not a captain previously         join
                        newShipToOldShip[i] = -1;
                    else                                                                            // Captain was in different slot previously     move
                        newShipToOldShip[i] = before.captainNameToShip[allNewShips[i].captainName].overallIndex;
                }
                else
                {
                    if (allOldShips[i].hasCaptain)
                    {
                        if (after.captainNameToShip.ContainsKey(allOldShips[i].captainName))        // Captain moved to another captain slot        move
                            newShipToOldShip[i] = -2;
                        else                                                                        // Captain is no longer a captain               left
                            newShipToOldShip[i] = -3;
                    }
                    else                                                                            // No captain before or after                   no change
                        newShipToOldShip[i] = -4;
                }
            }
            // Handle changes
            for (int i = 0; i < newShipToOldShip.Length; i++)
            {
                ShipData newShip = allNewShips[i];
                ShipData oldShip = null;
                // Movement changes (and link new ship to old, if possible)
                switch (newShipToOldShip[i])
                {
                    case -1:
                        changes.Add(new ChangeCaptainJoined(newShip.teamIndex, newShip.shipIndex, newShip.shipClass, newShip.guns));
                        break;
                    case -2:             // Handled by -1
                        break;
                    case -3:
                        changes.Add(new ChangeCaptainLeft(newShip.teamIndex, newShip.shipIndex));
                        break;
                    case -4:
                        break;
                    default:
                        oldShip = allOldShips[newShipToOldShip[i]];
                        if (newShipToOldShip[i] != i)
                            changes.Add(new ChangeCaptainMoved(oldShip.teamIndex, oldShip.shipIndex, newShip.teamIndex, newShip.shipIndex));
                        break;
                }
                // Check if other changes are required
                if (oldShip != null && newShip != null)
                {
                    if (!oldShip.shipClass.Equals(newShip.shipClass))       // Class change
                        changes.Add(new ChangeShipClass(newShip.teamIndex, newShip.shipIndex, oldShip.shipClass, newShip.shipClass, newShip.guns));
                    else
                    {
                        if (!oldShip.HasSameLoadout(newShip))               // Gun change
                            changes.Add(new ChangeGun(newShip.teamIndex, newShip.shipIndex, oldShip.guns, newShip.guns));
                        if (!oldShip.shipName.Equals(newShip.shipName))      // Name change
                            changes.Add(new ChangeShipName(newShip.teamIndex, newShip.shipIndex, oldShip.shipName, newShip.shipName));
                    }
                }
            }

            return changes;
        }
    }
}
