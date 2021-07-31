using HarmonyLib;
using static BuffKit.Util;

namespace BuffKit.AnnounceChanges
{
    [HarmonyPatch(typeof(MatchLobbyView), "Awake")]
    public class MatchLobbyView_Awake
    {
        private static void Prepare()
        {
            AnnounceChanges.CreateLog();
        }
        private static void Postfix(MatchLobbyView __instance)
        {
            __instance.lobbyDataChanged += delegate
            {
                AnnounceChanges.LobbyDataChanged(__instance);
            };
        }
    }

    [HarmonyPatch(typeof(UIManager.UINewMatchLobbyState), "Enter")]
    public class UINewMatchLobbyState_Enter
    {
        private static void Postfix()
        {
            var mlv = MatchLobbyView.Instance;
            
        }
    }
}
