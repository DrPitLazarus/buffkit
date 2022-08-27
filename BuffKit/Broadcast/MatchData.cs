using System;
using System.Collections.Generic;
using Muse.Goi2.Entity;
using Muse.Icarus.Common;
using Newtonsoft.Json;

namespace BuffKit.Broadcast
{
    public static class Helper
    {
        public static T NullIfEqual<T>(T originalValue, T newValue)
            where T : class
        {
            if (originalValue == null) return newValue;
            if (originalValue.Equals(newValue)) return null;
            else return newValue;
        }
        public static Nullable<T> ValueNullIfEqual<T>(Nullable<T> originalValue, Nullable<T> newValue)
            where T : struct
        {
            if (originalValue.Equals(newValue)) return null;
            else return (T?)newValue;
        }
        public static T[] ValueListNullIfEqual<T>(T[] originalValue, T[] newValue)
            where T : struct
        {
            if (originalValue == null) return newValue;
            if (originalValue.Length != newValue.Length) return newValue;
            for(var i = 0; i < originalValue.Length; i++)
            {
                if (!originalValue[i].Equals(newValue[i])) return newValue;
            }
            return null;
        }
        public static T[] NullIfEmpty<T>(T[] list)
        {
            if (list == null) return null;
            if (list.Length == 0) return null;
            foreach (var v in list)
                if (v != null) return list;
            return null;
        }
    }

    [JsonObject(MemberSerialization.Fields)]
    public class MatchData
    {
        string lobbyID;
        string lobbyName;
        int? mapID;
        int[] scores;
        float? matchTimer;
        ShipData[] ships;
        string[] killfeed;              // All new additions to the killfeed

        // Create snapshot from current data
        public MatchData(List<Airship> ships, List<string> killfeed)
        {
            this.lobbyID = MatchLobbyView.Instance.MatchId;
            this.lobbyName = MatchLobbyView.Instance.MatchName;
            this.mapID = MatchLobbyView.Instance.MapId;
            // Region r = CachedRepository.Instance.Get<Region>(this.mapID);
            // string mapName = r.NameText.En;

            int teamCount = MatchLobbyView.Instance.TeamCount;
            this.scores = new int[teamCount];
            for (var i = 0; i < teamCount; i++)
                this.scores[i] = Mission.Instance.TeamScore(i);

            MatchStateView instance = MatchStateView.Instance;
            this.matchTimer = (instance != null) ? instance.ModCountdown : 0;

            this.ships = new ShipData[ships.Count];
            for (var i = 0; i < ships.Count; i++)
                this.ships[i] = new ShipData(ships[i]);

            this.killfeed = killfeed.ToArray();
        }

        // Create diff from current data and snapshot
        public static MatchData MatchDataDiff(MatchData snapshot, MatchData current)
        {
            MatchData diff = new MatchData();

            diff.lobbyID = Helper.NullIfEqual(snapshot.lobbyID, current.lobbyID);
            diff.lobbyName = Helper.NullIfEqual(snapshot.lobbyName, current.lobbyName);
            diff.mapID = Helper.ValueNullIfEqual(snapshot.mapID, current.mapID);
            diff.scores = Helper.ValueListNullIfEqual(snapshot.scores, current.scores);
            diff.matchTimer = Helper.ValueNullIfEqual(snapshot.matchTimer, current.matchTimer);
            if (snapshot.ships.Length == current.ships.Length)
            {
                diff.ships = new ShipData[snapshot.ships.Length];
                for (var i = 0; i < snapshot.ships.Length; i++)
                    diff.ships[i] = ShipData.ShipDataDiff(snapshot.ships[i], current.ships[i]);
            }
            else
                diff.ships = current.ships;
            diff.ships = Helper.NullIfEmpty(diff.ships);
            diff.killfeed = Helper.NullIfEmpty(current.killfeed);

            return diff;
        }
        private MatchData() { }

