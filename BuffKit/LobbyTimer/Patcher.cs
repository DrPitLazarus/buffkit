using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using static BuffKit.Util;

namespace BuffKit.LobbyTimer
{
    [HarmonyPatch(typeof(UIManager.UINewMatchLobbyState), "Enter")]
    class UINewMatchLobbyState_Enter
    {
        private static void Postfix()
        {
            var mlv = MatchLobbyView.Instance;
            var tbc = TimerButtonContainer.Instance;
            if (!HasModPrivilege(mlv))
            {
                tbc.gameObject.SetActive(false);
                return;
            }
            
            tbc.gameObject.SetActive(true);
        }
    }
    
    [HarmonyPatch(typeof(MatchLobbyView), "Start")]
    class MatchLobbyView_Start
    {
        private static void Postfix()
        {
            var mlv = MatchLobbyView.Instance;
            var tbc = TimerButtonContainer.Instance;
            if (!HasModPrivilege(mlv))
            {
                tbc.gameObject.SetActive(false);
                return;
            }

            tbc.gameObject.SetActive(true);
            var lobbyTimer = mlv.gameObject.AddComponent<Timer>();
            lobbyTimer.gameObject.SetActive(true);
            lobbyTimer.MatchId = mlv.MatchId;
            lobbyTimer.Initialize(tbc);
        }
    }
    
    [HarmonyPatch(typeof(MatchLobbyView), "OnDisable")]
    class MatchLobbyView_OnDisable
    {
        private static void Prefix(MatchLobbyView __instance)
        {
            var lobbyTimer = __instance?.gameObject?.GetComponent<Timer>();
            if (lobbyTimer == null) return;
            Object.Destroy(lobbyTimer);
        }
    }
    
    [HarmonyPatch(typeof(UIMatchLobby), "Awake")]
    class UIMatchLobby_Awake
    {
        private static void Postfix()
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