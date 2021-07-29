using BuffKit.MatchModMenu;
using HarmonyLib;

namespace BuffKit.Patchers
{
    [HarmonyPatch(typeof(UIManager.UIMatchMenuState), "ModFeatures")]
    public static class UIMatchMenuState_ModFeatures
    {
        private static bool Prefix()
        {
            UIManager.TransitionToState(UIModMenuState.Instance);
            return false;
        }
    }
}