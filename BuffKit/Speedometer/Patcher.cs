using System.Collections.Generic;
using BuffKit.Settings;
using HarmonyLib;
using UnityEngine;
using Resources = BuffKit.UI.Resources;

namespace BuffKit.Speedometer
{
    [HarmonyPatch]
    public class SpeedometerPatcher
    {
        public static bool Enabled { get; private set; } = false;
        public static ToggleGrid DisplaySettings { get; private set; }
        private static bool _firstPrepare = true;

        private static void Prepare()
        {
            if (!_firstPrepare) return;
            Settings.Settings.Instance.AddEntry("speedometer", "speedometer", v => Enabled = v, Enabled);

            Util.OnGameInitialize += delegate
            {
                var gridIcons = new List<Sprite>() { Resources.PilotIcon, Resources.GunnerIcon, Resources.EngineerIcon };
                var gridLabels = new List<string> { "horizontal speed", "vertical speed", "rotation speed", "x position east/west", "y position altitude", "z position north/south" };
                DisplaySettings = new ToggleGrid(gridIcons, gridLabels, true);
                Settings.Settings.Instance.AddEntry("speedometer", "speedometer display", v => DisplaySettings = v, DisplaySettings);
            };

            _firstPrepare = false;
        }

        [HarmonyPatch(typeof(Mission), "Start")]
        [HarmonyPostfix]
        private static void Mission_Start()
        {
            if (!Enabled) return;
            Speedometer.Initialize();
            Speedometer.UpdateVisibility();
        }

        [HarmonyPatch(typeof(UIManager.UIGamePlayState), "ActivateHUDElements")]
        [HarmonyPostfix]
        private static void UIManager_ActivateHUDElements()
        {
            if (!Enabled) return;
            Speedometer.SetActive(true);
        }

        [HarmonyPatch(typeof(UIManager.UIGamePlayState), "DeactivateHUDElements")]
        [HarmonyPostfix]
        private static void UIManager_DeactivateHUDElements()
        {
            if (!Enabled) return;
            Speedometer.SetActive(false);
        }
    }
}