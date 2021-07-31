using HarmonyLib;
using static BuffKit.Util;

namespace BuffKit.MapPicker.Patchers
{
    [HarmonyPatch(typeof(UIManager.UINewMatchLobbyState), "PaintFooterButtons")]
    public class UINewMatchLobbyState_PaintFooterButtons
    {
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
}