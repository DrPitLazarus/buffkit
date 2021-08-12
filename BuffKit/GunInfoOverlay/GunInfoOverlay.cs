using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Muse.Goi2.Entity;
using TMPro;
using static BuffKit.Util.Util;

namespace BuffKit.GunInfoOverlay
{
    public static class GunInfoOverlay
    {
        /*
         * Info
         *   Name
         *   Direct damage (type, value)
         *   AoE damage (type, value)
         *   RoF
         *   Reload speed
         *   Magazine size
         *   Range
         * ====================================
         *   Projectile speed
         *   Arming time
         *   Buckshots
         *   Shell drop
         *   Fire chance
         *   AoE size
         *   Special effects (1 of)
         *     Pull (strength, duration)
         *     Flare (illumination, duration)
         *     Mine impulse
         *     Knockback
         * ====================================
         *   Arcs (horizontal, vertical)
         */

        private static BepInEx.Logging.ManualLogSource log;
        public static void CreateLog()
        {
            log = BepInEx.Logging.Logger.CreateLogSource("guninfo");
        }
        public static void ListAllGunDetails()
        {
            StringBuilder b = new StringBuilder();
            foreach (var id in GunIds)
            {
                var gun = CachedRepository.Instance.Get<GunItem>(id);
                DisplayGun(gun);
            }
        }
        public static void DisplayGun(GunItem gun)
        {
            var gunInfo = GunItemInfo.FromGunItem(gun);
            _lName.text = gunInfo.name;
            _lDamageDirect.text = $"Primary: {gunInfo.directDamage} {GetDamageTypeName(gunInfo.directDamageType)}";
            _lDamageAoE.text = $"Secondary: {gunInfo.areaDamage} {GetDamageTypeName(gunInfo.areaDamageType)}";
            _lRoF.text = $"RoF: {gunInfo.rateOfFire} shots/s";
            _lReloadTime.text = $"Reload time: {1f / gunInfo.reloadSpeed}s";
            _lClipSize.text = $"Clip size: {gunInfo.magazineSize}";
            _lRange.text = $"Range: {gunInfo.range}m ({gunInfo.RangeString})";

            _lProjectileSpeed.text = $"Speed: {gunInfo.projectileSpeed}m/s";
            _lArmingTime.text = $"Arming time: {gun.GetParam("fArmingDelay", "0")}s";
            _lBuckshots.text = $"Buckshots: {gunInfo.buckshots}";
            _lShellDrop.text = $"Drop: {gunInfo.shellDrop}m/s²";
            _lFireChance.text = $"Fire chance: {gunInfo.directFireChance * 100f}% chance of {gunInfo.directFireStacks} direct, {gunInfo.areaFireChance * 100f}% chance of {gunInfo.areaFireStacks} indirect";
            if (gunInfo.aoeRangeMax != gunInfo.aoeRangeMin)
                _lAoESize.text = $"AoE: {gunInfo.aoeRangeMin}m to {gunInfo.aoeRangeMax}m";
            else
                _lAoESize.text = $"AoE: {gunInfo.aoeRangeMin}m";

            string special = "No special";
            string strengthName = null;
            float strengthValue = 0;
            string strengthUnit = null;
            string durationName = null;
            float durationValue = 0;
            string durationUnit = null;
            if (gunInfo.TryGetSpecialEfx(ref strengthName, ref strengthValue, ref strengthUnit, ref durationName, ref durationValue, ref durationUnit))
            {
                special = "Special: ";
                if (!string.IsNullOrEmpty(strengthName))
                {
                    special += $"{strengthName}: {strengthValue}{strengthUnit}     ";
                }
                if (!string.IsNullOrEmpty(durationName))
                {
                    special += $"{durationName}: {durationValue}{durationUnit}";
                }
            }
            _lSpecialEffect.text = special;

            _lArcsHorizontal.text = $"Horizontal arcs: {gunInfo.leftAngle}° left, {gunInfo.rightAngle}° right";
            _lArcsVertical.text = $"Vertical arcs: {gunInfo.upAngle}° up, {gunInfo.downAngle}° down";

            log.LogInfo(GetDisplayString());
        }


