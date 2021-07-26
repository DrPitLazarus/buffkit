using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BuffKit.Util;

namespace BuffKit
{
    public class LobbyTimer : MonoBehaviour
    {
        private const int Interval = 30;
        private const int MainDuration = 210;
        private const int OvertimeDuration = 60;
        private const int LoadoutSetupDuration = 30;
        private const int PreLockAnnouncementTime = 30;
        private const int LockAnnouncementTime = 0;
        private const int PauseDuration = 120;

        private State _currentState;
        private int _pausesLeft = 2;
        private int _prePauseSecondsLeft;
        private State _prePauseState;
        private Action _repaint;

        private int _secondsLeft = MainDuration;
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

        private List<Button> UpdateButtons()
        {
            var buttons = new List<Button>();
            switch (_currentState)
            {
                case State.Startup:
                    buttons.Add(new Button
                    {
                        Label = "START TIMER",
                        Action = delegate
                        {
                            Transition(State.Main);
                            StartCoroutine(Tick());
                        }
                    });
                    break;
                case State.Main:
                    if (_pausesLeft > 0)
                        buttons.Add(
                            new Button {Label = "PAUSE TIMER", Action = delegate { Transition(State.Pause); }}
                        );

                    break;
                case State.LoadoutSetup:
                    if (_pausesLeft > 0)
                        buttons.Add(
                            new Button {Label = "PAUSE TIMER", Action = delegate { Transition(State.Pause); }});
                    buttons.Add(
                        new Button {Label = "START OVERTIME", Action = delegate { Transition(State.Overtime); }});
                    break;
                case State.LoadoutSetupEnd:
                    buttons.Add(new Button {Label = "FORCE START", Action = LobbyActions.ForceStart});
                    buttons.Add(
                        new Button {Label = "START OVERTIME", Action = delegate { Transition(State.Overtime); }});
                    break;
                case State.Overtime:
                    if (_pausesLeft > 0)
                        buttons.Add(
                            new Button {Label = "PAUSE TIMER", Action = delegate { Transition(State.Pause); }});
                    break;
                case State.OvertimeLoadoutSetup:
                    if (_pausesLeft > 0)
                        buttons.Add(
                            new Button {Label = "PAUSE TIMER", Action = delegate { Transition(State.Pause); }});

                    buttons.Add(new Button {Label = "FORCE START", Action = LobbyActions.ForceStart});
                    break;
                case State.End:
                    buttons.Add(new Button {Label = "FORCE START", Action = LobbyActions.ForceStart});
                    break;
                case State.Pause:
                    buttons.Add(new Button {Label = "RESUME TIMER", Action = delegate { Transition(_prePauseState); }});
                    if (_pausesLeft > 0)
                        buttons.Add(new Button
                        {
                            Label = "EXTEND PAUSE", Action = delegate
                            {
                                _pausesLeft--;
                                _secondsLeft += PauseDuration;
                                TrySendMessage(
                                    string.Format(TimerStrings.PauseExtended, FormatSeconds(_secondsLeft)),
                                    "match");
                                Repaint();
                            }
                        });

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
            //Startup -> Main, initial startup
            if (_currentState is State.Startup && newState is State.Main)
            {
                _currentState = newState;
                _secondsLeft = MainDuration;

                TrySendMessage(string.Format(
                    TimerStrings.Startup,
                    FormatSeconds(_secondsLeft),
                    LoadoutSetupDuration.ToString()
                ), "match");
            }

            //Main -> LoadoutSetup
            if (_currentState is State.Main && newState is State.LoadoutSetup)
            {
                _currentState = newState;
                _secondsLeft = LoadoutSetupDuration;

                TrySendMessage(string.Format(
                    TimerStrings.LoadoutSetup,
                    LoadoutSetupDuration.ToString()
                ), "match");
            }

            //LoadoutSetup -> LoadoutSetupEnd
            if (_currentState is State.LoadoutSetup && newState is State.LoadoutSetupEnd)
            {
                _currentState = newState;
                TrySendMessage(
                    TimerStrings.LoadoutSetupEnd,
                    "match");
            }

            //LoadoutSetupEnd || LoadoutSetup -> Overtime
            if (_currentState is State.LoadoutSetupEnd ||
                _currentState is State.LoadoutSetup && newState is State.Overtime)
            {
                _currentState = newState;
                _secondsLeft = OvertimeDuration;

                TrySendMessage(string.Format(
                    TimerStrings.OvertimeStart,
                    FormatSeconds(_secondsLeft),
                    LoadoutSetupDuration.ToString()
                ), "match");
            }

            //Overtime -> OvertimeLoadoutSetup
            if (_currentState is State.Overtime && newState is State.OvertimeLoadoutSetup)
            {
                _currentState = newState;
                _secondsLeft = LoadoutSetupDuration;

                TrySendMessage(string.Format(
                    TimerStrings.OvertimeLoadoutSetup,
                    LoadoutSetupDuration.ToString()
                ), "match");
            }

            //OvertimeLoadoutSetup -> End
            if (_currentState == State.OvertimeLoadoutSetup && newState is State.End)
            {
                _currentState = newState;
                TrySendMessage(
                    TimerStrings.OvertimeLoadoutSetupEnd,
                    "match");
            }

            //Any -> Pause
            if (newState is State.Pause)
            {
                _prePauseState = _currentState;
                _currentState = newState;
                _pausesLeft--;
                _prePauseSecondsLeft = _secondsLeft;
                _secondsLeft = PauseDuration;

                TrySendMessage(
                    string.Format(TimerStrings.Pause,
                        FormatSeconds(_prePauseSecondsLeft),
                        FormatSeconds(PauseDuration)
                    ),
                    "match");
            }

            //Pause -> Any
            if (newState != State.Pause && _currentState == State.Pause)
            {
                _currentState = newState;
                _secondsLeft = _prePauseSecondsLeft;
                TrySendMessage(
                    string.Format(TimerStrings.TimeResumed, FormatSeconds(_secondsLeft)),
                    "match");
            }

            Repaint();
        }


        private IEnumerator Tick()
        {
            while (_secondsLeft >= 0)
            {
                if (_secondsLeft > 0)
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
                        case State.Main when _secondsLeft == MainDuration:
                            break;
                        case State.Overtime when _secondsLeft == OvertimeDuration:
                            break;
                        //Don't announce the remaining time when first entering pause.
                        case State.Pause when _secondsLeft == PauseDuration:
                            break;
                        case State.Pause:
                            if (_secondsLeft % Interval == 0)
                                TrySendMessage(
                                    string.Format(
                                        TimerStrings.PauseAnnouncement,
                                        FormatSeconds(_secondsLeft)),
                                    "match");

                            break;
                        default:
                            if (_secondsLeft == PreLockAnnouncementTime)
                                TrySendMessage(
                                    string.Format(
                                        TimerStrings.PreLockAnnouncement,
                                        FormatSeconds(_secondsLeft),
                                        (PreLockAnnouncementTime - LockAnnouncementTime).ToString()
                                    ),
                                    "match");
                            else if (_secondsLeft % Interval == 0)
                                TrySendMessage(
                                    string.Format(
                                        TimerStrings.TimerAnnouncement,
                                        FormatSeconds(_secondsLeft)),
                                    "match");

                            break;
                    }

                    yield return new WaitForSeconds(1);
                    _secondsLeft--;
                    continue;
                }

                // Timer hits zero, what do?
                MuseLog.Info(_currentState.ToString());
                MuseLog.Info("Entering switch");
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
                        _secondsLeft = _prePauseSecondsLeft;
                        Transition(_prePauseState);
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
            Pause
        }

        public struct Button
        {
            public string Label;
            public Action Action;
        }
    }
}