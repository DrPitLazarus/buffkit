using HarmonyLib;

namespace BuffKit.MatchModMenu
{
    [HarmonyPatch(typeof(UIManager.UIMatchMenuState), "ModFeatures")]
    class UIMatchMenuState_ModFeatures
    {
        private static bool Prefix()
        {
            UIManager.TransitionToState(UIModMenuState.Instance);
            return false;
        }
    }
}