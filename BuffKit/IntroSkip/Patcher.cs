using HarmonyLib;
using UnityEngine.UI;

namespace BuffKit.IntroSkip
{
    [HarmonyPatch(typeof(UIManager.UIInitialState), "Enter")]
    class UIInitialState_Enter
    {
        private static bool enableSkip = true;

        private static void Postfix()
        {
            Util.Util.OnSettingsInitialize += () =>
            {
                Settings.Settings.Instance.AddEntry("misc", "skip intro", v => enableSkip = v,
                    enableSkip);

                if (!enableSkip) return;
                var t = UILauncherMainPanel.Instance.transform;
                var button = t
                    .FindChild(
                        "Launcher Main Panel/Content/Bottom Panel/Bottom Panel Content/Play Button Panel/Play Button")
                    .gameObject.GetComponent<Button>();

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(UILauncherMainPanel.ForceCloseWithoutCallback);
            };
        }
    }
}