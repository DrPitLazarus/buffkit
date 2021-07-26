﻿using System.Linq;
using MuseBase.Multiplayer.Unity;
using static BuffKit.Util;

namespace BuffKit
{
    public class UIModMenuState : UIManager.UINewHeaderState
    {
        public const int TimerDuration = 1200;
        public const int OvertimeDuration = 180;
        public static UIModMenuState Instance = new UIModMenuState();
        private bool _needRepaint;
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
                                MuseWorldClient.Instance.ChatHandler.TrySendMessage("REF: GAME PAUSED", "match");
                                UIManager.TransitionToState(state);
                            });
                        else
                            dm.AddButton("Resume timer", string.Empty, UIMenuItem.Size.Small, false, false, delegate
                            {
                                _needRepaint = true;
                                MatchActions.ExtendCountdown(0);
                                TrySendMessage("REF: GAME RESTARTED", "match");
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
                        dm.AddButton("Start the timer", string.Empty, UIMenuItem.Size.Small, false, false,
                            delegate
                            {
                                _needRepaint = true;
                                MatchActions.StartCountdown(TimerDuration);
                                MuseWorldClient.Instance.ChatHandler.TrySendMessage("REF: TIMER STARTED", "match");
                                UIManager.TransitionToState(state);
                            });
                        dm.AddButton("Start overtime", string.Empty, UIMenuItem.Size.Small, false, false,
                            delegate
                            {
                                _needRepaint = true;
                                MatchActions.StartCountdown(OvertimeDuration);
                                MuseWorldClient.Instance.ChatHandler.TrySendMessage("REF: OVERTIME STARTED", "match");
                                UIManager.TransitionToState(state);
                            });
                    }
                }
                else
                {
                    dm.AddButton(action.Name(mlv), string.Empty, UIMenuItem.Size.Small, false, false,
                        delegate { action.Act(mlv); });
                }
        }
    }
}