using System;
using System.Collections.Generic;
using UnityEngine;

namespace BuffKit.LobbyStateMachine
{
    public enum State
    {
        Startup,
        MainTimer,
        Overtime,
        ShipsLocked,
        TeamPause,
        RefPause,
        End
    }

    public enum Message
    {
        End,
        Extend,
        Pause,
        RefPause
    }

    public class Machine : MonoBehaviour
    {
        public LinkedList<State> History;
        public State PreviousState;
        public State CurrentState;

        public Dictionary<State, Action> OnEnterState;
        public Dictionary<State, Action> OnLeaveState;
        
        public int PausesLeft = 2;

        public void Awake()
        {
            CurrentState = State.Startup;
            History.AddFirst(State.Startup);
        }

        public void SendMessage(Message message)
        {
            switch (message)
            {
                case Message.End:
                    HandleEnd();
                    break;
                case Message.Pause:
                    HandlePause();
                    break;
                case Message.RefPause:
                    HandleRefPause();
                    break;
                case Message.Extend:
                    HandleExtend();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(message), message, null);
            }
        }

        private void HandleEnd()
        {
            OnLeaveState[CurrentState].Invoke();
            History.AddLast(CurrentState);

            switch (CurrentState)
            {
                case State.Startup:
                    CurrentState = State.MainTimer;
                    break;
                case State.MainTimer:
                    CurrentState = State.ShipsLocked;
                    break;
                case State.Overtime:
                    CurrentState = State.ShipsLocked;
                    break;
                case State.ShipsLocked:
                    CurrentState = State.End;
                    break;
                case State.TeamPause:
                    CurrentState = PreviousState;
                    break;
                case State.RefPause when PreviousState == State.TeamPause:
                    //Can't be null because Startup -> RefPause is not a valid transition
                    CurrentState = History.Last.Previous.Value;
                    break;
                case State.RefPause:
                    CurrentState = PreviousState;
                    break;
                case State.End:
                    break;
            }

            PreviousState = CurrentState;
            OnEnterState[CurrentState].Invoke();
        }

        private void HandlePause()
        {
            switch (CurrentState)
            {
                //Can't pause from these three
                case State.Startup:
                    break;
                case State.RefPause:
                    break;
                case State.End:
                    break;
                default:
                    if (PausesLeft > 0)
                    {
                        PausesLeft--;
                        OnLeaveState[CurrentState].Invoke();
                        History.AddLast(CurrentState);
                    
                        PreviousState = History.Last.Value;
                        CurrentState = State.TeamPause;
                    
                        OnEnterState[CurrentState].Invoke();
                    }
                    break;
            }
        }

        private void HandleRefPause()
        {
            switch (CurrentState)
            {
                //Can't ref pause from these, oopsie
                case State.Startup:
                    break;
                case State.RefPause:
                    break;
                case State.End:
                    break;
                default:
                    OnLeaveState[CurrentState].Invoke();
                    History.AddLast(CurrentState);
                    
                    PreviousState = History.Last.Value;
                    CurrentState = State.RefPause;
                    
                    OnEnterState[CurrentState].Invoke();
                    break;
            }
        }

        private void HandleExtend()
        {
            switch (CurrentState)
            {
                case State.TeamPause:
                    if (PausesLeft > 0)
                    {
                        PausesLeft--;
                    }
                    break;
                case State.ShipsLocked when PreviousState == State.MainTimer:
                    OnLeaveState[CurrentState].Invoke();
                    History.AddLast(CurrentState);
                    
                    PreviousState = History.Last.Value;
                    CurrentState = State.Overtime;
                    
                    OnEnterState[CurrentState].Invoke();
                    break;
            }
        }
    }
}