using System;
using System.Collections.Generic;
using Muse.Goi2.Entity;
using Muse.Icarus.Common;
using Newtonsoft.Json;

namespace BuffKit.Broadcast
{
    [JsonObject(MemberSerialization.Fields)]
    public class MatchData
    {
        string lobbyID;
        string lobbyName;
        int mapID;
        int[] scores;
        float matchTimer;
        ShipData[] ships;
        string[] killfeed;
        // TODO: add killfeed messages

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
    }

    [JsonObject(MemberSerialization.Fields)]
    public class ShipData
    {
        string name;
        string shipClass;
        int team;
        float[] position;
        float[] velocity;
        float yaw;
        float angularVelocity;
        bool spotted;
        float hull;
        float maxHull;
        ComponentData armor;
        ComponentData balloon;
        ComponentData[] engines;
        GunComponentData[] guns;
        PlayerData[] crew;

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
    }

    [JsonObject(MemberSerialization.Fields)]
    public class ComponentData
    {
        float health;
        float maxHealth;
        float repairCooldownProgress;   // 0 (started) -> 1 (no cooldown)
        float rebuildProgress;          // 0 (dead) -> 1
        int fireStacks;                 // 0 -> 20
        float buffProgress;             // 0 (nothing) -> 1 (finished buffing)
        float buffRemaining;            // 1 (buffed) -> 0
        bool fireproof;
        bool failsafed;

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
    }

    [JsonObject(MemberSerialization.Fields)]
    public class GunComponentData
    {
        ComponentData component;
        int gun;                    // Gun ID
        int loadedAmmo;             // Skill ID (-1 for normal)
        int ammoRemaining;
        int ammoMax;
        float reloadProgress;       // 0 -> 1, resets back to 0 when loaded

        public GunComponentData(Turret gun)
        {
            this.component = new ComponentData(gun);
            this.gun = gun.ItemId;
            this.loadedAmmo = gun.AmmoEquipmentID;
            this.ammoRemaining = gun.Ammunition;
            this.ammoMax = gun.AmmunitionClipSize;
            this.reloadProgress = gun.ReloadProgress;
        }
    }

    [JsonObject(MemberSerialization.Fields)]
    public class PlayerData
    {
        string name;
        string playerClass;
        int[] equipment;
        int selectedEquipment;

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
    }
}