        public void Merge(MatchData other)
        {
            if (other.lobbyID != null) this.lobbyID = other.lobbyID;
            if (other.lobbyName != null) this.lobbyName = other.lobbyName;
            if (other.mapID != null) this.mapID = other.mapID;
            if (other.scores != null) this.scores = other.scores;
            if (other.matchTimer != null) this.matchTimer = other.matchTimer;
            if (other.ships != null)
                for (var i = 0; i < other.ships.Length; i++)
                    if (other.ships[i] != null)
                        this.ships[i].Merge(other.ships[i]);
            if (other.killfeed != null) this.killfeed = other.killfeed;
        }

        public string DebugEquals(MatchData other)
        {
            List<string> different = new List<string>();

            if (!other.lobbyID.Equals(this.lobbyID)) different.Add("lobbyID");
            if (!other.lobbyName.Equals(this.lobbyName)) different.Add("lobbyName");
            if (!other.mapID.Equals(this.mapID)) different.Add("mapID");
            for (var i = 0; i < scores.Length; i++)
                if (other.scores[i] != this.scores[i])
                    different.Add(String.Format("score {0}", i));
            if (!other.matchTimer.Equals(this.matchTimer)) different.Add("matchTimer");
            for (var i = 0; i < ships.Length; i++)
            {
                if (other.ships.Length != this.ships.Length)
                    different.Add("ships count");
                else { 
                    var shipTest = this.ships[i].DebugEquals(other.ships[i]);
                    if (shipTest != null)
                        different.Add(String.Format("ship {0}: [{1}]", i, shipTest));
                }
            }
            //if (!other.killfeed.Equals(this.killfeed)) different.Add("killfeed");

            if (different.Count == 0) return null;
            return String.Join("\n", different.ToArray());
        }

    }

    [JsonObject(MemberSerialization.Fields)]
    public class ShipData
    {
        string name;
        string shipClass;
        int? team;
        float[] position;
        float[] velocity;
        float? yaw;
        float? angularVelocity;
        bool? spotted;
        float? hull;
        float? maxHull;
        ComponentData armor;
        ComponentData balloon;
        ComponentData[] engines;
        GunComponentData[] guns;
        PlayerData[] crew;
        public string DebugEquals(ShipData other)
        {
            List<string> different = new List<string>();

            if (!other.name.Equals(this.name)) different.Add("name");
            if (!other.shipClass.Equals(this.shipClass)) different.Add("shipClass");
            if (!other.team.Equals(this.team)) different.Add("team");
            var p1 = this.position;
            var p2 = other.position;
            if (p1[0] != p2[0] || p1[1] != p2[1] || p1[2] != p2[2])
                different.Add(String.Format("position ({0},{1},{2} to {3},{4},{5})",
                new object[]{
                    p1[0], p1[1], p1[2], p2[0], p2[1], p2[2]
                }));
            var v1 = this.velocity;
            var v2 = other.velocity;
            if (v1[0] != v2[0] || v1[1] != v2[1] || v1[2] != v2[2])
                different.Add(String.Format("velocity ({0},{1},{2} to {3},{4},{5})",
                new object[]{
                    v1[0], v1[1], v1[2], v2[0], v2[1], v2[2]
                }));
            if (!other.yaw.Equals(this.yaw)) different.Add("yaw");
            if (!other.angularVelocity.Equals(this.angularVelocity)) different.Add("angularVelocity");
            if (!other.spotted.Equals(this.spotted)) different.Add("spotted");
            if (!other.hull.Equals(this.hull)) different.Add("hull");
            if (!other.maxHull.Equals(this.maxHull)) different.Add("maxHull");

            var armorTest = armor.DebugEquals(other.armor);
            if (armorTest != null) different.Add(String.Format("armor: {0}", armorTest));
            var balloonTest = balloon.DebugEquals(other.balloon);
            if (balloonTest != null) different.Add(String.Format("balloon: {0}", balloonTest));

            if (other.engines.Length != this.engines.Length)
                different.Add("engine count");
            else
                for (var i = 0; i < engines.Length; i++)
                {
                    var engineTest = this.engines[i].DebugEquals(other.engines[i]);
                    if (engineTest != null) different.Add(String.Format("engine {0}: {1}", i, engineTest));
                }
            if (other.guns.Length != this.guns.Length)
                different.Add("gun count");
            else
                for (var i = 0; i < guns.Length; i++)
                {
                    var gunsTest = this.guns[i].DebugEquals(other.guns[i]);
                    if (gunsTest != null) different.Add(String.Format("gun {0}: {1}", i, gunsTest));
                }
            if (other.crew.Length != this.crew.Length)
                different.Add("crew count");
            else
                for (var i = 0; i < crew.Length; i++)
                {
                    var crewTest = this.crew[i].DebugEquals(other.crew[i]);
                    if (crewTest != null) different.Add(String.Format("crew {0}: {1}", i, crewTest));
                }

            if (different.Count == 0) return null;
            return String.Join("\n", different.ToArray());
        }

