using System;
using System.Collections.Generic;
using HarmonyLib;
using Muse.Goi2.Entity;
using MuseBase.Multiplayer;
using MuseBase.Multiplayer.Unity;
using UnityEngine;
using UnityEngine.UI;
using Text = UnityEngine.UI.Text;

namespace BuffKit.SkirmishAlerts
{
    [HarmonyPatch]
    public class SkirmishAlerts
    {
        private static readonly TimeSpan _alertDelay = TimeSpan.FromSeconds(0.1);

        private static bool _enabled = true;
        private static bool _logAlertsInChat = true;
        private static bool _alertsHaveSound = true;
        private static bool _firstPrepare = true;

        private static UIMatchStateSoundType _alertSound => _alertsHaveSound ? UIMatchStateSoundType.Normal : UIMatchStateSoundType.None;

        private static bool _shouldBeEnabled = false;
        private static bool _firstMissionStartState = true;
        private static AnnouncementInfo _cachedAnnouncement;
        private static DateTime? _announcementTime;
        private static Func<string> _alertRawCallback;
        private static Func<string> _alertLogCallback;
        //private static string _lastShipDeath; // TODO: find a reliable way to get the last kill/death.

        private static void Prepare()
        {
            if (!_firstPrepare) return;
            Settings.Settings.Instance.AddEntry("skirmish alerts", "skirmish alerts", v => _enabled = v, _enabled);
            Settings.Settings.Instance.AddEntry("skirmish alerts", "log alerts in chat", v => _logAlertsInChat = v, _logAlertsInChat);
            Settings.Settings.Instance.AddEntry("skirmish alerts", "alerts have sound", v => _alertsHaveSound = v, _alertsHaveSound);
            _firstPrepare = false;
        }

        [HarmonyPatch(typeof(Mission), "Start")]
        [HarmonyPostfix]
        private static void Mission_Start()
        {
            if (!_enabled) return;

            ResetAll();

            if (_firstMissionStartState)
            {
                // Edit Alert Object
                var textBoxObjectPath = "/Game UI/Match UI/UI HUD Canvas/UI Alert Display/Alert Root/Text Box/";
                // Increase height of the alert text box. Original 45.
                GameObject.Find(textBoxObjectPath).GetComponent<LayoutElement>().preferredHeight = 50;
                GameObject.Find(textBoxObjectPath + "Alert Mask/").GetComponent<LayoutElement>().preferredHeight = 50;
                // Enable rich text and add a shadow component.
                var titleObject = GameObject.Find(textBoxObjectPath + "Alert Mask/Text/");
                var subtitleObject = GameObject.Find(textBoxObjectPath + "Alert Mask/SubText/");
                titleObject.GetComponent<Text>().supportRichText = true;
                subtitleObject.GetComponent<Text>().supportRichText = true;
                if (titleObject.GetComponent<Shadow>() == null) titleObject.AddComponent<Shadow>();
                if (subtitleObject.GetComponent<Shadow>() == null) subtitleObject.AddComponent<Shadow>();

                _firstMissionStartState = false;
            }

            var isSpectator = NetworkedPlayer.Local.IsSpectator;
            _shouldBeEnabled = isSpectator;
        }

        [HarmonyPatch(typeof(Mission), "Update")]
        [HarmonyPostfix]
        private static void Mission_Update()
        {
            // Used to process delayed alerts.
            if (!_enabled || !_shouldBeEnabled) return;

            if (_announcementTime != null && DateTime.Now - _announcementTime >= _alertDelay)
            {
                EnqueueRawAlert(_alertRawCallback());
                if (_logAlertsInChat) LogMessageInChat(_alertLogCallback());
                ResetAlert();
            }
        }

        //[HarmonyPatch(typeof(Ship), "OnRemoteDestroy")]
        //[HarmonyPostfix]
        //private static void Ship_OnRemoteDestroy(Ship __instance)
        //{
        //    if (!_enabled) return;

        //    _lastShipDeath = __instance.GetDisplayName();
        //}

