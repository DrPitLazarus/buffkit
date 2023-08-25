using System;
using System.Collections;
using System.Text;
using UnityEngine;
using static BuffKit.Util;

namespace BuffKit.LobbyTimer
{
    public class Timer : MonoBehaviour
    {
        private const int Interval = 30;
        private const int MainDuration = 210;
        private const int OvertimeDuration = 60;
        private const int LoadoutSetupDuration = 60;
        private const int PreLockAnnouncementTime = 30;
        private const int LockAnnouncementTime = 0;
        private const int PauseDuration = 120;

        public State PreviousState;
        public State CurrentState = State.Startup;
        
        public int PausesLeft = 2;
        
        public int SecondsLeft = MainDuration;
        public int PrePauseSecondsLeft;
        public string MatchId;

        public int RedLastChange = MainDuration;
        public int BlueLastChange = MainDuration;

        public bool IsActive => CurrentState == State.Main ||
                                CurrentState == State.Overtime ||
                                CurrentState == State.LoadoutSetup ||
                                CurrentState == State.OvertimeLoadoutSetup ||
                                CurrentState == State.LoadoutSetupEnd ||
                                CurrentState == State.End;

        private TimerButtonContainer _tbc;
        

        private static string FormatSeconds(int seconds)
        {
            var t = TimeSpan.FromSeconds(seconds);
            return $"{t.Minutes:D1}:{t.Seconds:D2}";
        }

        private void UpdatePauseButton()
        {
            _tbc.PauseButton.interactable = PausesLeft > 0;
        }

