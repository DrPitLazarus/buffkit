using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Muse.Goi2.Entity;

namespace BuffKit.AchievementScreenState
{
    [HarmonyPatch]
    public class AchievementScreenState
    {
        private static bool _firstPrepare = true;

        private static AchievementScreenStateData _workingState = new();
        private static List<AchievementScreenStateData> _savedStates = new(3);
        private static int _lastState = -1;

        private struct AchievementScreenStateData
        {
            public GameType GameType;
            public string MajorCategory;
            public string MinorCategory;
            public Quest Quest;

            public readonly string StateString => $"{(int)GameType}, {MajorCategory}, {MinorCategory}, {Quest?.NameText.En ?? ""}";
        }

        private static void Prepare()
        {
            if (_firstPrepare)
            {
                //Settings.Settings.Instance.AddEntry("ship loadout notes", "ship loadout notes", v => Enabled = v, Enabled);
                _firstPrepare = false;
            }
        }

        [HarmonyPatch(typeof(UIManager), "UIAchievements", [])] // Select overloaded method with no params.
        [HarmonyPrefix]
        private static bool UIAchievements()
        {
            // Called when going to the achievement screen with no achievement specified. Example: Profile > Achievements button.
            if ((_lastState == -1))
            {
                MuseLog.Info("No last state, going to default state.");
                return true;
            }

            var savedIndex = (int)_lastState - 1;
            MuseLog.Info($"Restoring last state: {_savedStates[savedIndex].StateString}.");
            UIManager.UIAchievements(_savedStates[savedIndex].Quest);

            return false;
        }

        [HarmonyPatch(typeof(UIAchievementScreen), "SelectMode")]
        [HarmonyPrefix]
        private static bool SelectMode(GameType type, UIAchievementScreen __instance)
        {
            // Called when game type is selected. The PvP, Co-op, and Neutral tabs.
            if (_lastState == -1) return true;

            var savedIndex = (int)type - 1;
            var state = _savedStates[savedIndex];

            if (state.Quest == null)
            {
                MuseLog.Info($"No tab state {(int)type}, going to default state.");
                return true;
            }

            //if (__instance.ViewTarget != null)
            //{
            //    MuseLog.Info($"Don't restore tab state, achievement specified: {__instance.ViewTarget.NameText.En}.");
            //    return true;
            //}

            MuseLog.Info($"Restoring tab state: {_savedStates[savedIndex].StateString}");
            __instance.ViewTarget = state.Quest;

            return true;
        }

        [HarmonyPatch(typeof(UIAchievementScreen), "SelectQuest")]
        [HarmonyPostfix]
        private static void SelectQuest(Quest quest, UIAchievementScreen __instance)
        {
            // Called when an achievement is selected.
            // Set the working state.
            _workingState.Quest = quest;
            _workingState.MinorCategory = quest.Category;

            // From UIAchievementScreen.Activated setter.
            var achievementMajorCategory = AchievementMajorCategory.Instances.FirstOrDefault((AchievementMajorCategory c) => c.Criteria(quest));

            _workingState.MajorCategory = achievementMajorCategory.Name;
            _workingState.GameType = achievementMajorCategory.GameType;
            _lastState = (int)achievementMajorCategory.GameType;
            SaveState();
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

            // Skirmish = 1, Coop = 2, Neutral = 3.
            var savedIndex = (int)_workingState.GameType - 1;
            if (savedIndex < 0) return;
            // Don't save if the same.
            if (_workingState.StateString == _savedStates[savedIndex].StateString) return;

            _savedStates[savedIndex] = _workingState;
            MuseLog.Info($"Saved state: {_savedStates[savedIndex].StateString}.");
        }
    }
}