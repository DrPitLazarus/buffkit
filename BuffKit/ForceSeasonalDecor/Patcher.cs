using HarmonyLib;
using MuseBase.Multiplayer.Unity;
using UnityEngine;

namespace BuffKit.ForceSeasonalDecor
{
    [HarmonyPatch(typeof(MuseWorldClient), "SetupCache")]
    public class ForceSeasonalDecor
    {
        private static bool _forceFireworks = false;
        private static bool _forceSpookyAi = false;
        private static bool _forceChristmasTrees = false;
        private static bool _firstPrepare = true;

        private static void Prepare()
        {
            if (_firstPrepare)
            {
                Settings.Settings.Instance.AddEntry("misc", "force seasonal fireworks", v => _forceFireworks = v, _forceFireworks);
                Settings.Settings.Instance.AddEntry("misc", "force seasonal spooky ai", v => _forceSpookyAi = v, _forceSpookyAi);
                Settings.Settings.Instance.AddEntry("misc", "force seasonal christmas trees", v => _forceChristmasTrees = v, _forceChristmasTrees);
                _firstPrepare = false;
            }
        }

        [HarmonyPatch(typeof(PlayerAirshipAssetBehaviour), "Awake")]
        [HarmonyPostfix]
        private static void Fireworks(PlayerAirshipAssetBehaviour __instance, ref OneShotEffectSystem ___fireworksDestroyedEffect)
        {
            if (!_forceFireworks) return;
            MuseLog.Info("Enable fireworks!");
            if (___fireworksDestroyedEffect != null)
            {
                __instance.ShipDestroyedEffect = ___fireworksDestroyedEffect;
            }
        }

        [HarmonyPatch(typeof(NetworkedPlayer), "Start")]
        [HarmonyPostfix]
        private static void SpookyAi(ref PlayerGraphics ___graphics, ref int ___userId)
        {
            if (!_forceSpookyAi) return;
            MuseLog.Info("Enable spooky AI!");
            ___graphics.IsGhost = ___userId <= 0;
        }

        [HarmonyPatch(typeof(PlayerAirshipAssetBehaviour), "Awake")]
        [HarmonyPostfix]
        private static void ChristmasTrees(ref GameObject ___christmasTree)
        {
            if (!_forceChristmasTrees) return;
            MuseLog.Info("Enable christmas trees!");
            ___christmasTree.SetActive(true);
        }
    }
}