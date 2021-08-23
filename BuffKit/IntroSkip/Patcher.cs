using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;

namespace BuffKit.IntroSkip
{
    [HarmonyPatch(typeof(UIIntroScreen), "Activate")]
    class UIIntroScreen_Activate
    {
        private static bool _firstPrepare = true;
        private static bool _enableSkip = true;
        private static void Prepare()
        {
            if (_firstPrepare)
            {
                Util.Util.OnGameInitialize += delegate
                {
                    Settings.Settings.Instance.AddEntry("misc", "skip intro", delegate (bool v) { _enableSkip = v; }, _enableSkip);
                };
                _firstPrepare = false;
            }
        }
        private static void Postfix(UIIntroScreen __instance)
        {
            if (_enableSkip)
                __instance.Deactivate();
        }
    }
}
