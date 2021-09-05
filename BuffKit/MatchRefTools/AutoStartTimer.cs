namespace BuffKit.MatchRefTools
{
    public static class AutoStartTimer
    {
        private static bool _isEnabled = false;
        public static void SetEnabled(bool isEnabled) { _isEnabled = isEnabled; }
        public static void TryStartTimer()
        {
            var log = BepInEx.Logging.Logger.CreateLogSource("autostarttimer");
            if (!_isEnabled)
            {
                log.LogInfo("Did not start timer - setting not active");
                return;
            }
            log.LogInfo("Attempting to start match timer");
            // TODO: check if timer is already running (find timer display, see how it is updated) - maybe not necessary?
            if (Util.Util.HasModPrivilege(MatchLobbyView.Instance))
            {
                MatchActions.StartCountdown(20 * 60);
                Util.Util.ForceSendMessage("REF: TIMER STARTED");
                log.LogInfo("Called StartCountdown");
            }
            else
            {
                log.LogInfo("Did not start countdown - invalid permissions in lobby");
            }
        }
    }
}