        // Create snapshot from current data
        public ShipData(Airship ship)
        {
            this.name = ship.name;
            this.shipClass = ship.ShipClass;
            this.team = ship.Side;
            this.position = new float[] { ship.Position.x, ship.Position.y, ship.Position.z };
            this.velocity = new float[] { ship.LocalVelocity.x, ship.LocalVelocity.y, ship.LocalVelocity.z };
            this.yaw = ship.Yaw;
            this.angularVelocity = ship.AngularVelocity;
            this.spotted = ship.SpotStrength > 0;       // 0.95 - in full view, 0 < x < 0.95, losing spot

            List<Engine> engines = new List<Engine>();
            List<Turret> guns = new List<Turret>();

            foreach (Repairable c in ship.Repairables)
            {
                switch (c.Type)
                {
                    case LocalShipPartType.Hull:
                        var h = (Hull)c;
                        this.hull = h.CoreHealth;
                        this.maxHull = h.MaxCoreHealth;
                        this.armor = new ComponentData(h);
                        break;
                    case LocalShipPartType.Zeppelin:
                        var b = (Balloon)c;
                        this.balloon = new ComponentData(b);
                        break;
                    case LocalShipPartType.Engine:
                        var e = (Engine)c;
                        engines.Add(e);
                        break;
                    case LocalShipPartType.Gun:
                        var g = (Turret)c;
                        guns.Add(g);
                        break;
                    default:
                        break;
                }
            }
            // Sort guns
            var shipId = ship.ShipModelId;
            guns.Sort(
                (x, y) => Util.GetGunSlotIndex(shipId, x.SlotName).CompareTo(Util.GetGunSlotIndex(shipId, y.SlotName))
            );

            this.engines = new ComponentData[engines.Count];
            this.guns = new GunComponentData[guns.Count];
            for (var i = 0; i < engines.Count; i++)
                this.engines[i] = new ComponentData(engines[i]);
            for (var i = 0; i < guns.Count; i++)
                this.guns[i] = new GunComponentData(guns[i]);

            this.crew = new PlayerData[ship.Players.Count];
            var j = 0;
            foreach (var player in ship.Players)
            {
                this.crew[j] = new PlayerData(player);
                j++;
            }
        }

