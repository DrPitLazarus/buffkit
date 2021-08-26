﻿using HarmonyLib;

namespace BuffKit.Util
{
    [HarmonyPatch(typeof(UIManager.UILoadingLobbyState), "Exit")]
    public class UILoadingLobbyState_Exit
    {
        static bool firstCall = true;

        private static void Postfix()
        {
            if (firstCall)
            {
                firstCall = false;
                Util.Initialize();
            }
            else
                Util.OnLobbyLoadTrigger(); // Util._Initialize calls this
        }
    }

    [HarmonyPatch(typeof(UIManager.UIInitialState), "Exit")]
    public class UIInitialState_Exit
    {
        private static void Postfix()
        {
            UI.Resources.Initialize();
            Settings.Settings.Initialize();
        }
    }
}