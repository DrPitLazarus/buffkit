using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;

namespace BuffKit.MatchRefTools
{
    static class Patcher
    {
        private static GameObject _obToggle;
        private static Toggle _toggle;
        private static bool _enableFKA = false;
        private static bool _enableAutoTimer = false;

        public static void Init()
        {
            var log = BepInEx.Logging.Logger.CreateLogSource("patch");
            log.LogInfo("In patch initialize");

            _obToggle = UI.Builder.BuildMenuToggle(
                GameObject.Find("/Menu UI/Standard Canvas/Pages/UI Match Lobby/Lobby Main Panel/Team Group/Button Group").transform,
                out _toggle, "Auto ref match ", false, OnToggleChange);

            _obToggle.SetActive(false);

            FirstKillAnnouncement.Initialize();
            Settings.Settings.Instance.AddEntry("ref tools", "announce first kill", delegate (bool v)
            {
                _enableFKA = v;
                FirstKillAnnouncement.Instance.SetEnabled(_enableFKA && _toggle.isOn);
            }, _enableFKA);

            MatchBlockerView.Done += AutoStartTimer.TryStartTimer;
            Settings.Settings.Instance.AddEntry("ref tools", "auto start timer", delegate (bool v)
            {
                _enableAutoTimer = v;
                AutoStartTimer.SetEnabled(_enableAutoTimer && _toggle.isOn);
            }, _enableAutoTimer);

            MatchLobbyView.enterMatchLobby += delegate
            {
                var mlv = MatchLobbyView.Instance;
                //if (mlv != null)
                //{
                //    log.LogInfo($"In enterMatchLobby - running: {mlv.Running}, started: {mlv.Started}, loading: {mlv.Loading}");
                //}
                if (mlv != null && !mlv.Started)
                    _toggle.isOn = false;
                _obToggle.SetActive(mlv != null && !mlv.Running && Util.Util.HasModPrivilege(mlv));
            };
        }

        private static void OnToggleChange(bool v)
        {
            FirstKillAnnouncement.Instance.SetEnabled(_enableFKA && v);
            AutoStartTimer.SetEnabled(_enableAutoTimer && v);
            var log = BepInEx.Logging.Logger.CreateLogSource("reftools");
            log.LogInfo($"Ref auto tools callback called with {v}");
        }
    }
    [HarmonyPatch(typeof(Deathmatch), "OnRemoteUpdate")]
    class Deathmatch_OnRemoteUpdate
    {
        private static bool _firstPrepare = true;
        private static void Prepare()
        {
            if (_firstPrepare)
            {
                Util.Util.OnGameInitialize += Patcher.Init;
                _firstPrepare = false;
            }
        }
        private static void Postfix(Deathmatch __instance)
        {
            FirstKillAnnouncement.Instance.OnMatchUpdate(__instance);
        }
    }
}
