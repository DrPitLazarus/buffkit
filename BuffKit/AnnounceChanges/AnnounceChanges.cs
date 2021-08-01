using System.Collections.Generic;
using System.Linq;
using System.Text;
using Muse.Goi2.Entity;
using Muse.Goi2.Entity.Vo;
using static BuffKit.Util.Util;

namespace BuffKit.AnnounceChanges
{

    public class AnnounceChanges
    {
        static BepInEx.Logging.ManualLogSource log;
        public static void CreateLog()
        {
            if (log == null)
            {
                log = BepInEx.Logging.Logger.CreateLogSource("AnnounceChanges");
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
            
            public ShipData(MatchLobbyView mlv, CrewEntity crewEntity, int indexInTeam, int overallIndex)
            {
                teamIndex = crewEntity.Team;
                shipIndex = indexInTeam;
                this.overallIndex = overallIndex;
                hasCaptain = crewEntity.HasCaptain;
                if (hasCaptain)
                    captainName = crewEntity.Captain.Name;
                shipName = crewEntity.ShipName;
                shipClass = crewEntity.ShipClass;

                if (crewEntity.HasCaptain)
                {
                    var svo = GetShipVOFromCrewId(mlv, crewEntity.CrewId);
                    var sortedGunIds = GetSortedGunIds(svo.Model, svo.Presets[0].Guns);
                    foreach (var g in sortedGunIds)
                        if (g != -1)
                            guns.Add(CachedRepository.Instance.Get<GunItem>(g).NameText.En);
                        else
                            guns.Add("Empty");
                }
            }
            public override string ToString()
            {
                var s = new StringBuilder();
                if (!hasCaptain)
                    s.Append("  [No captain]");
                else
                {
                    s.Append($"  {shipName} ({shipClass}) :");
                    for (var i = 0; i < guns.Count; i++)
                    {
                        s.Append($"\n   {i+1}: {guns[i]}");
                    }
                }
                return s.ToString();
            }
            public bool HasSameLoadout(ShipData other)
            {
                if (shipClass != other.shipClass) return false;
                if (guns.Count != other.guns.Count) return false;       // May occur if ship is missing guns
                for (var i = 0; i < guns.Count; i++)
                    if (!guns[i].Equals(other.guns[i])) return false;
                return true;
            }
            public bool HasSameCaptain(ShipData other)
            {
                if (captainName == null ^ other.captainName == null) return false;
                if (captainName == null) return true;
                return captainName.Equals(other.captainName);
            }
            public bool HasSameShipName(ShipData other) { return shipName.Equals(other.shipName); }
        }
        class MatchData
        {
            public List<List<ShipData>> ships;                          // Access by ships[teamIndex][shipIndex]
            public List<ShipData> flatShips;                            // Access by flatShips[overallIndex]
            public Dictionary<string, ShipData> captainNameToShip;      // Access to ship data by captain playername
            public MatchData(MatchLobbyView mlv)
            {
                ships = new List<List<ShipData>>();
                flatShips = new List<ShipData>();
                var overallIndex = 0;
                foreach(var team in mlv.Crews)
                {
                    List<ShipData> currentTeam = new List<ShipData>();
                    var indexInTeam = 0;
                    foreach(var crew in team)
                    {
                        ShipData currentShip = new ShipData(mlv, crew, indexInTeam, overallIndex);
                        currentTeam.Add(currentShip);
                        flatShips.Add(currentShip);
                        indexInTeam++;
                        overallIndex++;
                    }
                    ships.Add(currentTeam);
                    captainNameToShip = new Dictionary<string, ShipData>();
                    foreach (var s in flatShips)
                        if (s.hasCaptain)
                            captainNameToShip[s.captainName] = s;
                }
            }
        }

        private static MatchData _matchDataLast;

        public static ShipViewObject GetShipVOFromCrewId(MatchLobbyView mlv, string crewId)
        {
            foreach (var csvo in mlv.CrewShips)
                if (csvo.CrewId == crewId) return ShipPreview.GetShipVO(csvo);
            return null;
        }

        public static void LobbyDataChanged(MatchLobbyView mlv)
        {
            if (mlv.Running) return;             // Skip if match running

            MatchData matchDataNew = new MatchData(mlv);
            var timer = mlv.GetComponent<LobbyTimer.Timer>();
            
            // log.LogInfo(matchDataNew);

            if (_matchDataLast != null)
            {
                // Check for any changes
                var changeList = DetermineChanges(_matchDataLast, matchDataNew);
                if (changeList.Count != 0)
                    log.LogInfo("Changes occured");
                string msg = "";
                bool first = true;
                foreach (var c in changeList)
                {
                    if (c is ChangeCaptainJoined
                    || c is ChangeCaptainLeft
                    || c is ChangeCaptainMoved
                    || c is ChangeShipName) continue;

                    if (first)
                        first = false;
                    else
                        msg += ". ";
                    msg += c.GetDetailsShort();
                    // log.LogInfo(c.ToString());
                }
                
                if (!(timer is null) && timer.IsActive)
                {
                    ForceSendMessage(msg);
                }
            }

            _matchDataLast = matchDataNew;
        }

        static List<Change> DetermineChanges(MatchData before, MatchData after)
        {
            List<Change> changes = new List<Change>();

            // Create map of old ships to new ships
            var allOldShips = before.flatShips;
            var allNewShips = after.flatShips;
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
                        if (!oldShip.shipName.Equals(newShip.shipName))     // Name change
                            changes.Add(new ChangeShipName(newShip.teamIndex, newShip.shipIndex, oldShip.shipName, newShip.shipName));
                    }
                }
            }

            return changes;
        }
    }
}
