using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BuffKit.Util;

namespace BuffKit.LobbyTimer
{
    public class Timer : MonoBehaviour
    {
        private const int Interval = 30;
        private const int MainDuration = 210;
        private const int OvertimeDuration = 60;
        private const int LoadoutSetupDuration = 30;
        private const int PreLockAnnouncementTime = 30;
        private const int LockAnnouncementTime = 0;
        private const int PauseDuration = 120;

        private State _previousState;
        private State _currentState;
        
        private int _pausesLeft = 2;

        private Action _repaint;

        public int SecondsLeft = MainDuration;
        private int _prePauseSecondsLeft;
        
        public List<Button> Buttons;

        private static string FormatSeconds(int seconds)
        {
            var t = TimeSpan.FromSeconds(seconds);
            return $"{t.Minutes:D1}:{t.Seconds:D2}";
        }

        public void Act(Action repaintCallback)
        {
            _repaint = repaintCallback;
            Repaint();
        }

        private void Repaint()
        {
            Buttons = UpdateButtons();
            _repaint();
        }

        private void AddPauseButtons(List<Button> buttons)
        {
            buttons.Add( new Button {Kind = ButtonKind.PauseTimer, Action = delegate { Transition(State.Pause); }});
            buttons.Add(new Button {Kind = ButtonKind.RefPauseTimer, Action = delegate { Transition(State.RefPause); }});
        }

