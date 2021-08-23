using HarmonyLib;

namespace BuffKit.AutoStartTimer
{
    [HarmonyPatch(typeof(Deathmatch), "RemoteInitialize")]
    class Deathmatch_RemoteInitialize
    {
        private static bool _firstPrepare = true;
        private static void Prepare()
        {
            if (_firstPrepare)
            {
                Util.Util.OnGameInitialize += delegate
                {
                    MatchBlockerView.Done += AutoStartTimer.TryStartTimer;
                    Settings.Settings.Instance.AddEntry("ref tools", "auto start timer", AutoStartTimer.SetEnabled, false);
                };
                _firstPrepare = false;
            }
        }
    }
}
