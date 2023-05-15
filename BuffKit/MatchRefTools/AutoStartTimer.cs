using BuffKit.MatchModMenu;

namespace BuffKit.MatchRefTools
{
    public static class AutoStartTimer
    {
        private static bool _isEnabled = false;
        public static void SetEnabled(bool isEnabled) { _isEnabled = isEnabled; }
        public static void TryStartTimer()
        {
            if (!_isEnabled)
            {
                MuseLog.Info("Did not start timer - setting not active");
                return;
            }
            MuseLog.Info("Attempting to start match timer");
            // TODO: check if timer is already running (find timer display, see how it is updated) - maybe not necessary?
            if (Util.HasModPrivilege(MatchLobbyView.Instance))
            {
                MatchActions.StartCountdown(25 * 60);
                Util.ForceSendMessage("REF: TIMER STARTED");
                MuseLog.Info("Called StartCountdown");
                UIModMenuState.Instance.MainTimerStarted = true;
            }
            else
            {
                MuseLog.Info("Did not start countdown - invalid permissions in lobby");
            }
        }
    }
}