        private void Repaint()
        {
            switch (CurrentState)
            {
                case State.Startup:
                    _tbc.SetStatus("");
                    _tbc.SetCountdown("-:--");
                        
                    _tbc.StartButton.interactable = true;
                    _tbc.StartButton.onClick.RemoveAllListeners();
                    _tbc.StartButton.onClick.AddListener(() => Transition(State.Main));
                    _tbc.PauseButton.interactable = false;
                    _tbc.PauseButton.onClick.RemoveAllListeners();
                    _tbc.PauseButton.onClick.AddListener(() => Transition(State.Pause));
                    _tbc.ExtendButton.interactable = false;
                    _tbc.ExtendButton.onClick.RemoveAllListeners();
                    _tbc.RefPauseButton.interactable = false;
                    _tbc.RefPauseButton.onClick.RemoveAllListeners();
                    _tbc.RefPauseButton.onClick.AddListener(() => Transition(State.RefPause));
                    _tbc.ResetButton.interactable = false;
                    _tbc.ResetButton.onClick.RemoveAllListeners();
                    _tbc.ResetButton.onClick.AddListener(() => Transition(State.Startup));
                    break;
                
                case State.Main:
                    _tbc.SetStatus("PICKING");
                    _tbc.SetCountdown(FormatSeconds(SecondsLeft));
                        
                    _tbc.StartButton.interactable = false;
                    UpdatePauseButton();
                    _tbc.ExtendButton.interactable = false;
                    _tbc.RefPauseButton.interactable = true;
                    _tbc.ResetButton.interactable = true;
                    break;
                
                case State.LoadoutSetup:
                    _tbc.SetStatus("LOCKED");
                    _tbc.SetCountdown(FormatSeconds(SecondsLeft));
                    
                    _tbc.StartButton.interactable = false;
                    UpdatePauseButton();
                    _tbc.ExtendButton.interactable = true;
                    _tbc.ExtendButton.onClick.RemoveAllListeners();
                    _tbc.ExtendButton.onClick.AddListener(() => Transition(State.Overtime));
                    _tbc.ExtendButton.SetHoverTooltip("Start overtime");
                    _tbc.RefPauseButton.interactable = true;
                    _tbc.ResetButton.interactable = true;
                    break;
                
                case State.LoadoutSetupEnd:
                    _tbc.SetStatus("AWAITING REF DECISION");
                    _tbc.SetCountdown("-:--");
                    
                    _tbc.StartButton.interactable = false;
                    _tbc.PauseButton.interactable = false;
                    _tbc.ExtendButton.interactable = true;
                    _tbc.ExtendButton.onClick.RemoveAllListeners();
                    _tbc.ExtendButton.onClick.AddListener(() => Transition(State.Overtime));
                    _tbc.ExtendButton.SetHoverTooltip("Start overtime");
                    _tbc.RefPauseButton.interactable = false;
                    _tbc.ResetButton.interactable = true;
                    break;
                
                case State.Overtime:
                    _tbc.SetStatus("OVERTIME");
                    _tbc.SetCountdown(FormatSeconds(SecondsLeft));
                    
                    _tbc.StartButton.interactable = false;
                    UpdatePauseButton();
                    _tbc.ExtendButton.interactable = false;
                    _tbc.RefPauseButton.interactable = true;
                    _tbc.ResetButton.interactable = true;
                    break;
                
                case State.OvertimeLoadoutSetup:
                    _tbc.SetStatus("LOCKED");
                    _tbc.SetCountdown(FormatSeconds(SecondsLeft));
                    
                    _tbc.StartButton.interactable = false;
                    UpdatePauseButton();
                    _tbc.ExtendButton.interactable = false;
                    _tbc.RefPauseButton.interactable = true;
                    _tbc.ResetButton.interactable = true;
                    break;
                
                case State.End:
                    _tbc.SetStatus("STARTING MATCH");
                    _tbc.SetCountdown("-:--");
                    
                    _tbc.StartButton.interactable = false;
                    _tbc.PauseButton.interactable = false;
                    _tbc.ExtendButton.interactable = false;
                    _tbc.RefPauseButton.interactable = false;
                    _tbc.ResetButton.interactable = true;
                    break;
                
                case State.Pause:
                    _tbc.StartButton.interactable = true;
                    _tbc.StartButton.onClick.RemoveAllListeners();
                    _tbc.StartButton.onClick.AddListener(() => Transition(PreviousState));
                    _tbc.PauseButton.interactable = false;
                    _tbc.ExtendButton.interactable = PausesLeft > 0;
                    _tbc.ExtendButton.SetHoverTooltip("Extend pause");
                    _tbc.RefPauseButton.interactable = true;
                    _tbc.ResetButton.interactable = true;
                    
                    if (PausesLeft > 0)
                    {
                        _tbc.ExtendButton.onClick.RemoveAllListeners();
                        _tbc.ExtendButton.onClick.AddListener(() =>
                        {
                            PausesLeft--;
                            SecondsLeft += PauseDuration;
                            ForceSendMessage(
                                string.Format(TimerStrings.PauseExtended, FormatSeconds(SecondsLeft)));
                            Repaint();
                        });
                    }
                    
                    _tbc.SetStatus("PAUSED");
                    break;
                
                case State.RefPause:
                    _tbc.StartButton.interactable = true;
                    _tbc.StartButton.onClick.RemoveAllListeners();
                    _tbc.StartButton.onClick.AddListener(() => Transition(PreviousState));
                    _tbc.PauseButton.interactable = false;
                    _tbc.ExtendButton.interactable = false;
                    _tbc.RefPauseButton.interactable = false;
                    _tbc.ResetButton.interactable = true;
                    
                    _tbc.SetStatus("PAUSED INDEFINITELY");
                    _tbc.SetCountdown("-:--");
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Awake()
        {
            CurrentState = State.Startup;
        }

        public void Initialize(TimerButtonContainer tbc)
        {
            _tbc = tbc;
            Repaint();
        }

        private void Transition(State newState)
        {
            switch (newState)
            {
                //Any -> Startup, used for reset
                case State.Startup:
                    StopAllCoroutines();
                    Repaint();
                    ForceSendMessage(TimerStrings.TimerReset);
                    break;
                //Startup -> Main, initial startup
                case State.Main when CurrentState is State.Startup:
                    SecondsLeft = MainDuration;
                    StartCoroutine(Tick());

                    ForceSendMessage(string.Format(
                        TimerStrings.Startup,
                        FormatSeconds(SecondsLeft),
                        LoadoutSetupDuration.ToString()
                    ));
                    break;
                //Main -> LoadoutSetup
                case State.LoadoutSetup when CurrentState is State.Main:
                    SecondsLeft = LoadoutSetupDuration;
                    
                    ForceSendMessage(string.Format(
                        TimerStrings.LoadoutSetupStart,
                        LoadoutSetupDuration.ToString()
                    ));

                    string teams = "";
                    if (RedLastChange < PreLockAnnouncementTime 
                        && BlueLastChange < PreLockAnnouncementTime)
                    {
                        teams = "BOTH TEAMS";
                    } else if (RedLastChange < PreLockAnnouncementTime)
                    {
                        teams = "BLUE";
                    } else if (BlueLastChange < PreLockAnnouncementTime)
                    {
                        teams = "RED";
                    }
                    else
                    {
                        teams = "NO TEAM";
                    }

                    if (teams.Length > 0)
                    {
                        ForceSendMessage(string.Format(
                            TimerStrings.OvertimeTeamAnnouncement,
                            RedLastChange.ToString(),
                            BlueLastChange.ToString(),
                            teams
                        ));
                    }

                    break;
                //LoadoutSetup -> LoadoutSetupEnd
                case State.LoadoutSetupEnd when CurrentState is State.LoadoutSetup:
                    ForceSendMessage(
                        TimerStrings.LoadoutSetupEnd);
                    break;
                //LoadoutSetup, LoadoutSetupEnd -> Overtime
                case State.Overtime when CurrentState is State.LoadoutSetup:
                case State.Overtime when CurrentState is State.LoadoutSetupEnd:
                    SecondsLeft = OvertimeDuration;

                    ForceSendMessage(string.Format(
                        TimerStrings.OvertimeStart,
                        FormatSeconds(SecondsLeft),
                        LoadoutSetupDuration.ToString()
                    ));
                    break;
                //Overtime -> OvertimeLoadoutSetup
                case State.OvertimeLoadoutSetup when CurrentState is State.Overtime:
                    SecondsLeft = LoadoutSetupDuration;

                    ForceSendMessage(string.Format(
                        TimerStrings.OvertimeLoadoutSetupStart,
                        LoadoutSetupDuration.ToString()
                    ));
                    break;
                //OvertimeLoadoutSetup -> End
                case State.End when CurrentState == State.OvertimeLoadoutSetup:
                    ForceSendMessage(
                        TimerStrings.OvertimeLoadoutSetupEnd);
                    break;
                //Any -> Pause
                case State.Pause:
                    PausesLeft--;
                    PrePauseSecondsLeft = SecondsLeft;
                    SecondsLeft = PauseDuration;
                    
                    _tbc.StartButton.onClick.RemoveAllListeners();
                    _tbc.StartButton.onClick.AddListener(() => Transition(PreviousState));

                    ForceSendMessage(
                        string.Format(TimerStrings.PauseStart,
                            FormatSeconds(PrePauseSecondsLeft),
                            FormatSeconds(PauseDuration)
                        ));
                    break;
                //Pause -> RefPause
                case State.RefPause when CurrentState == State.Pause:
                    ForceSendMessage(TimerStrings.RefPauseStart);
                    //Make sure we return to pre-pause state instead of pause
                    CurrentState = newState;
                    
                    _tbc.ExtendButton.onClick.RemoveAllListeners();
                    Repaint();
                    return;
                //Any -> RefPause
                case State.RefPause:
                    PrePauseSecondsLeft = SecondsLeft;
                    
                    _tbc.StartButton.onClick.RemoveAllListeners();
                    _tbc.StartButton.onClick.AddListener(() => Transition(PreviousState));
                    
                    ForceSendMessage(TimerStrings.RefPauseStart);
                    break;
                default:
                {
                    if (CurrentState == State.Pause || CurrentState == State.RefPause)
                    {
                        SecondsLeft = PrePauseSecondsLeft;
                        _tbc.StartButton.onClick.RemoveAllListeners();
                        _tbc.StartButton.onClick.AddListener(() => Transition(PreviousState));
                        
                        ForceSendMessage(
                            string.Format(TimerStrings.TimerResumed, FormatSeconds(SecondsLeft)));
                    }
                    break;
                }
            }
            
            MuseLog.Info($"SETTING PREVIOUS STATE TO {CurrentState}");
            PreviousState = CurrentState;
            MuseLog.Info($"SETTING CURRENT STATE TO {newState}");
            CurrentState = newState;

            Repaint();
        }
        
        private IEnumerator Tick()
        {
            while (SecondsLeft >= 0)
            {
                if (SecondsLeft > 0)
                {
                    switch (CurrentState)
                    {
                        case State.Startup:
                            break;
                        //Don't announce anything during loadout setups
                        case State.LoadoutSetup:
                            break;
                        case State.LoadoutSetupEnd:
                            yield return new WaitUntil(() => CurrentState != State.LoadoutSetupEnd);
                            break;
                        case State.OvertimeLoadoutSetup:
                            break;
                        case State.End:
                            break;
                        //Don't announce the remaining time when first entering a state
                        case State.Main when SecondsLeft == MainDuration:
                            break;
                        case State.Overtime when SecondsLeft == OvertimeDuration:
                            break;
                        //Don't announce the remaining time when first entering pause.
                        case State.Pause when SecondsLeft == PauseDuration:
                            break;
                        case State.Pause:
                            if (SecondsLeft % Interval == 0)
                                ForceSendMessage(
                                    string.Format(
                                        TimerStrings.PauseAnnouncement,
                                        FormatSeconds(SecondsLeft)));

                            break;
                        case State.RefPause:
                            yield return new WaitUntil(() => CurrentState != State.RefPause);
                            break;
                        default:
                            if (SecondsLeft == PreLockAnnouncementTime)
                                ForceSendMessage(
                                    string.Format(
                                        TimerStrings.PreLockAnnouncement,
                                        FormatSeconds(SecondsLeft)
                                        ));
                            else if (SecondsLeft % Interval == 0)
                                ForceSendMessage(
                                    string.Format(
                                        TimerStrings.TimerAnnouncement,
                                        FormatSeconds(SecondsLeft)));

                            break;
                    }

                    _tbc.SetCountdown(FormatSeconds(SecondsLeft));
                    yield return new WaitForSeconds(1);
                    SecondsLeft--;
                    continue;
                }

                // Timer hits zero, what do?
                switch (CurrentState)
                {
                    case State.Main:
                        Transition(State.LoadoutSetup);
                        continue;
                    case State.LoadoutSetup:
                        Transition(State.LoadoutSetupEnd);
                        continue;
                    case State.LoadoutSetupEnd:
                        yield return new WaitUntil(() => CurrentState != State.LoadoutSetupEnd);
                        continue;
                    case State.Overtime:
                        Transition(State.OvertimeLoadoutSetup);
                        continue;
                    case State.OvertimeLoadoutSetup:
                        Transition(State.End);
                        continue;
                    case State.Pause:
                        SecondsLeft = PrePauseSecondsLeft;
                        Transition(PreviousState);
                        continue;
                    case State.RefPause:
                        SecondsLeft = PrePauseSecondsLeft;
                        Transition(PreviousState);
                        continue;
                    case State.End:
                        yield break;
                }
            }
        }

        public enum State
        {
            Startup,
            Main,
            LoadoutSetup,
            LoadoutSetupEnd,
            Overtime,
            OvertimeLoadoutSetup,
            End,
            Pause,
            RefPause
        }
    }
}