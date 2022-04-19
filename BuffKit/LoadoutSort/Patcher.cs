using BuffKit.UI;
using HarmonyLib;

namespace BuffKit.LoadoutSort
{
    [HarmonyPatch(typeof(UIManager.UILoadingLobbyState), "Exit")]
    class UILoadingLobbyState_Exit
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
}