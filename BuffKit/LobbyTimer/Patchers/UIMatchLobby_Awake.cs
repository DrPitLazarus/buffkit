using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace BuffKit.LobbyTimer.Patchers
{
    [HarmonyPatch(typeof(UIMatchLobby), "Awake")]
    public class UIMatchLobby_Awake
    {
        public static void Postfix()
        {
            var lobby = UIMatchLobby.Instance;

            var textPrototypeButton = 
                Object.Instantiate(lobby.shipCustomizationButton, lobby.shipCustomizationButton.transform.parent);
            textPrototypeButton.transform.name = "prototypeButton";
            textPrototypeButton.gameObject.SetActive(false);
            
            var spacerGo = new GameObject("Spacer");
            spacerGo.transform.parent = textPrototypeButton.transform.parent;
            var spacerLe = spacerGo.AddComponent<LayoutElement>();
            spacerLe.flexibleWidth = 1f;

            var tbcGo = new GameObject("Timer Button Container");
            tbcGo.SetActive(false);
            tbcGo.transform.parent = textPrototypeButton.transform.parent;

            var tbc = tbcGo.AddComponent<TimerButtonContainer>();
            TimerButtonContainer.Instance = tbc;
            tbc.LayoutGroup = tbcGo.AddComponent<HorizontalLayoutGroup>();
            tbc.LayoutElement = tbcGo.AddComponent<LayoutElement>();

            tbc.LayoutGroup.spacing = 5f;
            tbc.LayoutGroup.childForceExpandHeight = false;
            tbc.LayoutGroup.childForceExpandWidth = false;
            
            var imagePrototypeButton = 
                Object.Instantiate(lobby.engineerButton, lobby.shipCustomizationButton.transform.parent);
            imagePrototypeButton.transform.name = "Image Prototype Button";
            imagePrototypeButton.gameObject.SetActive(false);

            var icons = new Dictionary<string, Texture2D>();
            var assetPath = @"BepInEx\plugins\BuffKit\Assets\Timer";
            var gp = Directory.GetCurrentDirectory();
            var path = Path.Combine(gp, assetPath);
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                var data = File.ReadAllBytes(file);
                var texture = new Texture2D(16, 16);
                texture.LoadImage(data);
                icons.Add(Path.GetFileNameWithoutExtension(file), texture);
            }

            GameObject iconGo;
            Image image;
            UIHoverTooltipTarget tt;
            
            tbc.StartTimerButton = 
                Object.Instantiate(imagePrototypeButton, tbcGo.transform);
            tbc.StartTimerButton.name = "Start Timer Button";

            tt = tbc.StartTimerButton.gameObject.GetComponent<UIHoverTooltipTarget>();
            tt.tooltip = "Start the timer or overtime (or resume if currently paused)";
            
            iconGo = tbc.StartTimerButton.transform.Find("Icon").gameObject;
            image = iconGo.GetComponent<Image>();
            image.sprite = Sprite.Create(icons["start"], new Rect(new Vector2(0, 0), new Vector2(128, 128)),
                new Vector2(64, 64));
            
            tbc.PauseTimerButton = 
                Object.Instantiate(imagePrototypeButton, tbcGo.transform);
            tbc.PauseTimerButton.name = "Pause Timer Button";
            
            tt = tbc.PauseTimerButton.gameObject.GetComponent<UIHoverTooltipTarget>();
            tt.tooltip = "Pause the timer";
            
            iconGo = tbc.PauseTimerButton.transform.Find("Icon").gameObject;
            image = iconGo.GetComponent<Image>();
            image.sprite = Sprite.Create(icons["pause"], new Rect(new Vector2(0, 0), new Vector2(128, 128)),
                new Vector2(64, 64));
            
            tbc.ExtendPauseButton = 
                Object.Instantiate(imagePrototypeButton, tbcGo.transform);
            tbc.ExtendPauseButton.name = "Extend Pause Button";
            
            tt = tbc.ExtendPauseButton.gameObject.GetComponent<UIHoverTooltipTarget>();
            tt.tooltip = "Extend the pause by 2 minutes, only available once";
            
            iconGo = tbc.ExtendPauseButton.transform.Find("Icon").gameObject;
            image = iconGo.GetComponent<Image>();
            image.sprite = Sprite.Create(icons["extend"], new Rect(new Vector2(0, 0), new Vector2(128, 128)),
                new Vector2(64, 64));
            
            tbc.RefPauseTimerButton = 
                Object.Instantiate(imagePrototypeButton, tbcGo.transform);
            tbc.RefPauseTimerButton.name = "Ref Pause Button";
            
            tt = tbc.RefPauseTimerButton.gameObject.GetComponent<UIHoverTooltipTarget>();
            tt.tooltip = "Stop the timer until manually resumed";
            
            iconGo = tbc.RefPauseTimerButton.transform.Find("Icon").gameObject;
            image = iconGo.GetComponent<Image>();
            image.sprite = Sprite.Create(icons["halt"], new Rect(new Vector2(0, 0), new Vector2(128, 128)),
                new Vector2(64, 64));
        }
    }
}