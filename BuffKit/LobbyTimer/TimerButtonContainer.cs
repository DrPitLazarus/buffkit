using System;
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