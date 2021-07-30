using System.IO;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace BuffKit.LobbyTimer.Patchers
{
    [HarmonyPatch(typeof(UIManager.UINewMatchLobbyState), "Enter")]
    public class UINewMatchLobbyState_Enter
    {
        public static void Postfix()
        {
            MuseLog.Info("UINewMatchLobbyState entered");
            var lobby = UIMatchLobby.Instance;
            MuseLog.Info(lobby.ToString());

            var mlv = MatchLobbyView.Instance;
            var tbc = TimerButtonContainer.Instance;
            tbc.gameObject.SetActive(true);
            
            var lobbyTimer = mlv.gameObject.GetComponent<Timer>();
            if (lobbyTimer == null)
            {
                lobbyTimer = mlv.gameObject.AddComponent<Timer>();
                lobbyTimer.Act(tbc.Repaint);
            }
        }
    }
}