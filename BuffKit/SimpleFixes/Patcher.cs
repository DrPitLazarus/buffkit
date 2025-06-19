using HarmonyLib;
using Muse.Goi2.Entity;
using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Text = UnityEngine.UI.Text;

namespace BuffKit.SimpleFixes
{
    [HarmonyPatch]
    public class SimpleFixes
    {
        [HarmonyPatch(typeof(TeamColors), nameof(TeamColors.GetName))]
        [HarmonyPostfix]
        private static void RemoveSpaceAfterBlueTeam(ref string __result)
        {
            __result = __result.Trim();
        }

        [HarmonyPatch(typeof(UIMatchEndCrewPanel), nameof(UIMatchEndCrewPanel.SetStats))]
        [HarmonyPostfix]
        private static void FormatTimeCompleted(ref UIMatchEndCrewPanel.UIMatchEndCrewStatEntry[] ___crewStatEntries)
        {
            // Formats the time completed crew stat from seconds to m:ss (with hours just in case).
            for (var index = 0; index < ___crewStatEntries.Length; index++)
            {
                if (___crewStatEntries[index].name.text == "Time Completed")
                {
                    var originalText = ___crewStatEntries[index].value.text;
                    var isInt = int.TryParse(originalText, out _);
                    if (!isInt)
                    {
                        MuseLog.Info($"Not an int! Got:'${originalText}'.");
                        return;
                    }
                    var timeSpan = TimeSpan.FromSeconds(Convert.ToDouble(originalText));
                    var format = timeSpan.Hours > 0 ? "{0:##}:{1:00}:{2:00}" : "{1:##}:{2:00}";
                    ___crewStatEntries[index].value.text = string.Format(format, timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds);
                    return;
                }
            }
        }

        [HarmonyPatch(typeof(UIAnnouncementDisplay), nameof(UIAnnouncementDisplay.Awake))]
        [HarmonyPostfix]
        private static void AdjustKillFeedCharacterLimit(ref int ___charactersPerLine)
        {
            ___charactersPerLine = 83; // Original is 58.
        }

        [HarmonyPatch(typeof(UICreateMatchPanel), nameof(UICreateMatchPanel.Show))]
        [HarmonyPostfix]
        private static void NoScrambleByDefault(UICreateMatchPanel __instance)
        {
            // When creating a match, scramble is on by default. Who wants that?
            __instance.scrambleCheck.Checked = false;
        }

