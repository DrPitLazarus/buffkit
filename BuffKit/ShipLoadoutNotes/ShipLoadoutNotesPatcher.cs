using HarmonyLib;

namespace BuffKit.ShipLoadoutNotes
{
    [HarmonyPatch]
    internal class ShipLoadoutNotesPatcher
    {
        public static bool Enabled { get; private set; } = true;
        private static bool _firstPrepare = true;

        private static void Prepare()
        {
            if (!_firstPrepare) return;
            Util.OnGameInitialize += () =>
            {
                Settings.Settings.Instance.AddEntry("ship loadout notes", "ship loadout notes", v => Enabled = v, Enabled);
            };
            _firstPrepare = false;
        }

        /// <summary>
        /// Initialize and load note. Called when entering the ship customization screen and when a ship loadout is selected.
        /// </summary>
        [HarmonyPatch(typeof(UIShipCustomizationScreen), nameof(UIShipCustomizationScreen.SetActiveShip))]
        private static void Postfix()
        {
            if (!Enabled) return;
            if (!ShipLoadoutNotes.Initialized) ShipLoadoutNotes.Initialize();
            ShipLoadoutNotes.LoadNote();
        }

        /// <summary>
        /// Prevents ship preview camera from moving when the text box is focused.
        /// </summary>
        [HarmonyPatch(typeof(UIShipCustomizationScreen), nameof(UIShipCustomizationScreen.UpdateCamera))]
        private static bool Prefix()
        {
            if (!Enabled) return true;
            return !ShipLoadoutNotes.InputFieldFocused;
        }

        /// <summary>
        /// Prevents enter going to the chat box if the chat box is open.
        /// </summary>
        [HarmonyPatch(typeof(UIPCChatPanel), nameof(UIPCChatPanel.FocusOutgoingMessageField))]
        [HarmonyPrefix]
        private static bool PreventChatBoxFocusOnEnter()
        {
            if (!Enabled) return true;
            return !ShipLoadoutNotes.InputFieldFocused;
        }
    }
}