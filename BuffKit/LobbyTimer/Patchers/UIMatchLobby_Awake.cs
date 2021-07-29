using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace BuffKit.LobbyTimer.Patchers
{
    [HarmonyPatch(typeof(UIMatchLobby), "Awake")]
    public class UIMatchLobby_Awake
    {
        public static void Postfix()
        {
            var lobby = UIMatchLobby.Instance;

            var prototypeButton = 
                Object.Instantiate(lobby.shipCustomizationButton, lobby.shipCustomizationButton.transform.parent);
            prototypeButton.transform.name = "prototypeButton";

            var tbcGO = prototypeButton.transform.parent.gameObject;
            var tbc = tbcGO.AddComponent<TimerButtonContainer>();
            
            prototypeButton.gameObject.SetActive(false);
            
            tbc.StartTimerButton = 
                Object.Instantiate(prototypeButton, tbcGO.transform);
            tbc.StartTimerButton.name = "Start Timer Button";
            tbc.StartTimerButton.GetComponentInChildren<Text>().text = "START TIMER";
            
            tbc.StartOvertimeButton = 
                Object.Instantiate(prototypeButton, tbcGO.transform);
            tbc.StartOvertimeButton.name = "Start Overtime Button";
            tbc.StartOvertimeButton.GetComponentInChildren<Text>().text = "START OVERTIME";
            
            tbc.PauseTimerButton = 
                Object.Instantiate(prototypeButton, tbcGO.transform);
            tbc.PauseTimerButton.name = "Pause Timer Button";
            tbc.PauseTimerButton.GetComponentInChildren<Text>().text = "PAUSE TIMER";
            
            tbc.ResumeTimerButton = 
                Object.Instantiate(prototypeButton, tbcGO.transform);
            tbc.ResumeTimerButton.name = "Resume Timer Button";
            tbc.ResumeTimerButton.GetComponentInChildren<Text>().text = "RESUME TIMER";
            
            tbc.ExtendPauseButton = 
                Object.Instantiate(prototypeButton, tbcGO.transform);
            tbc.ExtendPauseButton.name = "Extend Pause Button";
            tbc.ExtendPauseButton.GetComponentInChildren<Text>().text = "EXTEND PAUSE";
            
            tbc.RefPauseTimerButton = 
                Object.Instantiate(prototypeButton, tbcGO.transform);
            tbc.RefPauseTimerButton.name = "Ref Pause Button";
            tbc.RefPauseTimerButton.GetComponentInChildren<Text>().text = "USE REF PAUSE";
            tbcGO.SetActive(false);
        }
    }
}