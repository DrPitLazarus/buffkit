using HarmonyLib;
using UnityEngine.UI;
using static BuffKit.Util;

namespace BuffKit.MapPicker
{
    [HarmonyPatch(typeof(UIManager.UINewMatchLobbyState), "PaintFooterButtons")]
    class UINewMatchLobbyState_PaintFooterButtons
    {
        private static bool _firstPrepare = true;
        private static void Prepare()
        {
            if (!_firstPrepare) return;
            
            OnGameInitialize += delegate
            {
                Settings.Settings.Instance.AddEntry("ref tools", "only show DM maps", delegate (bool v)
                {
                    MapPicker.FilterNonDM = v;
                }, MapPicker.FilterNonDM);
            };

            _firstPrepare = false;
        }
        
        private static bool Prefix()
        {
            var mlv = MatchLobbyView.Instance;
            if (mlv == null || NetworkedPlayer.Local == null) return false;

            var footer = UIPageFrame.Instance.footer;
            footer.ClearButtons();

            if (!HasModPrivilege(mlv)) return false;
            footer.AddButton("CHANGE MAP", delegate { MapPicker.Paint(); });
            footer.AddButton("MOD MATCH", delegate { UIManager.UINewMatchLobbyState.instance.ModFeatures(); });

            return false;
        }
    }

    [HarmonyPatch(typeof(UINewModalDialog), "Awake")]
    class UINewModalDialog_Awake
    {
        private static void Postfix()
        {
            var dropdown = UIPageFrame.Instance.modalDialog.dropdown;
            var template = dropdown.transform.FindChild("Template");
            var content = template.FindChild("Viewport").GetChild(0);
            var item = content.GetChild(0);
            item.gameObject.AddComponent<ScrollEventPassthrough>();
            var sr = template.GetComponent<ScrollRect>();
            sr.scrollSensitivity = 30;
        }
    }

}