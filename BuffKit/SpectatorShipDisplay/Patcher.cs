using System;
using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using Muse.Goi2.Entity;
using Muse.Goi2.Entity.Vo;
using UnityEngine;

namespace BuffKit.SpectatorShipDisplay
{
    // Create additional skill display icons (so gunners have all 6 skills displayed)
    [HarmonyPatch(typeof(UIShipDetailsView), "Awake")]
    class UIShipDetailsView_Awake
    {
        private static void Postfix(UIShipDetailsView __instance, CrewToolInspector[] ___inspectorCache, Color ___selectedToolColor)
        {
            // Create additional tool display slots
            for (var i = 0; i < ___inspectorCache.Length; i++)
            {
                var old_tools = ___inspectorCache[i].tools;

                var skillIcon5 = old_tools[4].gameObject;
                var skillIcon6 = GameObject.Instantiate(skillIcon5, skillIcon5.transform.parent);
                skillIcon6.name = "skillIcon6";

                var new_tools = new UIImage[old_tools.Length + 1];
                for (var j = 0; j < old_tools.Length; j++)
                {
                    new_tools[j] = old_tools[j];
                }
                var skillIcon6UIImage = skillIcon6.GetComponent<UIImage>();
                skillIcon6UIImage.X = 265;
                new_tools[old_tools.Length] = skillIcon6UIImage;

                skillIcon6UIImage.transform.parent.GetComponent<UITransform>().children.Add(skillIcon6UIImage);

                ___inspectorCache[i].tools = new_tools;
            }
            var parentUITransform = __instance.GetComponent<UITransform>();
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            typeof(UITransform).GetMethod("PropagateParentReference", flags).Invoke(parentUITransform, null);
        }
    }

    // Sort guns as in ship builder
    [HarmonyPatch(typeof(UIShipDetailsView), "DrawComponentIndicators")]
    class UIShipDetailsView_DrawComponentIndicators
    {
        private static void Prefix(ref IList<Repairable> repairables)
        {
            // Separate repairables into guns and not-guns
            var other_repairables = new List<Repairable>();
            var gun_repairables = new List<Repairable>();

            foreach (var r in repairables)
            {
                if (r.SlotType == ShipPartSlotType.GUN)
                    gun_repairables.Add(r);
                else
                    other_repairables.Add(r);
            }

            // Sort guns by slot index (same as in ship builder)
            var shipModelId = UIManager.UIOrbitState.Instance.TargetShip.ShipModelId;
            // NetworkedPlayer.Local.CurrentShip.ShipModelId;

            gun_repairables.Sort(
                (x, y) => Util.GetGunSlotIndex(shipModelId, x.SlotName).CompareTo(Util.GetGunSlotIndex(shipModelId, y.SlotName))
            );

            // Remake list
            repairables = other_repairables;
            foreach (var r in gun_repairables)
                repairables.Add(r);
        }
    }

}
