using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using static BuffKit.Util.Util;

namespace BuffKit.LobbyTimer
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
            var tbc = TimerButtonContainer.Instance;

            var lobbyTimer = mlv.gameObject.AddComponent<Timer>();
            lobbyTimer.gameObject.SetActive(true);
            lobbyTimer.MatchId = mlv.MatchId;
            lobbyTimer.Initialize(tbc);
        }
    }
    
    [HarmonyPatch(typeof(MatchLobbyView), "OnDisable")]
    public class MatchLobbyView_OnDisable
    {
        public static void Prefix(MatchLobbyView __instance)
        {
            var lobbyTimer = __instance?.gameObject?.GetComponent<Timer>();
            if (lobbyTimer.Equals(null)) return;
            Object.Destroy(lobbyTimer);
        }
    }
    
    [HarmonyPatch(typeof(UIMatchLobby), "Awake")]
    public class UIMatchLobby_Awake
    {
        public static void Postfix()
        {
            var lobby = UIMatchLobby.Instance;

            var imagePrototypeButton =
                Object.Instantiate(lobby.engineerButton, lobby.engineerButton.transform.parent);
            imagePrototypeButton.transform.name = "Image Prototype Button";
            imagePrototypeButton.gameObject.SetActive(false);

            var le = lobby.transform.FindChild("Lobby Main Panel").gameObject.GetComponent<HorizontalLayoutGroup>();
            le.childForceExpandWidth = false;

            var le2 = le.transform.FindChild("Map Panel").gameObject.GetComponent<LayoutElement>();
            le2.preferredWidth = 375;

            //An empty layout element that takes up all the extra space available
            //Pushes the timer container to the right
            var spacerGo = new GameObject("Spacer");
            spacerGo.transform.parent = imagePrototypeButton.transform.parent;
            spacerGo.AddComponent<LayoutElement>().flexibleWidth = 1f;

            var tbcGo = new GameObject("Timer Button Container");
            tbcGo.transform.parent = imagePrototypeButton.transform.parent;
            tbcGo.SetActive(false);

            // But hey, at least I don't have to touch the cache, right?
            var font = imagePrototypeButton
                .transform
                .parent
                .FindChild("Ship Loadout Button/Label")
                .gameObject
                .GetComponent<Text>()
                .font;
            
            TimerButtonContainer.Instance = tbcGo.AddComponent<TimerButtonContainer>();
            TimerButtonContainer.Instance.Initialize(imagePrototypeButton, font);
        }
    }
}