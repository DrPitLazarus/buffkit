using HarmonyLib;
using Muse.Goi2.Entity;

namespace BuffKit.Broadcast
{
    [HarmonyPatch(typeof(InstanceContainerManager), "Awake")]
    class InstanceContainerManager_Awake
    {
        private static bool _firstPrepare = true;
        private static void Prepare()
        {
            if (_firstPrepare)
            {
                Util.OnGameInitialize += delegate
                {
                    Broadcaster.Initialize();
                    SaveReplay.Initialize();
                };
                _firstPrepare = false;
            }
        }

        private static void Postfix(InstanceContainerManager __instance)
        {
            __instance.gameObject.AddComponent<MatchDataObserver>();
        }
    }

    [HarmonyPatch(typeof(UIManager.UIMatchCompleteState), "Enter")]
    class UIMatchCompleteState_Enter
    {
        private static void Postfix()
        {
            SaveReplay.Instance.EndMatch();
        }
    }

    [HarmonyPatch(typeof(UIManager.UINewMatchLobbyState), "Enter")]
    class UINewMatchLobbyState_Enter
    {
        private static void Postfix()
        {
            SaveReplay.Instance.EndMatch();
        }
    }

    [HarmonyPatch(typeof(UIAnnouncementDisplay), "HandleAnnouncement")]
    class UIAnnouncementDisplay_HandleAnnouncement
    {

        private static void Postfix(Announcement newAnnouncement, string ___defaultTextColorHex, string ___neutralTextColorHex)
        {
            KillfeedObserver.HandleAnnouncement(newAnnouncement, ___defaultTextColorHex, ___neutralTextColorHex);
        }
    }
}


/*

InstanceContainerManager
*Mission (PracticeMission)
MatchLobbyView
Instance Container/Ship Container/[shipname]/Airship

 */