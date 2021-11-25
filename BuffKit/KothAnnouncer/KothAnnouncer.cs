namespace BuffKit.KothAnnouncer
{
    public class KothAnnouncer
    {
        public static KothAnnouncer Instance;

        private CrazyKing _currentMatch;
        private int _previousTeam = -1;
        private bool _enabled = true;

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
        }

        public void OnMatchUpdate(CrazyKing match)
        {
            if (!_enabled) return;
            
            if (match != _currentMatch)
            {
                OnMatchInitialize(match);
            }

            for (int i = 0; i < match.objectives.Length; i++)
            {
                var objective = match.objectives[i];
                if (objective.active)
                {
                    if (objective.controllingTeam != _previousTeam)
                    {
                        Util.ForceSendMessage(
                            $"[REF] {Util.GetTeamName(objective.controllingTeam).ToUpper()} captured the point! {(int)match.resourcesGathered[objective.controllingTeam].resources[0].amount}/{(int)match.resourceGoals[0].amount}");
                        _previousTeam = objective.controllingTeam;
                    }
                    return;
                }
            }
        }
    }
}
