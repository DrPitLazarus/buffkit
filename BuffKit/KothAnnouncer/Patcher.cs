using BuffKit.MatchRefTools;
using BuffKit.UI;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace BuffKit.KothAnnouncer
{
    static class Patcher
    {
        private static bool _enableCKAnnouncer = false;
        
        public static void Init()
        {
            KothAnnouncer.Initialize();

            Settings.Settings.Instance.AddEntry("chaos", "Enable CK Announcer", delegate (bool v)
            {
                _enableCKAnnouncer = v;
                KothAnnouncer.Instance.SetEnabled(_enableCKAnnouncer);
            }, _enableCKAnnouncer);

        }
    }
    
    [HarmonyPatch(typeof(CrazyKing), "OnRemoteUpdate")]
    class CrazyKing_OnRemoteUpdate
    {
        private static bool _firstPrepare = true;
        private static void Prepare()
        {
            if (_firstPrepare)
            {
                Util.OnGameInitialize += Patcher.Init;
                _firstPrepare = false;
            }
        }
        private static void Postfix(CrazyKing __instance)
        {
            KothAnnouncer.Instance.OnMatchUpdate(__instance);
        }
    }
    
    
    class CrazyKing_OnMatchInitialize
    {
        private static bool _firstPrepare = true;
        private static void Prepare()
        {
            if (_firstPrepare)
            {
                Util.OnGameInitialize += Patcher.Init;
                _firstPrepare = false;
            }
        }
        private static void Postfix(CrazyKing __instance)
        {
            KothAnnouncer.Instance.OnMatchUpdate(__instance);
        }
    }
}
