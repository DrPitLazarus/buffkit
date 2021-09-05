using System;
using System.Collections.Generic;
using System.Linq;
using static BuffKit.Util.Util;

namespace BuffKit.MatchModMenu
{
    public class UIModMenuState : UIManager.UINewHeaderState
    {
        public const int TimerDuration = 1200;
        public const int OvertimeDuration = 180;
        public static UIModMenuState Instance = new UIModMenuState();
        private bool _needRepaint;
        private bool _mainTimerStarted = false;
        public override UIManager.UIState BackState => UIManager.UIMatchMenuState.instance.BackState;

        public override void Enter(UIManager.UIState previous, UIManager.UIContext uiContext)
        {
            base.Enter(previous, uiContext);
            UIMatchManager.Deactivate();
            UITutorialManager.Deactivate();
            UITutorialManager.GameMode = false;
            UIPageFrame.Instance.ShowLobbyChat();
            PaintMenu(BackState);
        }

        public override void Update(UIManager.UIContext uiContext)
        {
            base.Update(uiContext);
            if (_needRepaint)
            {
                PaintMenu(BackState);
                _needRepaint = false;
            }
        }

        public override void Exit(UIManager.UIState next, UIManager.UIContext uiContext)
        {
            UIScoreboard.Activated = false;
            UIPageFrame.Instance.header.Deactivate();
            base.Exit(next, uiContext);
        }

        public override void UIEventExit()
        {
            if (UIScoreboard.Activated)
                UIScoreboard.Activated = false;
            else
                base.UIEventExit();
        }


        private void PaintMenu(UIManager.UIState state)
        {
            var mlv = MatchLobbyView.Instance;
            var msv = MatchStateView.Instance;

            var dm = UIPageFrame.Instance.navigationMenu;
            dm.Clear();

            var actions = (from action in MatchModAction.RunningMatchActions
                where action.CanAct(NetworkedPlayer.Local, mlv)
                select action).ToList();

            foreach (var action in actions)
                if (action is TimerModAction)
                {
                    if (msv.ModCountdown > 0.0)
                    {
                        if (msv.ModCountdownSwitch)
                            dm.AddButton("Pause timer", string.Empty, UIMenuItem.Size.Small, false, false, delegate
                            {
                                _needRepaint = true;
                                MatchActions.PauseCountdown();
                                ForceSendMessage("REF: GAME PAUSED");
                                UIManager.TransitionToState(state);
                            });
                        else
                            dm.AddButton("Resume timer", string.Empty, UIMenuItem.Size.Small, false, false, delegate
                            {
                                _needRepaint = true;
                                MatchActions.ExtendCountdown(0);
                                ForceSendMessage("REF: GAME RESUMED");
                                UIManager.TransitionToState(state);
                            });

                        dm.AddButton("Stop the timer", string.Empty, UIMenuItem.Size.Small, false, false,
                            delegate
                            {
                                _needRepaint = true;
                                MatchActions.StopCountdown();
                                UIManager.TransitionToState(state);
                            });
                    }
                    else
                    {
                        if (!_mainTimerStarted)
                        {
                            dm.AddButton("Start timer (20 minutes)", string.Empty, UIMenuItem.Size.Small, false, false,
                                delegate
                                {
                                    _needRepaint = true;
                                    _mainTimerStarted = true;
                                    MatchActions.StartCountdown(TimerDuration);
                                    ForceSendMessage("REF: TIMER STARTED");
                                    UIManager.TransitionToState(state);
                                });
                        }
                        else if (_mainTimerStarted)
                        {
                            dm.AddButton("Start overtime (3 minutes)", string.Empty, UIMenuItem.Size.Small, false,
                                false,
                                delegate
                                {
                                    _needRepaint = true;
                                    MatchActions.StartCountdown(OvertimeDuration);
                                    ForceSendMessage("REF: OVERTIME STARTED");
                                    UIManager.TransitionToState(state);
                                });
                        }
                    }
                    
                    dm.AddButton("Custom timer", string.Empty, UIMenuItem.Size.Small, false, false,
                        delegate
                        {
                            _needRepaint = true;
                            var options = new List<int>
                            {
                                10,
                                30,
                                60,
                                120,
                                180,
                                300,
                                480,
                                600,
                                720,
                                900,
                                1200
                            };
                            
                            UINewModalDialog.Select("Select duration", string.Empty, UINewModalDialog.DropdownSetting.CreateSetting(options, t => TimeSpan.FromSeconds(t).ToString()), index =>
                            {
                                if (index < 0) return;

                                if (msv.ModCountdown > 0.0)
                                {
                                    MatchActions.ExtendCountdown(options[index]);
                                    ForceSendMessage("REF: TIMER EXTENDED");
                                }
                                else
                                {
                                    MatchActions.StartCountdown(options[index]);
                                    ForceSendMessage("REF: TIMER STARTED");
                                }
                                
                                UIManager.TransitionToState(state);
                            });
                        });
                }
                else
                {
                    dm.AddButton(action.Name(mlv), string.Empty, UIMenuItem.Size.Small, false, false,
                        delegate { action.Act(mlv); });
                }
        }
    }
}