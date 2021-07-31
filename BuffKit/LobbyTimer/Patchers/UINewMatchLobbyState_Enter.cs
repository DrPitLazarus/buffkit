using HarmonyLib;
using static BuffKit.Util;

namespace BuffKit.LobbyTimer.Patchers
{
    [HarmonyPatch(typeof(UIManager.UINewMatchLobbyState), "Enter")]
    public class UINewMatchLobbyState_Enter
    {
        public static void Postfix()
        {
            var mlv = MatchLobbyView.Instance;
            if (!HasModPrivilege(mlv)) return;
            
            var tbc = TimerButtonContainer.Instance;
            tbc.gameObject.SetActive(true);

            var lobbyTimer = mlv.gameObject.GetComponent<Timer>();
            if (lobbyTimer == null)
            {
                lobbyTimer = mlv.gameObject.AddComponent<Timer>();
                lobbyTimer.gameObject.SetActive(true);
                lobbyTimer.Initialize(tbc);
            }
        }
    }
}