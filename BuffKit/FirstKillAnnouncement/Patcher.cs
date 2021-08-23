using HarmonyLib;

namespace BuffKit.FirstKillAnnouncement
{
    [HarmonyPatch(typeof(Deathmatch), "OnRemoteUpdate")]
    class Deathmatch_OnRemoteUpdate
    {
        private static bool _firstPrepare = true;
        private static void Prepare()
        {
            if (_firstPrepare)
            {
                Util.Util.OnGameInitialize += delegate
                {
                    FirstKillAnnouncement.Initialize();
                    Settings.Settings.Instance.AddEntry("ref tools", "announce first kill", FirstKillAnnouncement.Instance.SetEnabled, false);
                };
                _firstPrepare = false;
            }
        }
        private static void Postfix(Deathmatch __instance)
        {
            FirstKillAnnouncement.Instance.OnMatchUpdate(__instance);
        }
    }
}
