using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Muse.Goi2.Entity;

namespace BuffKit.AchievementScreenState
{
    [HarmonyPatch]
    public class AchievementScreenState
    {
        private static bool _enabled = true;
        private static bool _firstPrepare = true;

        private static AchievementScreenStateData _workingState = new();
        private static readonly List<AchievementScreenStateData> _savedStates = new(3);
        private static int _lastState = -1;
        private static bool _achievementSpecified = true;

        private struct AchievementScreenStateData
        {
            public GameType GameType;
            public string MajorCategory;
            public string MinorCategory;
            public Quest Quest;

            public override readonly string ToString()
            {
                return $"{(int)GameType}, {MajorCategory}, {MinorCategory}, {Quest?.NameText.En ?? ""}";
            }
        }

        private static void Prepare()
        {
            if (!_firstPrepare) return;
            Settings.Settings.Instance.AddEntry("misc", "achievement screen state", v => _enabled = v, _enabled);
            _firstPrepare = false;
        }

        [HarmonyPatch(typeof(UIAchievementChainFinishPopup), "Awake")]
        [HarmonyPrefix]
        private static void PopupChain_Awake()
        {
            UIAchievementChainFinishPopup.Instance.viewAchievementButton.onClick.AddListener(delegate () { if (_enabled) { _achievementSpecified = true; } });
        }

        [HarmonyPatch(typeof(UIAchievementPopup), "Awake")]
        [HarmonyPrefix]
        private static void Popup_Awake()
        {
            UIAchievementPopup.Instance.viewAchievementButton.onClick.AddListener(delegate () { if (_enabled) { _achievementSpecified = true; } });
        }

        [HarmonyPatch(typeof(UIManager), "UIAchievements", [])] // Select overloaded method with no params.
        [HarmonyPrefix]
        private static bool UIAchievements()
        {
            // Called when going to the achievement screen with no achievement specified. Example: Profile > Achievements button.
            if (!_enabled) return true;

            _achievementSpecified = false;

            if (_lastState == -1)
            {
                MuseLog.Info("No last state, going to default state.");
                UIManager.UIAchievements(null);
                return false;
            }

            var savedIndex = _lastState - 1;
            MuseLog.Info($"Restoring last state: {_savedStates[savedIndex]}.");
            UIManager.UIAchievements(_savedStates[savedIndex].Quest);

            return false;
        }

        [HarmonyPatch(typeof(UIAchievementScreen), "SelectMode")]
        [HarmonyPrefix]
        private static void SelectMode(GameType type, UIAchievementScreen __instance)
        {
            // Called when game type is selected. The PvP, Co-op, and Neutral tabs.
            if (!_enabled) return;

            if (_achievementSpecified)
            {
                MuseLog.Info($"Don't restore tab state, achievement specified: {__instance.ViewTarget.NameText.En}.");
                return;
            }

            if (_lastState == -1) return;

            var savedIndex = (int)type - 1;
            var state = _savedStates[savedIndex];

            if (state.Quest == null)
            {
                MuseLog.Info($"No tab state {(int)type}, going to default state.");
                return;
            }

            MuseLog.Info($"Restoring tab state: {_savedStates[savedIndex]}");
            __instance.ViewTarget = state.Quest;
        }

        [HarmonyPatch(typeof(UIAchievementScreen), "SelectQuest")]
        [HarmonyPostfix]
        private static void SelectQuest(Quest quest)
        {
            // Called when an achievement is selected.
            if (!_enabled) return;

            // Ignore changes while the UI is navigating to the specified achievement. Unlike ViewTarget, toViewQuest is not set to null immediately.
            if (_achievementSpecified && UIManager.UIAchievementState.instance.toViewQuest != quest) return;

            // Set the working state.
            _workingState.Quest = quest;
            _workingState.MinorCategory = quest.Category;
            // Next line from UIAchievementScreen.Activated setter.
            var achievementMajorCategory = AchievementMajorCategory.Instances.FirstOrDefault((AchievementMajorCategory c) => c.Criteria(quest));
            _workingState.MajorCategory = achievementMajorCategory.Name;
            _workingState.GameType = achievementMajorCategory.GameType;
            _lastState = (int)achievementMajorCategory.GameType;

            SaveState();

            // Reset flag.
            if (_achievementSpecified) _achievementSpecified = false;
        }

        private static void SaveState()
        {
            // Init.
            if (_savedStates.Count == 0)
            {
                _savedStates.Add(new AchievementScreenStateData { });
                _savedStates.Add(new AchievementScreenStateData { });
                _savedStates.Add(new AchievementScreenStateData { });
            }

            // Skirmish = 1, Co-op = 2, Neutral = 3.
            var savedIndex = (int)_workingState.GameType - 1;
            if (savedIndex < 0) return;
            // Don't save if the same.
            if (_workingState.ToString() == _savedStates[savedIndex].ToString()) return;

            _savedStates[savedIndex] = _workingState;
            MuseLog.Info($"Saved state: {_savedStates[savedIndex]}.");
        }
    }
}