        // Create diff from current data and snapshot
        public static ShipData ShipDataDiff(ShipData snapshot, ShipData current)
        {
            ShipData diff = new ShipData();

            diff.name = Helper.NullIfEqual(snapshot.name, current.name);
            diff.shipClass = Helper.NullIfEqual(snapshot.shipClass, current.shipClass);
            diff.team = Helper.ValueNullIfEqual(snapshot.team, current.team);
            diff.position = Helper.ValueListNullIfEqual(snapshot.position, current.position);
            diff.velocity = Helper.ValueListNullIfEqual(snapshot.velocity, current.velocity);
            diff.yaw = Helper.ValueNullIfEqual(snapshot.yaw, current.yaw);
            diff.angularVelocity = Helper.ValueNullIfEqual(snapshot.angularVelocity, current.angularVelocity);
            diff.spotted = Helper.ValueNullIfEqual(snapshot.spotted, current.spotted);
            diff.hull = Helper.ValueNullIfEqual(snapshot.hull, current.hull);
            diff.maxHull = Helper.ValueNullIfEqual(snapshot.maxHull, current.maxHull);
            diff.armor = ComponentData.ComponentDataDiff(snapshot.armor, current.armor);
            diff.balloon = ComponentData.ComponentDataDiff(snapshot.balloon, current.balloon);
            if (snapshot.engines.Length == current.engines.Length)
            {
                diff.engines = new ComponentData[snapshot.engines.Length];
                for (var i = 0; i < snapshot.engines.Length; i++)
                    diff.engines[i] = ComponentData.ComponentDataDiff(snapshot.engines[i], current.engines[i]);
            }
            else
                diff.engines = current.engines;
            diff.engines = Helper.NullIfEmpty(diff.engines);
            if (snapshot.guns.Length == current.guns.Length)
            {
                diff.guns = new GunComponentData[snapshot.guns.Length];
                for (var i = 0; i < snapshot.guns.Length; i++)
                    diff.guns[i] = GunComponentData.GunComponentDataDiff(snapshot.guns[i], current.guns[i]);
            }
            else
                diff.guns = current.guns;
            diff.guns = Helper.NullIfEmpty(diff.guns);
            if (snapshot.crew.Length == current.crew.Length)
            {
                diff.crew = new PlayerData[snapshot.crew.Length];
                for (var i = 0; i < snapshot.crew.Length; i++)
                    diff.crew[i] = PlayerData.PlayerDataDiff(snapshot.crew[i], current.crew[i]);
            }
            else
                diff.crew = current.crew;
            diff.crew = Helper.NullIfEmpty(diff.crew);

            if (diff.name == null &&
                diff.shipClass == null &&
                diff.team == null &&
                diff.position == null &&
                diff.velocity == null &&
                diff.yaw == null &&
                diff.angularVelocity == null &&
                diff.spotted == null &&
                diff.hull == null &&
                diff.maxHull == null &&
                diff.armor == null &&
                diff.balloon == null &&
                diff.engines == null &&
                diff.guns == null &&
                diff.crew == null)
                return null;

            return diff;
        }
        private ShipData() { }

        public void Merge(ShipData other)
        {
            if (other.name != null) this.name = other.name;
            if (other.shipClass != null) this.shipClass = other.shipClass;
            if (other.team != null) this.team = other.team;
            if (other.position != null) this.position = other.position;
            if (other.velocity != null) this.velocity = other.velocity;
            if (other.yaw != null) this.yaw = other.yaw;
            if (other.angularVelocity != null) this.angularVelocity = other.angularVelocity;
            if (other.spotted != null) this.spotted = other.spotted;
            if (other.hull != null) this.hull = other.hull;
            if (other.maxHull != null) this.maxHull = other.maxHull;
            if (other.armor != null) this.armor.Merge(other.armor);
            if (other.balloon != null) this.balloon.Merge(other.balloon);
            if (other.engines != null)
            {
                if (other.engines.Length == this.engines.Length)
                {
                    for (var i = 0; i < other.engines.Length; i++)
                        if (other.engines[i] != null)
                            this.engines[i].Merge(other.engines[i]);
                }
                else this.engines = other.engines;
            }
            if (other.guns != null)
            {
                if (other.guns.Length == this.guns.Length)
                {
                    for (var i = 0; i < other.guns.Length; i++)
                        if (other.guns[i] != null)
                            this.guns[i].Merge(other.guns[i]);
                }
                else this.guns = other.guns;
            }
            if (other.crew != null)
            {
                if (other.crew.Length == this.crew.Length)
                {
                    for (var i = 0; i < other.crew.Length; i++)
                        if (other.crew[i] != null)
                            this.crew[i].Merge(other.crew[i]);
                }
                else
                    this.crew = other.crew;
            }
        }
    }

