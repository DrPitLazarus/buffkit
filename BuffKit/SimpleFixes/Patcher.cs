using System;
using HarmonyLib;

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
    }
}