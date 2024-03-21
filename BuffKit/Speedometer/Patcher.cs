using HarmonyLib;

namespace BuffKit.Speedometer
{
    [HarmonyPatch]
    public class SpeedometerPatcher
    {
        private static bool _enabled = true;
        private static bool _firstPrepare = true;

        private static void Prepare()
        {
            if (!_firstPrepare) return;
            Settings.Settings.Instance.AddEntry("speedometer", "speedometer", v => _enabled = v, _enabled);
            _firstPrepare = false;
        }

        [HarmonyPatch(typeof(Mission), "Start")]
        [HarmonyPostfix]
        private static void Mission_Start()
        {
            if (!_enabled) return;
            Speedometer.Initialize();
        }

        [HarmonyPatch(typeof(UIManager.UIGamePlayState), "ActivateHUDElements")]
        [HarmonyPostfix]
        private static void UIManager_ActivateHUDElements()
        {
            if (!_enabled) return;
            Speedometer.SetActive(true);
        }

        [HarmonyPatch(typeof(UIManager.UIGamePlayState), "DeactivateHUDElements")]
        [HarmonyPostfix]
        private static void UIManager_DeactivateHUDElements()
        {
            if (!_enabled) return;
            Speedometer.SetActive(false);
        }
    }
}