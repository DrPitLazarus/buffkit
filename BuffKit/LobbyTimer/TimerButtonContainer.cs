using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace BuffKit.LobbyTimer
{
    public class TimerButtonContainer : MonoBehaviour
    {
        public HorizontalLayoutGroup LayoutGroup;
        public LayoutElement LayoutElement;
        public Button StartButton;
        public Button PauseButton;
        public Button RefPauseButton;
        public Button ExtendButton;
        public Text StatusLabel;
        public Text CountdownLabel;
        public static TimerButtonContainer Instance { get; set; }

        public void Awake()
        {
            Instance = this;
        }

        public void Initialize(Button prototype, Font font)
        {
            LayoutGroup = gameObject.AddComponent<HorizontalLayoutGroup>();
            LayoutElement = gameObject.AddComponent<LayoutElement>();
            
            LayoutGroup.spacing = 5f;
            LayoutGroup.childForceExpandHeight = false;
            LayoutGroup.childForceExpandWidth = false;
            
            
            var statusGo = new GameObject("Status Label");
            statusGo.transform.parent = gameObject.transform;
            statusGo.SetActive(false);
            
            var statusLe = statusGo.AddComponent<LayoutElement>();
            statusLe.minHeight = 30;
            statusLe.preferredHeight = 30;

            StatusLabel = statusGo.AddComponent<Text>();
            StatusLabel.alignment = TextAnchor.MiddleRight;
            StatusLabel.text = "Waiting for start";
            StatusLabel.fontSize = 16;
            StatusLabel.font = font;
            
            var countdownGo = Instantiate(statusGo, gameObject.transform);
            CountdownLabel = countdownGo.GetComponent<Text>();
            countdownGo.name = "Countdown Label";
            CountdownLabel.text = "-:--";
            CountdownLabel.alignment = TextAnchor.MiddleCenter;
            
            var countdownLe = countdownGo.GetComponent<LayoutElement>();
            countdownLe.minHeight = 30;
            countdownLe.preferredHeight = 30;
            countdownLe.minWidth = 36;
            countdownLe.preferredWidth = 36;

            //TODO: figure out how to do this better
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
            
            StartButton = Instantiate(prototype, gameObject.transform);
            StartButton.name = "Start Timer Button";
            StartButton.gameObject.GetComponent<UIHoverTooltipTarget>()
                .tooltip = "Start the timer (or resume if currently paused)";
            StartButton.transform.FindChild("Icon").gameObject.GetComponent<Image>()
                .sprite = CreateSprite(icons["start"]);
            
            PauseButton = Instantiate(prototype, gameObject.transform);
            PauseButton.name = "Pause Timer Button";
            PauseButton.gameObject.GetComponent<UIHoverTooltipTarget>()
                .tooltip = "Pause the timer, uses one of the 2 available 2 minute long pauses";
            PauseButton.transform.FindChild("Icon").gameObject.GetComponent<Image>()
                .sprite = CreateSprite(icons["pause"]);
            
            ExtendButton = Instantiate(prototype, gameObject.transform);
            ExtendButton.name = "Extend Timer Button";
            ExtendButton.gameObject.GetComponent<UIHoverTooltipTarget>()
                .tooltip = "Extend the pause, uses one of the 2 available 2 minute long pauses";
            ExtendButton.transform.FindChild("Icon").gameObject.GetComponent<Image>()
                .sprite = CreateSprite(icons["extend"]);
            
            RefPauseButton = Instantiate(prototype, gameObject.transform);
            RefPauseButton.name = "Ref Pause Timer Button";
            RefPauseButton.gameObject.GetComponent<UIHoverTooltipTarget>()
                .tooltip = "Stop the timer until manually resumed";
            RefPauseButton.transform.FindChild("Icon").gameObject.GetComponent<Image>()
                .sprite = CreateSprite(icons["halt"]);
        }

        private static Sprite CreateSprite(Texture2D icon)
        {
            return Sprite.Create(icon, new Rect(new Vector2(0, 0), new Vector2(128, 128)),
                new Vector2(64, 64));
        }

        public void OnEnable()
        {
            StatusLabel?.gameObject?.SetActive(true);
            CountdownLabel?.gameObject?.SetActive(true);
            StartButton?.gameObject?.SetActive(true);
            PauseButton?.gameObject?.SetActive(true);
            RefPauseButton?.gameObject?.SetActive(true);
            ExtendButton?.gameObject?.SetActive(true);
        }
        
        public void SetStatus(string status)
        {
            StatusLabel.text = status;
        }

        public void SetCountdown(string timer)
        {
            CountdownLabel.text = timer;
        }
    }
}