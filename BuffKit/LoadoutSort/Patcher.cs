using HarmonyLib;

namespace BuffKit.LoadoutSort
{
    [HarmonyPatch(typeof(LoadoutQueryData), "GetChangedSkill")]
    class LoadoutQueryData_GetChangedSkills
    {
        static bool firstCall = true;

        private static void Prepare()
        {
            if (firstCall)
            {
                firstCall = false;
                Util.OnGameInitialize += UILoadoutSortPanel._Initialize;
            }
        }
    }

    [HarmonyPatch(typeof(UIPageFrame), "TryHideOverlay")]
    class UIPageFrame_TryHideOverlay
    {
        private static void Postfix(ref bool __result)
        {
            __result = __result || UILoadoutSpecificSortPanel.Instance.TryHide() || UILoadoutSortPanel.Instance.TryHide();
        }
    }

    [HarmonyPatch(typeof(UIPageFrame), "HideAllElements")]
    class UIPageFrame_HideAllElements
    {
        private static void Postfix()
        {
            UILoadoutSortPanel.Instance?.TryHide();
            UILoadoutSpecificSortPanel.Instance.TryHide();
        }
    }
}