    [JsonObject(MemberSerialization.Fields)]
    public class ComponentData
    {
        float? health;
        float? maxHealth;
        float? repairCooldownProgress;   // 0 (started) -> 1 (no cooldown)
        float? rebuildProgress;          // 0 (dead) -> 1
        int? fireStacks;                 // 0 -> 20
        float? buffProgress;             // 0 (nothing) -> 1 (finished buffing)
        float? buffRemaining;            // 1 (buffed) -> 0
        bool? fireproof;
        bool? failsafed;

        // Create snapshot from current data
        public ComponentData(Repairable component)
        {
            this.health = component.Health;
            this.maxHealth = component.MaxHealth;
            if (component.NoHealth)
            {
                // Component is dead
                this.repairCooldownProgress = 0;
                this.rebuildProgress = component.RepairProgress;
                this.fireStacks = 0;
                this.buffProgress = 0;
                this.buffRemaining = 0;
            }
            else
            {
                // Component is alive, possibly damaged
                this.rebuildProgress = 0;
                this.repairCooldownProgress = component.RepairProgress;
                this.fireStacks = component.FireCharges;
                this.buffProgress = component.BuffProgress;
                this.buffRemaining = component.BuffDuration;
            }
            this.fireproof = component.Status.GetStrength(SkillEffectType.ModifyIgnitionChance) == -1;
            this.failsafed = component.Status.ActiveStatusEffects.Contains(SkillEffectType.HealPart);
        }

        // Create diff from current data and snapshot
        public static ComponentData ComponentDataDiff(ComponentData snapshot, ComponentData current)
        {
            if (snapshot == null) MuseLog.Info("ComponentData snapshot is null!");
            if (current == null) MuseLog.Info("ComponentData current is null!");

            if (current == null || snapshot==null) return null;

            ComponentData diff = new ComponentData();

            diff.health = Helper.ValueNullIfEqual(snapshot.health, current.health);
            diff.maxHealth = Helper.ValueNullIfEqual(snapshot.maxHealth, current.maxHealth);
            diff.repairCooldownProgress = Helper.ValueNullIfEqual(snapshot.repairCooldownProgress, current.repairCooldownProgress);
            diff.rebuildProgress = Helper.ValueNullIfEqual(snapshot.rebuildProgress, current.rebuildProgress);
            diff.fireStacks = Helper.ValueNullIfEqual(snapshot.fireStacks, current.fireStacks);
            diff.buffProgress = Helper.ValueNullIfEqual(snapshot.buffProgress, current.buffProgress);
            diff.buffRemaining = Helper.ValueNullIfEqual(snapshot.buffRemaining, current.buffRemaining);
            diff.fireproof = Helper.ValueNullIfEqual(snapshot.fireproof, current.fireproof);
            diff.failsafed = Helper.ValueNullIfEqual(snapshot.failsafed, current.failsafed);

            if (diff.health == null &&
                diff.maxHealth == null &&
                diff.repairCooldownProgress == null &&
                diff.rebuildProgress == null &&
                diff.fireStacks == null &&
                diff.buffProgress == null &&
                diff.buffRemaining == null &&
                diff.fireproof == null &&
                diff.failsafed == null)
                return null;

            return diff;
        }
        private ComponentData() { }

