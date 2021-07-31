using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace BuffKit.LobbyTimer.Patchers
{
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