        [HarmonyPatch(typeof(UIManager.UIMatchCompleteState), "EnterState")]
        [HarmonyPostfix]
        private static void UIManager_UIMatchCompleteState_EnterState()
        {
            if (!_enabled || !_shouldBeEnabled) return;

            var deathmatch = Mission.Instance as Deathmatch;
            if (deathmatch == null) return;

            var message = $"Final score: {ScoresToString(deathmatch.Frags)}.";
            //message += $". Last death was {_lastShipDeath}.";
            if (_logAlertsInChat) LogMessageInChat(message);
        }

        [HarmonyPatch(typeof(UIAnnouncementDisplay), "HandleAnnouncement")]
        [HarmonyPostfix]
        private static void HandleAnnouncement(LinkedList<AnnouncementInfo> ___cachedAnnouncements)
        {
            // Called when an announcement is received from the server (top-left feed).
            // Last kill is not announced from the server, sadly.
            if (!_enabled || !_shouldBeEnabled) return;

            var announcement = ___cachedAnnouncements.First.Value;

            if (announcement.Announcement.Verb == AnnouncementVerb.Killed)
            {
                _cachedAnnouncement = announcement;
                HandleKillAnnouncement();
            }
        }

        private static void HandleKillAnnouncement()
        {
            var deathmatch = Mission.Instance as Deathmatch;
            if (deathmatch == null) return;

            var formattedSubtitleText = _cachedAnnouncement.formattedText;
            var alertSubtitleText = "{0} {1} {2}".F(
            [
                UIAnnouncementDisplay.instance.GetSubjectText(_cachedAnnouncement.Announcement),
                UIAnnouncementDisplay.instance.GetVerbText(_cachedAnnouncement.Announcement),
                UIAnnouncementDisplay.instance.GetObjectText(_cachedAnnouncement.Announcement),
            ]);

            // Add with text. (Gun name)
            var withText = _cachedAnnouncement.Announcement.With?.Name ?? "";
            if (withText.Length > 0)
            {
                formattedSubtitleText += $"\nwith {withText}";
                alertSubtitleText += $" with {withText}";
            }

            // Frags is not always updated when HandleAnnouncement is called. Need to call it a few frames later.
            _alertRawCallback = () =>
            {
                var mainText = ScoresToString(deathmatch.Frags, formatTeamColors: true);
                return BuildRawAlert(mainText, formattedSubtitleText, icon: 10, sound: _alertSound); // 10 is Boss icon.
            };

            _alertLogCallback = () =>
            {
                var mainText = ScoresToString(deathmatch.Frags);
                return $"{mainText}. {alertSubtitleText}.";
            };

            _announcementTime = DateTime.Now;
        }

        private static string BuildRawAlert(string title, string subtitle = "", int icon = -1, UIMatchStateSoundType sound = UIMatchStateSoundType.Normal, int duration = 0, string textInIcon = "")
        {
            // Icon required for subtitle to show.
            return $"{title.Trim()};{duration};{icon};{textInIcon.Trim()};{(int)sound};{subtitle.Trim()}";
        }

        private static void EnqueueRawAlert(string rawAlert)
        {
            UIMatchAlertDisplay.instance.EnqueueRawAlert(rawAlert);
        }

        private static void LogMessageInChat(string message)
        {
            MuseWorldClient.Instance.ChatHandler.AddMessage(ChatMessage.Console(message));
        }

        private static void ResetAlert()
        {
            _cachedAnnouncement = null;
            _announcementTime = null;
            _alertRawCallback = null;
            _alertLogCallback = null;
        }

        private static void ResetAll()
        {
            ResetAlert();
            //_lastShipDeath = null;
        }

        private static string ScoresToString(int[] scoresArray, bool formatTeamColors = false)
        {
            if (!formatTeamColors) return string.Join(" - ", Array.ConvertAll(scoresArray, v => v.ToString()));

            var text = "";
            var scoreIndex = 0;
            foreach (var score in scoresArray)
            {
                var color = TeamColors.GetColorAsHex(scoreIndex) + "FF";
                text += $"<color=#{color}>{score}</color> - ";
                scoreIndex++;
            }
            return text.Remove(text.Length - 3); // Remove last " - ".
        }
    }
}