        public void Merge(ComponentData other)
        {
            if (other.health != null) this.health = other.health;
            if (other.maxHealth != null) this.maxHealth = other.maxHealth;
            if (other.repairCooldownProgress != null) this.repairCooldownProgress = other.repairCooldownProgress;
            if (other.rebuildProgress != null) this.rebuildProgress = other.rebuildProgress;
            if (other.fireStacks != null) this.fireStacks = other.fireStacks;
            if (other.buffProgress != null) this.buffProgress = other.buffProgress;
            if (other.buffRemaining != null) this.buffRemaining = other.buffRemaining;
            if (other.fireproof != null) this.fireproof = other.fireproof;
            if (other.failsafed != null) this.failsafed = other.failsafed;
        }
        public string DebugEquals(ComponentData other)
        {
            if (other == null) return "component null";

            List<string> different = new List<string>();

            if (!other.health.Equals(this.health)) different.Add(String.Format("health ({0} to {1})", this.health, other.health));
            if (!other.maxHealth.Equals(this.maxHealth)) different.Add(String.Format("maxHealth ({0} to {1})", this.maxHealth, other.maxHealth));
            if (!other.repairCooldownProgress.Equals(this.repairCooldownProgress)) different.Add("repairCooldownProgress");
            if (!other.rebuildProgress.Equals(this.rebuildProgress)) different.Add("rebuildProgress");
            if (!other.fireStacks.Equals(this.fireStacks)) different.Add("fireStacks");
            if (!other.buffProgress.Equals(this.buffProgress)) different.Add("buffProgress");
            if (!other.buffRemaining.Equals(this.buffRemaining)) different.Add("buffRemaining");
            if (!other.fireproof.Equals(this.fireproof)) different.Add("fireproof");
            if (!other.failsafed.Equals(this.failsafed)) different.Add("failsafed");

            if (different.Count == 0) return null;
            return String.Join(",", different.ToArray());
        }
        public override bool Equals(object obj)
        {
            if (typeof(ComponentData) != obj.GetType())
                return base.Equals(obj);
            var other = (ComponentData)obj;
            return
                other.health == this.health &&
                other.maxHealth == this.maxHealth &&
                other.repairCooldownProgress == this.repairCooldownProgress &&
                other.rebuildProgress == this.rebuildProgress &&
                other.fireStacks == this.fireStacks &&
                other.buffProgress == this.buffProgress &&
                other.buffRemaining == this.buffRemaining &&
                other.fireproof == this.fireproof &&
                other.failsafed == this.failsafed;
        }
    }

    [JsonObject(MemberSerialization.Fields)]
    public class GunComponentData
    {
        ComponentData component;
        int? gun;                    // Gun ID
        int? loadedAmmo;             // Skill ID (-1 for normal)
        int? ammoRemaining;
        int? ammoMax;
        float? reloadProgress;       // 0 -> 1, resets back to 0 when loaded

        // Create snapshot from current data
        public GunComponentData(Turret gun)
        {
            this.component = new ComponentData(gun);
            this.gun = gun.ItemId;
            this.loadedAmmo = gun.AmmoEquipmentID;
            this.ammoRemaining = gun.Ammunition;
            this.ammoMax = gun.AmmunitionClipSize;
            this.reloadProgress = gun.ReloadProgress;
        }

        // Create diff from current data and snapshot
        public static GunComponentData GunComponentDataDiff(GunComponentData snapshot, GunComponentData current)
        {
            GunComponentData diff = new GunComponentData();

            diff.component = ComponentData.ComponentDataDiff(snapshot.component, current.component);
            diff.gun = Helper.ValueNullIfEqual(snapshot.gun, current.gun);
            diff.loadedAmmo = Helper.ValueNullIfEqual(snapshot.loadedAmmo, current.loadedAmmo);
            diff.ammoRemaining = Helper.ValueNullIfEqual(snapshot.ammoRemaining, current.ammoRemaining);
            diff.ammoMax = Helper.ValueNullIfEqual(snapshot.ammoMax, current.ammoMax);
            diff.reloadProgress = Helper.ValueNullIfEqual(snapshot.reloadProgress, current.reloadProgress);

            if (diff.component == null &&
                diff.gun == null &&
                diff.loadedAmmo == null &&
                diff.ammoRemaining == null &&
                diff.ammoMax == null &&
                diff.reloadProgress == null)
                return null;

            return diff;
        }

        private GunComponentData() { }

