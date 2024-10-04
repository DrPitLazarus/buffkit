using BuffKit.UI;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

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
            MuseLog.Info("In patch initialize");

            _obToggle = Builder.BuildMenuToggle(
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
                //    MuseLog.Info($"In enterMatchLobby - running: {mlv.Running}, started: {mlv.Started}, loading: {mlv.Loading}");
                //}
                if (mlv != null && !mlv.Started)
                    _toggle.isOn = false;
                _obToggle.SetActive(mlv != null && !mlv.Running && Util.HasModPrivilege(mlv));
            };
        }

        private static void OnToggleChange(bool v)
        {
            FirstKillAnnouncement.Instance.SetEnabled(_enableFKA && v);
            AutoStartTimer.SetEnabled(_enableAutoTimer && v);
            MuseLog.Info($"Ref auto tools callback called with {v}");
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
                Util.OnGameInitialize += Patcher.Init;
                _firstPrepare = false;
            }
        }
        private static void Postfix(Deathmatch __instance)
        {
            FirstKillAnnouncement.Instance.OnMatchUpdate(__instance);
        }
    }


    /// <summary>
    /// Adds a Force Start button in the lobby footer for players with mod privilege. There is no confirm prompt.
    /// Enabled by default.
    /// </summary>
    [HarmonyPatch]
    public class ForceStartModButton
    {
        private static bool _firstRun = true;
        private static bool _enabled = true;

        private static void Prepare()
        {
            if (!_firstRun) return;
            _firstRun = false;
            Settings.Settings.Instance.AddEntry("ref tools", "force start mod button", v => _enabled = v, _enabled);
        }

        [HarmonyPatch(typeof(UIManager.UINewMatchLobbyState), nameof(UIManager.UINewMatchLobbyState.PaintFooterButtons))]
        [HarmonyPostfix]
        private static void Postfix()
        {
            if (!_enabled) return;

            var mlv = MatchLobbyView.Instance;
            if (mlv == null || NetworkedPlayer.Local == null) return;
            if (!Util.HasModPrivilege(mlv)) return;

            var footer = UIPageFrame.Instance.footer;
            footer.AddButton("Force Start", ActivateForceStart);
        }

        private static void ActivateForceStart()
        {
            var mlv = MatchLobbyView.Instance;
            if (mlv == null) return;
            // Index 4 is MatchForceStartAction.
            MuseLog.Info("Activating!");
            MatchModAction.MatchLobbyActions[4].Act(mlv);
        }
    }
}
