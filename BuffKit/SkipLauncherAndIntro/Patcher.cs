using HarmonyLib;

namespace BuffKit.SkipLauncherAndIntro
{
    [HarmonyPatch(typeof(UIManager.UINewMainMenuState), "Update")]
    class SkipLauncherAndIntro
    {
        private static bool _enableSkip = true;
        private static bool _firstPrepare = true;

        private static void Prepare()
        {
            if (_firstPrepare)
            {
                Settings.Settings.Instance.AddEntry("misc", "skip launcher and intro", v => _enableSkip = v, _enableSkip);
                _firstPrepare = false;
            }
        }

        private static void Postfix()
        {
            if (!_enableSkip) return;
            // Update is called when the game is connected, same function that enables the play button.
            UILauncherMainPanel.ForceCloseWithoutCallback();
        }
    }
}