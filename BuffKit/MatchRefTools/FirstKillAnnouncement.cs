using System.Text;

namespace BuffKit.MatchRefTools
{
    public class FirstKillAnnouncement
    {

        public static FirstKillAnnouncement Instance;

        private Deathmatch _currentMatch;
        private int _total_kills = 0;
        private int _team_count = 0;

        public void OnMatchInitialize(Deathmatch match)
        {
            MuseLog.Info("OnMatchInitialize");
            _currentMatch = match;

            var counter = 0;
            foreach (var v in match.Frags)
                counter += v;
            _total_kills = counter;
            _team_count = match.numberOfTeams;
        }
        public void OnMatchUpdate(Deathmatch match)
        {
            if (match == _currentMatch)
            {
                var counter = 0;
                foreach (var v in match.Frags)
                {
                    counter += v;
                }

                var match_scores = match.Frags;

                if (_total_kills != counter)
                {
                    if (Util.HasModPrivilege(MatchLobbyView.Instance) && _enabled)
                    {
                        var sb = new StringBuilder();
                        sb.Append("REF: ");
                        for (var i = 0; i < _team_count; i++)
                        {
                            if (i > 0)
                                sb.Append(", ");
                            sb.Append($"{Util.GetTeamName(i).ToUpper()} {match_scores[i]}");
                        }
                        MuseLog.Info("Announcing updated points");
                        Util.ForceSendMessage(sb.ToString());
                    }
                    _total_kills = counter;
                }
            }
            else
            {
                MuseLog.Info("OnMatchUpdate with different match than expected, re-initializing");
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
