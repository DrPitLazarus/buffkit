using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static BuffKit.Util.Util;

namespace BuffKit.MapPicker
{
    [HarmonyPatch(typeof(UIManager.UINewMatchLobbyState), "PaintFooterButtons")]
    class UINewMatchLobbyState_PaintFooterButtons
    {
        public static bool Prefix()
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
    class UINewModalDialog_Select
    {
        public static void Postfix()
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