        public void Merge(GunComponentData other)
        {
            if (other.component != null) this.component.Merge(other.component);
            if (other.gun != null) this.gun = other.gun;
            if (other.loadedAmmo != null) this.loadedAmmo = other.loadedAmmo;
            if (other.ammoRemaining != null) this.ammoRemaining = other.ammoRemaining;
            if (other.ammoMax != null) this.ammoMax = other.ammoMax;
            if (other.reloadProgress != null) this.reloadProgress = other.reloadProgress;
        }
        public string DebugEquals(GunComponentData other)
        {
            List<string> different = new List<string>();

            var componentTest = this.component.DebugEquals(other.component);
            if (componentTest != null) different.Add(componentTest);

            if (!other.gun.Equals(this.gun)) different.Add("gun");
            if (!other.loadedAmmo.Equals(this.loadedAmmo)) different.Add("loadedAmmo");
            if (!other.ammoRemaining.Equals(this.ammoRemaining)) different.Add("ammoRemaining");
            if (!other.ammoMax.Equals(this.ammoMax)) different.Add("ammoMax");
            if (!other.reloadProgress.Equals(this.reloadProgress)) different.Add("reloadProgress");

            if (different.Count == 0) return null;
            return String.Join(",", different.ToArray());
        }
        public override bool Equals(object obj)
        {
            if (typeof(GunComponentData)!=obj.GetType())
                return base.Equals(obj);
            var other = (GunComponentData)obj;
            return
                other.component.Equals(this.component) &&
                other.gun == this.gun &&
                other.loadedAmmo == this.loadedAmmo &&
                other.ammoRemaining == this.ammoRemaining &&
                other.ammoMax == this.ammoMax &&
                other.reloadProgress == this.reloadProgress;
        }
    }

    [JsonObject(MemberSerialization.Fields)]
    public class PlayerData
    {
        string name;
        string playerClass;
        int[] equipment;
        int? selectedEquipment;

        // Create snapshot from current data
        public PlayerData(NetworkedPlayer player)
        {
            this.name = player.name;
            switch (player.PlayerClass)
            {
                case AvatarClass.Pilot:
                    this.playerClass = "pilot";
                    break;
                case AvatarClass.Engineer:
                    this.playerClass = "engineer";
                    break;
                case AvatarClass.Gunner:
                    this.playerClass = "gunner";
                    break;
            }
            this.equipment = new int[player.equipment.Count];
            for (var i = 0; i < player.equipment.Count; i++)
            {
                var tool = player.equipment[i];
                this.equipment[i] = tool.ActivationId;
            }
            this.selectedEquipment = player.CurrentToolActivationId;
        }

        // Create diff from current data and snapshot
        public static PlayerData PlayerDataDiff(PlayerData snapshot, PlayerData current)
        {
            PlayerData diff = new PlayerData();

            diff.name = Helper.NullIfEqual(snapshot.name, current.name);
            diff.playerClass = Helper.NullIfEqual(snapshot.playerClass, current.playerClass);
            diff.equipment = Helper.ValueListNullIfEqual(snapshot.equipment, current.equipment);
            diff.selectedEquipment = Helper.ValueNullIfEqual(snapshot.selectedEquipment, current.selectedEquipment);

            if (diff.name == null &&
                diff.playerClass == null && 
                diff.equipment == null && 
                diff.selectedEquipment == null)
                return null;

            return diff;
        }
        private PlayerData() { }

        public void Merge(PlayerData other)
        {
            if (other.name != null) this.name = other.name;
            if (other.playerClass != null) this.playerClass = other.playerClass;
            if (other.equipment != null) this.equipment = other.equipment;
            if (other.selectedEquipment != null) this.selectedEquipment = other.selectedEquipment;
        }
        public string DebugEquals(PlayerData other)
        {
            List<string> different = new List<string>();

            if (!other.name.Equals(this.name)) different.Add("name");
            if (!other.playerClass.Equals(this.playerClass)) different.Add("playerClass");
            if (other.equipment.Length != this.equipment.Length)
                different.Add("equipment length");
            else
                for (var i = 0; i < this.equipment.Length; i++)
                    if (this.equipment[i] != other.equipment[i])
                        different.Add(String.Format("equipment {0}", i));
            if (!other.selectedEquipment.Equals(this.selectedEquipment)) different.Add("selectedEquipment");

            if (different.Count == 0) return null;
            return String.Join("\n", different.ToArray());
        }
    }
}
