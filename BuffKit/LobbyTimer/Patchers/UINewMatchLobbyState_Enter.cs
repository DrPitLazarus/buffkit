using HarmonyLib;
using UnityEngine;
using static BuffKit.Util.Util;

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
        }
    }
    [HarmonyPatch(typeof(MatchLobbyView), "Start")]
    public class MatchLobbyView_Start
    {
        public static void Postfix()
        {
            var mlv = MatchLobbyView.Instance;
            if (!HasModPrivilege(mlv)) return;
            var lobbyTimer = mlv.gameObject.AddComponent<Timer>();
            
            var tbc = TimerButtonContainer.Instance;

            lobbyTimer = mlv.gameObject.AddComponent<Timer>();
            lobbyTimer.gameObject.SetActive(true);
            lobbyTimer.MatchId = mlv.MatchId;
            lobbyTimer.Initialize(tbc);
        }
    }
    
    [HarmonyPatch(typeof(MatchLobbyView), "OnDisable")]
    public class MatchLobbyView_OnDisable
    {
        public static void OnDisable()
        {
            var mlv = MatchLobbyView.Instance;
            if (!HasModPrivilege(mlv)) return;
            var lobbyTimer = mlv.gameObject.GetComponent<Timer>();
            Object.Destroy(lobbyTimer);
        }
    }
}