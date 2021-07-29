using HarmonyLib;
using static BuffKit.Util;

namespace BuffKit.AnnounceChanges
{
    [HarmonyPatch(typeof(MatchLobbyView), "Awake")]
    public class MatchLobbyView_Awake
    {
        private static void Prepare()
        {
            CallOutChanges.CreateLog();
        }
        private static void Postfix(MatchLobbyView __instance)
        {

            __instance.lobbyDataChanged += delegate
            {
                CallOutChanges.LobbyDataChanged(__instance);
            };
        }
    }
}
