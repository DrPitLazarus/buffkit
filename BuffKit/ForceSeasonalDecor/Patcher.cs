using HarmonyLib;
using MuseBase.Multiplayer.Unity;
using UnityEngine;

namespace BuffKit.ForceSeasonalDecor
{
    [HarmonyPatch(typeof(MuseWorldClient), "SetupCache")]
    public class ForceSeasonalDecor
    {
        private static bool forceFireworks = false;
        private static bool forceSpookyAi = false;
        private static bool forceChristmasTrees = false;
        private static bool firstPrepare = true;

        private static void Prepare()
        {
            if (firstPrepare)
            {
                Settings.Settings.Instance.AddEntry("misc", "force seasonal fireworks", v => forceFireworks = v, forceFireworks);
                Settings.Settings.Instance.AddEntry("misc", "force seasonal spooky ai", v => forceSpookyAi = v, forceSpookyAi);
                Settings.Settings.Instance.AddEntry("misc", "force seasonal christmas trees", v => forceChristmasTrees = v, forceChristmasTrees);
                firstPrepare = false;
            }
        }

        [HarmonyPatch(typeof(PlayerAirshipAssetBehaviour), "Awake")]
        [HarmonyPostfix]
        private static void Fireworks(PlayerAirshipAssetBehaviour __instance, ref OneShotEffectSystem ___fireworksDestroyedEffect)
        {
            if (!forceFireworks) return;
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
            if (!forceSpookyAi) return;
            MuseLog.Info("Enable spooky AI!");
            ___graphics.IsGhost = ___userId <= 0;
        }

        [HarmonyPatch(typeof(PlayerAirshipAssetBehaviour), "Awake")]
        [HarmonyPostfix]
        private static void ChristmasTrees(ref GameObject ___christmasTree)
        {
            if (!forceChristmasTrees) return;
            MuseLog.Info("Enable christmas trees!");
            ___christmasTree.SetActive(true);
        }
    }
}