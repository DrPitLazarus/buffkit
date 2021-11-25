using System;
using System.Collections.Generic;

namespace BuffKit.KothAnnouncer
{
    public class KothAnnouncer
    {
        public static KothAnnouncer Instance;

        private CrazyKing _currentMatch;
        private int _previousTeam = -1;
        private bool _enabled = true;
        private Dictionary<int, List<int>> _milestones = new Dictionary<int, List<int>>();

        public static void Initialize()
        {
            Instance = new KothAnnouncer();
        }

        public void SetEnabled(bool v)
        {
            _enabled = v;
        }

        public void OnMatchInitialize(CrazyKing match)
        {
            MuseLog.Info("KOTH Announcer Initialized");
            
            _currentMatch = match;
            _previousTeam = -1;
            _milestones.Clear();
            
            foreach (var team in _milestones)
            {
                for (int i = 1; i <= Math.Floor(_currentMatch.resourceGoals[0].amount / 100); i++)
                {
                    team.Value.Add(i * 100);
                }
            }
        }

        public void OnMatchUpdate(CrazyKing match)
        {
            if (!_enabled) return;
            
            if (match != _currentMatch)
            {
                OnMatchInitialize(match);
            }

            foreach (var objective in match.objectives)
            {
                if (!objective.active || objective.controllingTeam == _previousTeam) continue;
                
                Util.ForceSendMessage(
                    $"REF: {Util.GetTeamName(objective.controllingTeam).ToUpper()} captured the point! {(int)match.resourcesGathered[objective.controllingTeam].resources[0].amount}/{(int)match.resourceGoals[0].amount}");
                _previousTeam = objective.controllingTeam;
                return;
            }

            for (var i = 0; i < match.numberOfTeams; i++)
            {
                var amount = (int)match.resourcesGathered[i].resources[0].amount;
                if (amount != _milestones[i][0]) continue;
                
                Util.ForceSendMessage($"REF: {Util.GetTeamName(i)} reached {_milestones[i][0].ToString()} points!");
                _milestones[i].RemoveAt(0);
            }
        }
    }
}
