using HarmonyLib;
using UnityEngine;

namespace BuffKit.KothUiFix
{
    [HarmonyPatch(typeof(UIManager), "Start")]
    class UIManager_Start
    {
        static void Postfix()
        {
            KothUiFix.CreateFix();
        }
    }

    [HarmonyPatch(typeof(UIControlPointHUD), "OnUpdate")]
    class UIControlPointHUD_OnUpdate
    {
        static bool Prefix(UIControlPointHUD __instance)
        {
            if (__instance.isActive && __instance.controlPoint != null && __instance.controlPoint.MatchInProgress)
            {
                if (__instance.kingOfTheHill != null)
                {
                    for (int i = 0; i < __instance.teamHuds.Length; i++)
                    {
                        if (__instance.kingOfTheHill.resourcesGathered != null &&
                            __instance.kingOfTheHill.resourcesGathered.Length > i &&
                            __instance.kingOfTheHill.resourcesGathered[i] != null &&
                            __instance.kingOfTheHill.resourcesGathered[i].resources != null &&
                            __instance.kingOfTheHill.resourcesGathered[i].resources.Length == 1)
                        {
                            __instance.teamHuds[i].scoreLabel.text =
                                ((int)__instance.kingOfTheHill.resourcesGathered[i].resources[0].amount).ToString();
                        }
                        else
                        {
                            __instance.teamHuds[i].scoreLabel.text = "0";
                        }
                    }
                }
                if (__instance.crazyKing != null)
                {
                    int timeToNextHill = __instance.crazyKing.TimeToNextHill;
                    __instance.countdownTimerText.text =
                        string.Format("{0}:{1:D2}", timeToNextHill / 60, timeToNextHill % 60);
                    if (__instance.lastActiveObjectiveIndex != __instance.currentlyActiveObjectiveIndex)
                    {
                        __instance.lastActiveObjectiveIndex = __instance.currentlyActiveObjectiveIndex;
                        string text = ((char)(__instance.currentlyActiveObjectiveIndex + 65)).ToString();
                        if (__instance.centralControlPointText != null)
                        {
                            __instance.centralControlPointText.text = text;
                        }
                        if (UIMatchAlertDisplay.instance != null)
                        {
                            UIMatchAlertDisplay.instance.EnqueueAlert(
                                string.Format("Control Point {0} is Active", text), 0f, 0, text,
                                UIMatchStateSoundType.None);
                        }
                    }
                }
                else if (__instance.centralControlPointText != null)
                {
                    __instance.centralControlPointText.text = string.Empty;
                }
                __instance.RefreshCentralControlPointIconForFirstActivePoint();
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(UIControlPointHUD), "RefreshControlPoint")]
    class UIControlPointHUD_RefreshControlPoint
    {
        static bool Prefix(UIControlPointDisplayIcon controlPointIcon,
            bool isActive,
            float controlProgress,
            int controllingTeam,
            int capturingTeam,
            int capPointCount,
            int extraControlPointCount)
        {
            if (isActive)
            {
                if (controllingTeam == 0 || controllingTeam == 1 || controllingTeam == 2 || controllingTeam == 3)
                {
                    if (controlProgress == 0f && capturingTeam == -1)
                    {
                        controlProgress = 1f;
                    }
                    controlPointIcon.Fill(controlProgress, TeamColors.GetColor(controllingTeam));
                    controlPointIcon.RecolorCenter(TeamColors.GetColor(controllingTeam));
                }
                else if (capturingTeam == 0 || capturingTeam == 1 || capturingTeam == 2  || capturingTeam == 3)
                {
                    controlPointIcon.Fill(controlProgress, TeamColors.GetColor(capturingTeam));
                    controlPointIcon.RecolorCenter(Color.clear);
                }
                else
                {
                    controlPointIcon.ClearFill();
                    controlPointIcon.RecolorCenter(Color.clear);
                }
            }
            else
            {
                controlPointIcon.ClearFill();
                controlPointIcon.RecolorCenter(Color.clear);
            }

            return false;
        }
    }
}
