using HarmonyLib;

namespace BuffKit.AnnounceChanges
{
    [HarmonyPatch(typeof(MatchLobbyView), "Awake")]
    public class MatchLobbyView_Awake
    {
        private static bool _firstPrepare = true;
        private static void Prepare()
        {
            if (_firstPrepare)
            {
                AnnounceChanges.CreateLog();
                Util.OnGameInitialize += delegate
                {
                    Settings.Settings.Instance.AddEntry("ref tools", "announce changes", b => AnnounceChanges.IsEnabled = b, true);
                };
                _firstPrepare = false;
            }
        }

        private static void Postfix(MatchLobbyView __instance)
        {
            __instance.lobbyDataChanged += delegate { AnnounceChanges.LobbyDataChanged(__instance); };
        }
    }
}