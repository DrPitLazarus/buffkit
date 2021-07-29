using HarmonyLib;

namespace BuffKit.LobbyTimer.Patchers
{
    [HarmonyPatch(typeof(UIManager.UINewMatchLobbyState), "Enter")]
    public class UINewMatchLobbyStateEnter
    {
        public static void Postfix()
        {
            MuseLog.Info("UINewMatchLobbyState entered");
            var lobby = UIMatchLobby.Instance;
            MuseLog.Info(lobby.ToString());
            lobby.engineerButton.gameObject.SetActive(false);
            lobby.gunnerButton.gameObject.SetActive(false);
            lobby.pilotButton.gameObject.SetActive(false);
            lobby.shipCustomizationButton.gameObject.SetActive(false);
            lobby.charCustomizationButton.gameObject.SetActive(false);
            
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