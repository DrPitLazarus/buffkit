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
        private static readonly TimeSpan _announcementDelay = TimeSpan.FromSeconds(0.1);

        private static bool _enabled = true;
        private static bool _logAlertsInChat = true;
        private static bool _firstPrepare = true;

        private static AnnouncementInfo _cachedAnnouncement;
        private static DateTime? _announcementTime;
        private static Func<string> _announcementRawCallback;
        private static Func<string> _announcementLogCallback;

        private static void Prepare()
        {
            if (!_firstPrepare) return;
            Settings.Settings.Instance.AddEntry("skirmish alerts", "skirmish alerts", v => _enabled = v, _enabled);
            Settings.Settings.Instance.AddEntry("skirmish alerts", "log alerts in chat", v => _logAlertsInChat = v, _logAlertsInChat);
            _firstPrepare = false;
        }

        [HarmonyPatch(typeof(Mission), "Start")]
        [HarmonyPostfix]
        private static void EditAlertObject()
        {
            if (!_enabled) return;

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
        }

        [HarmonyPatch(typeof(Mission), "Update")]
        [HarmonyPostfix]
        private static void Mission_Update()
        {
            // Used to process delayed announcements.
            if (!_enabled) return;

            if (_announcementTime != null && DateTime.Now - _announcementTime >= _announcementDelay)
            {
                UIMatchAlertDisplay.instance.EnqueueRawAlert(_announcementRawCallback());
                if (_logAlertsInChat) MuseWorldClient.Instance.ChatHandler.AddMessage(ChatMessage.Console(_announcementLogCallback()));
                _announcementTime = null;
                _announcementRawCallback = null;
                _announcementLogCallback = null;
            }
        }

        [HarmonyPatch(typeof(UIAnnouncementDisplay), "HandleAnnouncement")]
        [HarmonyPostfix]
        private static void HandleAnnouncement(LinkedList<AnnouncementInfo> ___cachedAnnouncements)
        {
            // Called when an announcement is received from the server (top-left feed).
            if (!_enabled) return;

            _cachedAnnouncement = ___cachedAnnouncements.First.Value;

            switch (_cachedAnnouncement.Announcement.Verb)
            {
                case AnnouncementVerb.Killed:
                    HandleKillAnnouncement();
                    break;
                default:
                    break;
            }
        }

        private static void HandleKillAnnouncement()
        {
            // Last kill is not announced from the server, sadly.
            var deathmatch = Mission.Instance as Deathmatch;
            if (deathmatch == null)
            {
                MuseLog.Info("Deathmatch is null!");
                return;
            }

            var formattedAlertText = $";0;10;;1;{_cachedAnnouncement.formattedText}"; // Alert title is added before in raw callback.
            var alertText = "{0} {1} {2}".F(
            [
                UIAnnouncementDisplay.instance.GetSubjectText(_cachedAnnouncement.Announcement),
                UIAnnouncementDisplay.instance.GetVerbText(_cachedAnnouncement.Announcement),
                UIAnnouncementDisplay.instance.GetObjectText(_cachedAnnouncement.Announcement),
            ]);

            // Add with text. (Gun name)
            var withText = _cachedAnnouncement.Announcement.With?.Name ?? "";
            if (withText.Length > 0)
            {
                formattedAlertText += $"\nwith {withText}";
                alertText += $" with {withText}";
            }

            // Frags is not always updated when HandleAnnouncement is called. Need to call it a few frames later.
            _announcementRawCallback = () =>
            {
                var mainText = "";
                var scoreIndex = 0;

                foreach (var score in deathmatch.Frags)
                {
                    var color = TeamColors.GetColorAsHex(scoreIndex) + "FF";
                    mainText += $"<color=#{color}>{score}</color> - ";
                    scoreIndex++;
                }

                mainText = mainText.Remove(mainText.Length - 3); // Remove last " - ".
                return mainText + formattedAlertText;
            };

            _announcementLogCallback = () =>
            {
                var mainText = string.Join(" - ", Array.ConvertAll(deathmatch.Frags, v => v.ToString()));
                return $"{mainText}. {alertText}.";
            };

            _announcementTime = DateTime.Now;
        }
    }
}