        private static float GetGunParameter(GunItem gun, string paramKey)
        {
            string s;
            float result;
            if (gun.Params.TryGetValue(paramKey, out s) && float.TryParse(s, out result))
            {
                return result;
            }
            return 0f;
        }
        public static void CreatePanel()
        {
            var parent = GameObject.Find("/Menu UI/Standard Canvas/Common Elements/Info Overlay Window (don't hide)")?.transform;
            var obPanel = UI.Builder.BuildPanel(parent);
            var csf = obPanel.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            UI.Builder.BuildLabel(obPanel.transform, out _lName, TextAnchor.MiddleLeft, 10);
            UI.Builder.BuildLabel(obPanel.transform, out _lDamageDirect, TextAnchor.MiddleLeft, 10);
            UI.Builder.BuildLabel(obPanel.transform, out _lDamageAoE, TextAnchor.MiddleLeft, 10);
            UI.Builder.BuildLabel(obPanel.transform, out _lRoF, TextAnchor.MiddleLeft, 10);
            UI.Builder.BuildLabel(obPanel.transform, out _lReloadTime, TextAnchor.MiddleLeft, 10);
            UI.Builder.BuildLabel(obPanel.transform, out _lClipSize, TextAnchor.MiddleLeft, 10);
            UI.Builder.BuildLabel(obPanel.transform, out _lRange, TextAnchor.MiddleLeft, 10);

            UI.Builder.BuildLabel(obPanel.transform, out _lProjectileSpeed, TextAnchor.MiddleLeft, 10);
            UI.Builder.BuildLabel(obPanel.transform, out _lArmingTime, TextAnchor.MiddleLeft, 10);
            UI.Builder.BuildLabel(obPanel.transform, out _lBuckshots, TextAnchor.MiddleLeft, 10);
            UI.Builder.BuildLabel(obPanel.transform, out _lShellDrop, TextAnchor.MiddleLeft, 10);
            UI.Builder.BuildLabel(obPanel.transform, out _lFireChance, TextAnchor.MiddleLeft, 10);
            UI.Builder.BuildLabel(obPanel.transform, out _lAoESize, TextAnchor.MiddleLeft, 10);
            UI.Builder.BuildLabel(obPanel.transform, out _lSpecialEffect, TextAnchor.MiddleLeft, 10);

            UI.Builder.BuildLabel(obPanel.transform, out _lArcsHorizontal, TextAnchor.MiddleLeft, 10);
            UI.Builder.BuildLabel(obPanel.transform, out _lArcsVertical, TextAnchor.MiddleLeft, 10);
        }


        private static TextMeshProUGUI _lName, _lDamageDirect, _lDamageAoE, _lRoF, _lReloadTime, _lClipSize, _lRange;
        private static TextMeshProUGUI _lProjectileSpeed, _lArmingTime, _lBuckshots, _lShellDrop, _lFireChance, _lAoESize, _lSpecialEffect;
        private static TextMeshProUGUI _lArcsHorizontal, _lArcsVertical;
        private static string GetDisplayString()
        {
            StringBuilder b = new StringBuilder();
            b.Append($"\n{_lName.text}");
            b.Append($"\n{_lDamageDirect.text}");
            b.Append($"\n{_lDamageAoE.text}");
            b.Append($"\n{_lRoF.text}");
            b.Append($"\n{_lReloadTime.text}");
            b.Append($"\n{_lClipSize.text}");
            b.Append($"\n{_lRange.text}");

            b.Append($"\n{_lProjectileSpeed.text}");
            b.Append($"\n{_lArmingTime.text}");
            b.Append($"\n{_lBuckshots.text}");
            b.Append($"\n{_lShellDrop.text}");
            b.Append($"\n{_lFireChance.text}");
            b.Append($"\n{_lAoESize.text}");
            b.Append($"\n{_lSpecialEffect.text}");

            b.Append($"\n{_lArcsHorizontal.text}");
            b.Append($"\n{_lArcsVertical.text}");

            return b.ToString();
        }
    }
}
