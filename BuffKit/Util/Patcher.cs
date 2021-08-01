using HarmonyLib;

namespace BuffKit.Util
{
    [HarmonyPatch(typeof(UIManager.UILoadingLobbyState), "Exit")]
    public class UILoadingLobbyState_Exit
    {
        static bool _firstCall = true;
        private static void Postfix()
        {
            if (_firstCall)
            {
                _firstCall = false;
                Util._Initialize();
            }
            else
                Util._OnLobbyLoadTrigger();     // Util._Initialize calls this
        }
    }
}
