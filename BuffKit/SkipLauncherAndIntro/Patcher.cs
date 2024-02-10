using HarmonyLib;

namespace BuffKit.SkipLauncherAndIntro
{
    [HarmonyPatch(typeof(UIManager.UINewMainMenuState), "Update")]
    class SkipLauncherAndIntro
    {
        private static bool enableSkip = true;

        private static bool firstPrepare = true;
        private static void Prepare()
        {
            if (firstPrepare)
            {
                Settings.Settings.Instance.AddEntry("misc", "skip launcher and intro", v => enableSkip = v, enableSkip);
                firstPrepare = false;
            }
        }

        private static void Postfix()
        {
            if (!enableSkip) return;
            // Update is called when the game is connected, same function that enables the play button.
            UILauncherMainPanel.ForceCloseWithoutCallback();
        }
    }
}