using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static BuffKit.Util;
namespace BuffKit.LobbyTimer
{
    public class TimerButtonContainer : MonoBehaviour
    {
        public Button StartTimerButton;
        public Button StartOvertimeButton;
        public Button PauseTimerButton;
        public Button RefPauseTimerButton;
        public Button ResumeTimerButton;
        public Button ExtendPauseButton;
        public static TimerButtonContainer Instance { get; private set; }

        public void ClearButtons()
        {
            MuseLog.Info("Clear buttons started");
            StartTimerButton?.gameObject?.SetActive(false);
            StartOvertimeButton?.gameObject?.SetActive(false);
            PauseTimerButton?.gameObject?.SetActive(false);
            RefPauseTimerButton?.gameObject?.SetActive(false);
            ResumeTimerButton?.gameObject?.SetActive(false);
            ExtendPauseButton?.gameObject?.SetActive(false);
        }

        public void Awake()
        {
            Instance = this;
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
                        StartTimerButton.onClick.AddListener(() => button.Action());
                        StartTimerButton.gameObject.SetActive(true);
                        break;
                    case Timer.ButtonKind.StartOvertime:
                        StartOvertimeButton.onClick.AddListener(() => button.Action());
                        StartOvertimeButton.gameObject.SetActive(true);
                        break;
                    case Timer.ButtonKind.PauseTimer:
                        PauseTimerButton.onClick.AddListener(() => button.Action());
                        PauseTimerButton.gameObject.SetActive(true);
                        break;
                    case Timer.ButtonKind.ResumeTimer:
                        ResumeTimerButton.onClick.AddListener(() => button.Action());
                        ResumeTimerButton.gameObject.SetActive(true);
                        break;
                    case Timer.ButtonKind.ExtendPause:
                        ExtendPauseButton.onClick.AddListener(() => button.Action());
                        ExtendPauseButton.gameObject.SetActive(true);
                        break;
                    case Timer.ButtonKind.RefPauseTimer:
                        RefPauseTimerButton.onClick.AddListener(() => button.Action());
                        RefPauseTimerButton.gameObject.SetActive(true);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
    }
}