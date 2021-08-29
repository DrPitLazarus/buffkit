namespace BuffKit.FirstKillAnnouncement
{
    public class FirstKillAnnouncement
    {

        public static FirstKillAnnouncement Instance;

        private BepInEx.Logging.ManualLogSource log;
        private Deathmatch _currentMatch;
        private bool _canAnnounce = false;
        private FirstKillAnnouncement()
        {
            log = BepInEx.Logging.Logger.CreateLogSource("fka");
        }

        public void OnMatchInitialize(Deathmatch match)
        {
            log.LogInfo("OnMatchInitialize");
            _currentMatch = match;

            var counter = 0;
            foreach (var v in match.Frags)
                counter += v;
            _canAnnounce = counter == 0;
        }
        public void OnMatchUpdate(Deathmatch match)
        {
            if (match == _currentMatch)
            {
                if (!_canAnnounce) return;

                for (var i = 0; i < match.numberOfTeams; i++)
                    if (match.Frags[i] > 0)
                    {
                        if (Util.Util.HasModPrivilege(MatchLobbyView.Instance) && _enabled)
                        {
                            log.LogInfo("Announcing first kill");
                            Util.Util.ForceSendMessage($"REF: FIRST KILL {Util.Util.GetTeamName(i).ToUpper()}");
                        }
                        _canAnnounce = false;
                    }
            }
            else
            {
                log.LogInfo("OnMatchUpdate with different match than expected, re-initializing");
                OnMatchInitialize(match);
            }
        }

        private bool _enabled = false;
        public void SetEnabled(bool enable)
        {
            _enabled = enable;
        }

        public static void Initialize()
        {
            Instance = new FirstKillAnnouncement();
        }

    }
}