        [HarmonyPatch(typeof(UINotificationPanel), nameof(UINotificationPanel.ClearAll))]
        [HarmonyPostfix]
        private static void FixClearNotifications()
        {
            // Sometimes notifications relating to territories get stuck and do not dismiss.
            // Most noticeable during the Gauntlet event.
            // .notificationEntries do get cleared, but the gameObjects in .entryContainer remain.
            // This will fix the Clear All button, but not the individual dismiss buttons.
            var transform = UIPageFrame.Instance.socialContainer.notificationPanel.entryContainer.transform;

            while (transform.childCount > 4) // Leave the 4 template gameObjects alone.
            {
                UnityEngine.Object.DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }

        [HarmonyPatch(typeof(UILobbyChatPanel), nameof(UILobbyChatPanel.Awake))]
        [HarmonyPostfix]
        private static void AdjustChatPanelScrollSensitivity()
        {
            var scrollViewObject = GameObject.Find("/Menu UI/Standard Canvas/Menu Header Footer/Lobby Chat Panel/Scroll View");
            if (scrollViewObject == null) return;
            scrollViewObject.GetComponent<ScrollRect>().scrollSensitivity = 30; // Original 10.
        }

        [HarmonyPatch(typeof(UILibrary), nameof(UILibrary.Awake))]
        [HarmonyPostfix]
        private static void AdjustLibraryScrollSensitivity()
        {
            var parentObject = GameObject.Find("/Menu UI/Standard Canvas/Pages/Library");
            if (parentObject == null) return;
            var scrollRects = parentObject.GetComponentsInChildren<ScrollRect>(true);
            if (scrollRects == null) return;

            foreach (var scrollRect in scrollRects)
            {
                var newScrollSensitivity = 60; // Original 20. Some are 1 (lol).
                // Keep 20 for the lore book.
                if (scrollRect.name == "Selection Panel" || scrollRect.name == "Text Panel")
                {
                    newScrollSensitivity = 20;
                }
                scrollRect.scrollSensitivity = newScrollSensitivity;
            }
        }

        /// <summary>
        /// Updates the faction leaderboard display to use <c>faction_leaderboard_days</c> from the server instead of the hardcoded value.
        /// </summary>
        [HarmonyPatch(typeof(UIFactionPanel), nameof(UIFactionPanel.Awake))]
        [HarmonyPostfix]
        private static void UpdateFactionLeaderboardDays()
        {
            var days = SystemConfiguration.TryGetInt("faction_leaderboard_days");
            MuseLog.Info($"Faction leaderboard days: {days}.");
            var candidatePanelObject = UIFactionPanel.Instance?.candidatePanel;
            if (candidatePanelObject == null)
            {
                MuseLog.Info("candidatePanelObject is null!");
                return;
            }
            var resetText = candidatePanelObject.transform.Find("Candidate Panel/Container/Information Group/Information Description Label")?.GetComponent<Text>();
            if (resetText == null)
            {
                MuseLog.Info("resetText is null!");
                return;
            }
            // Only replace the number in the string.
            resetText.text = Regex.Replace(resetText.text, @"\d+", days.ToString());
        }

        /// <summary>
        /// In UIFactionPanel stats tab, some ScrollRects are not scrolled to the top when initialized.
        /// </summary>
        [HarmonyPatch(typeof(UIFactionPanel), nameof(UIFactionPanel.Awake))]
        [HarmonyPostfix]
        private static void FixMyFactionStatsScrollRects()
        {
            var scrollRects = UIFactionPanel.Instance?.myStatsPanel?.GetComponentsInChildren<ScrollRect>();
            if (scrollRects?.Length == 0)
            {
                MuseLog.Info("scrollRects is zero!");
                return;
            }
            foreach (var scrollRect in scrollRects)
            {
                // Reset scroll position to the top.
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }
    }



    [HarmonyPatch]
    public class AudioResetButton
    {
        private static bool _firstRun = true;

        [HarmonyPatch(typeof(UIOptionsAudioPanel), nameof(UIOptionsAudioPanel.Start))]
        [HarmonyPostfix]
        private static void Start()
        {
            if (!_firstRun) return;
            _firstRun = false;

            MuseLog.Info("Initializing...");
            var templateButtonGroupObject = GameObject.Find("/Menu UI/Standard Canvas/Pages/Options/Keyboard and Mouse Panel/Keyboard and Mouse Content/Binding Button Group");
            if (templateButtonGroupObject == null)
            {
                MuseLog.Info("templateButtonGroupObject is null!");
                return;
            }
            var parentObject = GameObject.Find("/Menu UI/Standard Canvas/Pages/Options/Audio Options Panel/Advanced Video Settings Content");
            if (parentObject == null)
            {
                MuseLog.Info("parentObject is null!");
                return;
            }
            var buttonGroupObject = GameObject.Instantiate(templateButtonGroupObject, parentObject.transform, false);
            buttonGroupObject.name = "Reset Audio Button Group";
            UnityEngine.Object.Destroy(buttonGroupObject.transform.GetChild(1).gameObject); // Don't need 2nd button.
            var resetAudioButtonObject = buttonGroupObject.transform.GetChild(0).gameObject;
            resetAudioButtonObject.name = "Reset Audio Button";
            resetAudioButtonObject.GetComponentInChildren<Text>().text = "Reset Audio";
            resetAudioButtonObject.GetComponentInChildren<Button>().onClick.AddListener(ResetAudio);
        }

        private static void ResetAudio()
        {
            MuseLog.Info("Resetting audio...");
            var config = AudioSettings.GetConfiguration();
            AudioSettings.Reset(config);
        }
    }
}