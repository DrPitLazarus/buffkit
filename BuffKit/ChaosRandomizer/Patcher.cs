﻿using HarmonyLib;

namespace BuffKit.ChaosRandomizer
{
    [HarmonyPatch(typeof(UIManager.UINewMatchLobbyState), "Enter")]
    class UIManager_UINewMatchLobbyState_Enter
    {
        private static bool _firstPrepare = true;
        private static void Prepare()
        {
            if (_firstPrepare)
            {
                _firstPrepare = false;
                Util.OnGameInitialize += delegate
                {
                    ChaosRandomizer.Initialize();
                    MatchLobbyView.enterMatchLobby += mlv =>
                    {
                        if (!ChaosRandomizer.Enabled) return;
                        ChaosRandomizer.Instance.EnterLobby(mlv);
                    };
                    MatchLobbyView.exitMatchLobby += mlv =>
                    {
                        if (!ChaosRandomizer.Enabled) return;
                        ChaosRandomizer.Instance.ExitLobby(mlv);
                    };
                };
            }
        }
        private static void Postfix()
        {
            if (!ChaosRandomizer.Enabled) return;
            ChaosRandomizer.Instance.EnterLobby(MatchLobbyView.Instance);
        }
    }

    [HarmonyPatch(typeof(UIPageFrame), "TryHideOverlay")]
    class UIPageFrame_TryHideOverlay
    {
        private static void Postfix(ref bool __result)
        {
            __result = __result || ChaosRandomizer.Instance.TryHidePanel();
        }
    }

    [HarmonyPatch(typeof(UIPageFrame), "HideAllElements")]
    class UIPageFrame_HideAllElements
    {
        private static void Postfix()
        {
            ChaosRandomizer.Instance?.TryHidePanel();
        }
    }
}
