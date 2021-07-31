using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static BuffKit.Util;
namespace BuffKit.LobbyTimer
{
    public class TimerButtonContainer : MonoBehaviour
    {
        public HorizontalLayoutGroup LayoutGroup;
        public LayoutElement LayoutElement;
        public Button StartTimerButton;
        public Button PauseTimerButton;
        public Button RefPauseTimerButton;
        public Button ExtendPauseButton;
        public static TimerButtonContainer Instance { get; set; }

        public void ClearButtons()
        {
            MuseLog.Info("Clear buttons started");
            StartTimerButton.interactable = false;
            PauseTimerButton.interactable = false;
            RefPauseTimerButton.interactable = false;
            ExtendPauseButton.interactable = false;
        }

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
            
            MuseLog.Info(statusGo.ToString());
            var statusLe = statusGo.AddComponent<LayoutElement>();
            statusLe.minHeight = 30;
            statusLe.preferredHeight = 30;
            
            
            StatusLabel = statusGo.AddComponent<Text>();
            StatusLabel.alignment = TextAnchor.MiddleCenter;
            StatusLabel.text = "Waiting for start";
            StatusLabel.fontSize = 16;
            StatusLabel.font = font;
            
            var countdownGo = Instantiate(statusGo, gameObject.transform);
            countdownGo.name = "Countdown Label";
            countdownGo.GetComponent<Text>().text = "";
            
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
            StartTimerButton?.gameObject?.SetActive(true);
            PauseTimerButton?.gameObject?.SetActive(true);
            RefPauseTimerButton?.gameObject?.SetActive(true);
            ExtendPauseButton?.gameObject?.SetActive(true);
        }

        public void Repaint()
        {
            MuseLog.Info("Repaint started");
            var mlv = MatchLobbyView.Instance;
            if (mlv == null || NetworkedPlayer.Local == null) return;

            if (!HasModPrivilege(mlv)) return;
            
            ClearButtons();
            
            var lobbyTimer = mlv.gameObject.GetComponent<Timer>();
            MuseLog.Info($"{lobbyTimer.Buttons.Count} buttons to paint");
            foreach (var button in lobbyTimer.Buttons)
            {
                switch (button.Kind)
                {
                    case Timer.ButtonKind.StartTimer:
                        StartTimerButton.onClick.RemoveAllListeners();
                        StartTimerButton.onClick.AddListener(() => button.Action());
                        StartTimerButton.interactable = true;
                        break;
                    case Timer.ButtonKind.PauseTimer:
                        PauseTimerButton.onClick.RemoveAllListeners();
                        PauseTimerButton.onClick.AddListener(() => button.Action());
                        PauseTimerButton.interactable = true;
                        break;
                    case Timer.ButtonKind.ExtendPause:
                        ExtendPauseButton.onClick.RemoveAllListeners();
                        ExtendPauseButton.onClick.AddListener(() => button.Action());
                        ExtendPauseButton.interactable = true;
                        break;
                    case Timer.ButtonKind.RefPauseTimer:
                        RefPauseTimerButton.onClick.RemoveAllListeners();
                        RefPauseTimerButton.onClick.AddListener(() => button.Action());
                        RefPauseTimerButton.interactable = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}