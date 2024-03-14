using HarmonyLib;

namespace BuffKit.ShipLoadoutNotes
{
    [HarmonyPatch]
    public class ShipLoadoutNotesPatcher
    {
        public static bool Enabled { get; private set; } = true;
        private static bool _firstPrepare = true;

        private static void Prepare()
        {
            if (_firstPrepare)
            {
                Settings.Settings.Instance.AddEntry("ship loadout notes", "ship loadout notes", v => Enabled = v, Enabled);
                _firstPrepare = false;
            }
        }

        [HarmonyPatch(typeof(UIShipCustomizationScreen), "SetActiveShip")]
        private static void Postfix()
        {
            // Called when entering the ship customization screen and when a ship loadout is selected.
            if (!Enabled) return;
            if (!ShipLoadoutNotes.Initialized) ShipLoadoutNotes.Initialize();
            ShipLoadoutNotes.LoadNote();
        }

        [HarmonyPatch(typeof(UIShipCustomizationScreen), "UpdateCamera")]
        private static bool Prefix()
        {
            // Prevent ship preview camera from moving when the text box is focused.
            if (!Enabled) return true;
            return !ShipLoadoutNotes.InputFieldFocused;
        }

        [HarmonyPatch(typeof(UIPCChatPanel), "FocusOutgoingMessageField")]
        [HarmonyPrefix]
        private static bool PreventChatBoxFocusOnEnter()
        {
            // Prevent enter going to the chat box if the chat box is open.
            if (!Enabled) return true;
            return !ShipLoadoutNotes.InputFieldFocused;
        }
    }
}