        private List<Button> UpdateButtons()
        {
            var buttons = new List<Button>();
            switch (_currentState)
            {
                case State.Startup:
                    buttons.Add(new Button
                    {
                        Kind = ButtonKind.StartTimer,
                        Action = delegate
                        {
                            Transition(State.Main);
                            StartCoroutine(Tick());
                        }
                    });
                    break;
                case State.Main:
                    AddPauseButtons(buttons);
                    break;
                case State.LoadoutSetup:
                    AddPauseButtons(buttons);
                    buttons.Add(
                        new Button {Kind = ButtonKind.StartTimer, Action = delegate { Transition(State.Overtime); }});
                    break;
                case State.LoadoutSetupEnd:
                    buttons.Add(
                        new Button {Kind = ButtonKind.StartTimer, Action = delegate { Transition(State.Overtime); }});
                    break;
                case State.Overtime:
                    AddPauseButtons(buttons);
                    break;
                case State.OvertimeLoadoutSetup:
                    AddPauseButtons(buttons);
                    break;
                case State.End:
                    break;
                case State.Pause:
                    buttons.Add(new Button {Kind = ButtonKind.StartTimer, Action = delegate { Transition(_previousState); }});
                    if (_pausesLeft > 0)
                    {
                        buttons.Add(new Button
                        {
                            Kind = ButtonKind.ExtendPause, Action = delegate
                            {
                                _pausesLeft--;
                                SecondsLeft += PauseDuration;
                                TrySendMessage(
                                    string.Format(TimerStrings.PauseExtended, FormatSeconds(SecondsLeft)));
                                Repaint();
                            }
                        });
                    }
                    else
                    {
                        buttons.Add(new Button
                        {
                            Kind = ButtonKind.RefPauseTimer, Action = delegate { Transition(State.RefPause);}
                        });
                    }

                    break;
                case State.RefPause:
                    buttons.Add(new Button {Kind = ButtonKind.StartTimer, Action = delegate
                    {
                        MuseLog.Info("RESUME TIMER CLICKED FROM REF PAUSE");
                        MuseLog.Info($"TRANSITIONING TO {_previousState}");
                        Transition(_previousState);
                    }});
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return buttons;
        }

        private void Awake()
        {
            _currentState = State.Startup;
        }

        private void Transition(State newState)
        {
            switch (newState)
            {
                //Startup -> Main, initial startup
                case State.Main when _currentState is State.Startup:
                    SecondsLeft = MainDuration;

                    TrySendMessage(string.Format(
                        TimerStrings.Startup,
                        FormatSeconds(SecondsLeft),
                        LoadoutSetupDuration.ToString()
                    ));
                    break;
                //Main -> LoadoutSetup
                case State.LoadoutSetup when _currentState is State.Main:
                    SecondsLeft = LoadoutSetupDuration;

                    TrySendMessage(string.Format(
                        TimerStrings.LoadoutSetupStart,
                        LoadoutSetupDuration.ToString()
                    ));
                    break;
                //LoadoutSetup -> LoadoutSetupEnd
                case State.LoadoutSetupEnd when _currentState is State.LoadoutSetup:
                    TrySendMessage(
                        TimerStrings.LoadoutSetupEnd);
                    break;
                //LoadoutSetupEnd -> Overtime
                case State.Overtime when _currentState is State.LoadoutSetupEnd:
                    SecondsLeft = OvertimeDuration;

                    TrySendMessage(string.Format(
                        TimerStrings.OvertimeStart,
                        FormatSeconds(SecondsLeft),
                        LoadoutSetupDuration.ToString()
                    ));
                    break;
                //Overtime -> OvertimeLoadoutSetup
                case State.OvertimeLoadoutSetup when _currentState is State.Overtime:
                    SecondsLeft = LoadoutSetupDuration;

                    TrySendMessage(string.Format(
                        TimerStrings.OvertimeLoadoutSetupStart,
                        LoadoutSetupDuration.ToString()
                    ));
                    break;
                //OvertimeLoadoutSetup -> End
                case State.End when _currentState == State.OvertimeLoadoutSetup:
                    TrySendMessage(
                        TimerStrings.OvertimeLoadoutSetupEnd);
                    break;
                //Any -> Pause
                case State.Pause:
                    _pausesLeft--;
                    _prePauseSecondsLeft = SecondsLeft;
                    SecondsLeft = PauseDuration;

                    TrySendMessage(
                        string.Format(TimerStrings.PauseStart,
                            FormatSeconds(_prePauseSecondsLeft),
                            FormatSeconds(PauseDuration)
                        ));
                    break;
                //Pause -> RefPause
                case State.RefPause when _currentState == State.Pause:
                    TrySendMessage(TimerStrings.RefPauseStart);
                    //Make sure we return to pre-pause state instead of pause
                    _currentState = newState;
                    Repaint();
                    return;
                //Any -> RefPause
                case State.RefPause:
                    _prePauseSecondsLeft = SecondsLeft;
                    TrySendMessage(TimerStrings.RefPauseStart);
                    break;
                default:
                {
                    if (_currentState == State.Pause || _currentState == State.RefPause)
                    {
                        SecondsLeft = _prePauseSecondsLeft;
                        TrySendMessage(
                            string.Format(TimerStrings.TimerResumed, FormatSeconds(SecondsLeft)));
                    }
                    break;
                }
            }
            
            MuseLog.Info($"SETTING PREVIOUS STATE TO {_currentState}");
            _previousState = _currentState;
            MuseLog.Info($"SETTING CURRENT STATE TO {newState}");
            _currentState = newState;
            
            Repaint();
        }


        private IEnumerator Tick()
        {
            while (SecondsLeft >= 0)
            {
                if (SecondsLeft > 0)
                {
                    switch (_currentState)
                    {
                        case State.Startup:
                            Transition(State.Main);
                            break;
                        //Don't announce anything during loadout setups
                        case State.LoadoutSetup:
                            break;
                        case State.LoadoutSetupEnd:
                            yield return new WaitUntil(() => _currentState != State.LoadoutSetupEnd);
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
                                TrySendMessage(
                                    string.Format(
                                        TimerStrings.PauseAnnouncement,
                                        FormatSeconds(SecondsLeft)));

                            break;
                        case State.RefPause:
                            yield return new WaitUntil(() => _currentState != State.RefPause);
                            break;
                        default:
                            if (SecondsLeft == PreLockAnnouncementTime)
                                TrySendMessage(
                                    string.Format(
                                        TimerStrings.PreLockAnnouncement,
                                        FormatSeconds(SecondsLeft),
                                        (PreLockAnnouncementTime - LockAnnouncementTime).ToString()
                                    ));
                            else if (SecondsLeft % Interval == 0)
                                TrySendMessage(
                                    string.Format(
                                        TimerStrings.TimerAnnouncement,
                                        FormatSeconds(SecondsLeft)));

                            break;
                    }

                    yield return new WaitForSeconds(1);
                    SecondsLeft--;
                    continue;
                }

                // Timer hits zero, what do?
                switch (_currentState)
                {
                    case State.Main:
                        Transition(State.LoadoutSetup);
                        continue;
                    case State.LoadoutSetup:
                        Transition(State.LoadoutSetupEnd);
                        continue;
                    case State.LoadoutSetupEnd:
                        yield return new WaitUntil(() => _currentState != State.LoadoutSetupEnd);
                        continue;
                    case State.Overtime:
                        Transition(State.OvertimeLoadoutSetup);
                        continue;
                    case State.OvertimeLoadoutSetup:
                        Transition(State.End);
                        continue;
                    case State.Pause:
                        SecondsLeft = _prePauseSecondsLeft;
                        Transition(_previousState);
                        continue;
                    case State.RefPause:
                        SecondsLeft = _prePauseSecondsLeft;
                        Transition(_previousState);
                        continue;
                    case State.End:
                        yield break;
                }
            }
        }

        private enum State
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
        public enum ButtonKind
        {
            StartTimer,
            PauseTimer,
            RefPauseTimer,
            ExtendPause,
        }

        public struct Button
        {
            public ButtonKind Kind;
            public Action Action;
        }
    }
}