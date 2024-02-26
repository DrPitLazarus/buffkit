using HarmonyLib;

namespace BuffKit.ShipLoadoutNotes
{
    [HarmonyPatch]
    public class ShipLoadoutNotesPatcher
    {
        [HarmonyPatch(typeof(UIShipCustomizationScreen), "SetActiveShip")]
        private static void Postfix()
        {
            // Called when entering the ship customization screen and when a ship loadout is selected.
            if (!ShipLoadoutNotes.IsInitialized) ShipLoadoutNotes.Initialize();
            ShipLoadoutNotes.LoadNote();
        }

        [HarmonyPatch(typeof(UIShipCustomizationScreen), "UpdateCamera")]
        private static bool Prefix()
        {
            // Prevent ship preview camera from moving when the text box is focused.
            return !ShipLoadoutNotes.NoteInputFieldIsFocused;
        }
    }
}