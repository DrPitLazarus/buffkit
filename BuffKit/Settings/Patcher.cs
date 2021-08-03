using HarmonyLib;

namespace BuffKit.Settings
{
    [HarmonyPatch(typeof(UIMatchLobby), "Awake")]
    class UIMatchLobby_Awake
    {
        [HarmonyPatch(typeof(UIManager.UILoadingLobbyState), "Exit")]
        public class UILoadingLobbyState_Exit
        {
            static bool _firstCall = true;
            private static void Prepare()
            {
                if (_firstCall)
                {
                    Util.Util.OnGameInitialize += Settings._Initialize;
                    _firstCall = false;
                }
            }
        }
    }
}
