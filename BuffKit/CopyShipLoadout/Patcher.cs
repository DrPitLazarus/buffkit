using HarmonyLib;
using Muse.Goi2.Entity.Vo;
using Muse.Networking;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BuffKit.CopyShipLoadout
{
    [HarmonyPatch]
    public class CopyShipLoadout
    {
        private static bool _enabled = true;
        private static bool _showCopyShipLoadoutNotification = true;
        private static bool _firstMainMenuState = true;

        private static ShipDataController _shipDataController => UIManager.UINewShipState.instance.shipDataController;
        private static UIShipCustomizationScreen _shipCustomizationScreen => UIShipCustomizationScreen.Instance;

        /// <summary>
        /// Feature initialization. Create settings.
        /// </summary>
        [HarmonyPatch(typeof(UIManager.UINewMainMenuState), nameof(UIManager.UINewMainMenuState.Enter))]
        [HarmonyPostfix]
        private static void Initialize()
        {
            if (!_firstMainMenuState) return;
            _firstMainMenuState = false;
            Settings.Settings.Instance.AddEntry("loadout manager", "loadout manager/copy ship loadout", v => _enabled = v, _enabled);
            Settings.Settings.Instance.AddEntry("loadout manager", "loadout manager/copy ship loadout notification", v => _showCopyShipLoadoutNotification = v, _showCopyShipLoadoutNotification);

            // AllShips is null if the player has not opened the ship customization screen yet.
            if (_shipDataController.AllShips == null)
            {
                _shipDataController.QueryShipsFromServer();
            }
        }

        /// <summary>
        /// Add the "Copy Loadout" button to the footer of the ship customization screen. Works on your own ship and previewing other ships.
        /// </summary>
        [HarmonyPatch(typeof(UIManager.UINewShipInspectionState), nameof(UIManager.UINewShipInspectionState.Enter))]
        [HarmonyPatch(typeof(UIManager.UINewShipState), nameof(UIManager.UINewShipState.Enter))]
        [HarmonyPostfix]
        private static void AddCopyLoadoutButtonToFooter()
        {
            if (!_enabled) return;
            UIPageFrame.Instance.footer.AddButton("Copy Loadout", CopyFromShipCustomizationScreen);
        }

        /// <summary>
        /// Action for the "Copy Loadout" button. Opens a dropdown modal dialog with the player's ship presets.
        /// </summary>
        private static void CopyFromShipCustomizationScreen()
        {
            // Gather ship data from the ship customization screen.
            var shipModelId = _shipCustomizationScreen.currentShip.Model.Id;
            var shipModelName = _shipCustomizationScreen.currentShip.Model.NameText.En;
            var shipGunSlotNamesAndGunIds = new Dictionary<string, int>();
            foreach (var gunSlot in _shipCustomizationScreen.gunSlots)
            {
                // All 6 gun slots always exist, but they may not be activated.
                // Was getting gun slot names conflicts in the dictionary.
                if (!gunSlot.Activated) continue;
                var gunSlotName = gunSlot.Item.Name;
                var gunId = gunSlot.Item.GunId;
                shipGunSlotNamesAndGunIds.Add(gunSlotName, gunId);
            }
            // Check if the player has the ship model in their inventory.
            var shipViewObject = GetShipViewObjectByShipModelId(shipModelId);
            if (shipViewObject == null)
            {
                Util.SendToastNotification($"ERROR: Cannot copy. You do not have the ship {shipModelName}.", true);
                MuseLog.Info($"ERROR: Ship model ID {shipModelId} was not found in player's ShipDataController.AllShips. Exiting.");
                return;
            }
            var playerShipId = shipViewObject.Id;

            // Initialize the dropdown modal dialog.
            var dialogTitle = "Copy Ship Loadout";
            var dialogMessage = "Select the ship preset slot you would like to save over.";
            var indexedShipLoadoutViewObjectList = shipViewObject.Presets.Select((shipLoadoutViewObject, index) =>
                new IndexedShipLoadoutViewObject { Index = index, ShipLoadoutViewObject = shipLoadoutViewObject })
                .ToList();
            var dropdownOptions = UINewModalDialog.DropdownSetting.CreateSetting<IndexedShipLoadoutViewObject>(
                indexedShipLoadoutViewObjectList,
                // Method to get generate text for each dropdown option.
                // Examples: "Preset 1" or "Preset 1: ShipPresetNameHere" or "Preset 1: ShipPresetNameHere (active) or Preset 1 (active)"
                (IndexedShipLoadoutViewObject preset) =>
                {
                    var dropdownText = $"Preset {preset.Index + 1}";
                    if (preset.ShipLoadoutViewObject.Name != null)
                    {
                        dropdownText += $": {preset.ShipLoadoutViewObject.Name}";
                    }
                    if (shipModelId == _shipDataController?.ActiveShip.ModelId && preset.Index == _shipDataController?.ActiveShip.CurrentPresetIndex)
                    {
                        dropdownText += " (active)";
                    }
                    return dropdownText;
                },
                indexedShipLoadoutViewObjectList.First());
            // Show the dropdown modal dialog.
            UINewModalDialog.Select(dialogTitle, dialogMessage, dropdownOptions, (int destinationPresetIndex) =>
                {
                    // If cancelled, do nothing.
                    if (destinationPresetIndex < 0)
                    {
                        return;
                    }
                    if (destinationPresetIndex > shipViewObject.Presets.Count - 1)
                    {
                        MuseLog.Info($"ERROR: Player's ShipViewObject.Presets index out of bounds. Got {destinationPresetIndex}. Expected <= {shipViewObject.Presets.Count - 1}. Exiting.");
                        return;
                    }
                    SaveShipLoadout(playerShipId, destinationPresetIndex, shipGunSlotNamesAndGunIds, (response) =>
                    {
                        if (response.Success)
                        {
                            var message = $"Ship loadout copied to {shipModelName} in preset {destinationPresetIndex + 1}.";
                            MuseLog.Info(message);
                            if (_showCopyShipLoadoutNotification)
                            {
                                Util.SendToastNotification(message);
                            }
                        }
                        // Download the changes we just made.
                        _shipDataController.QueryShipsFromServer();
                    });
                }
            );
        }

        /// <summary>
        /// Given a ship model ID, get the player's matching <see cref="ShipViewObject"/> in <see cref="ShipDataController.AllShips"/>.
        /// </summary>
        /// <param name="shipModelId"></param>
        /// <returns><see cref="ShipViewObject"/> or <c>null</c> if not found.</returns>
        private static ShipViewObject GetShipViewObjectByShipModelId(int shipModelId)
        {
            var shipViewObject = _shipDataController.AllShips.Where(ship => ship.ModelId == shipModelId).FirstOrDefault();
            return shipViewObject;
        }

        /// <summary>
        /// Save player ship loadout. Depends on the which game mode (PvP or PvE) the player is in. Saves only the guns and does not modify anything else.
        /// </summary>
        /// <param name="playerShipId">Unique player-ship ID from <see cref="ShipViewObject"/>.</param>
        /// <param name="shipPresetIndex"></param>
        /// <param name="gunSlotNameAndGunIdDictionary"></param>
        /// <param name="responseCallback"></param>
        private static void SaveShipLoadout(int playerShipId, int shipPresetIndex, Dictionary<string, int> gunSlotNameAndGunIdDictionary, Action<ExtensionResponse> responseCallback = null)
        {
            var playerShipIdAndPresetIndexDictionary = new Dictionary<int, int> { { playerShipId, shipPresetIndex } };
            var shipGunsDictionary = CreateShipGunsDictionary(playerShipId, shipPresetIndex, gunSlotNameAndGunIdDictionary);
            var emptyLoadoutNamesDictionary = new Dictionary<Muse.Common.MuseTuple<int, int>, string>();
            var emptySkills = "";
            SubDataActions.CustomizeShips(playerShipId, playerShipIdAndPresetIndexDictionary, shipGunsDictionary, emptyLoadoutNamesDictionary, emptySkills, (response) =>
            {
                responseCallback?.Invoke(response);
            });
        }

        /// <summary>
        /// Helper method for <c>SaveShipLoadout()</c>. Transforms gun data into the required structure for <c>SubDataActions.CustomizeShips()</c>.
        /// </summary>
        /// <param name="playerShipId">Unique player-ship ID from <see cref="ShipViewObject"/>.</param>
        /// <param name="shipPresetIndex"></param>
        /// <param name="gunSlotNameAndGunId"></param>
        /// <returns></returns>
        private static Dictionary<Muse.Common.MuseTuple<int, int, string>, int> CreateShipGunsDictionary(int playerShipId, int shipPresetIndex, Dictionary<string, int> gunSlotNameAndGunId)
        {
            var shipGunsDictionary = new Dictionary<Muse.Common.MuseTuple<int, int, string>, int>();
            foreach (var gunSlotEntry in gunSlotNameAndGunId)
            {
                var gunSlotName = gunSlotEntry.Key;
                var gunId = gunSlotEntry.Value;
                var gunTuple = new Muse.Common.MuseTuple<int, int, string>(playerShipId, shipPresetIndex, gunSlotName);
                shipGunsDictionary.Add(gunTuple, gunId);
            }
            return shipGunsDictionary;
        }

        /// <summary>
        /// Class to hold the index and <see cref="Muse.Goi2.Entity.Vo.ShipViewObject"/> for the dropdown modal dialog.
        /// </summary>
        public class IndexedShipLoadoutViewObject
        {
            public int Index { get; set; }
            public ShipLoadoutViewObject ShipLoadoutViewObject { get; set; }
        }

        // Testing method. Do not use.
        private static void TestSetJunker0ToAllFlares()
        {
            var shipViewObject = _shipDataController.AllShips.Where(ship => ship.Model.NameText.En == "Junker").First();
            var playerShipId = shipViewObject.Id;
            var shipPresetIndex = 0;
            var gunId = 198; // Flare Gun
            var gunSlotNameAndGunIdDictionary = new Dictionary<string, int>();
            for (var gunSlotIndex = 0; gunSlotIndex < shipViewObject.Model.GunSlots; gunSlotIndex++)
            {
                gunSlotNameAndGunIdDictionary.Add($"gun-slot-{gunSlotIndex + 1}", gunId);
            }
            SaveShipLoadout(playerShipId, shipPresetIndex, gunSlotNameAndGunIdDictionary);
        }

        // Testing method. Do not use.
        private static void TestSetJunker0ToAllHarpoons()
        {
            var shipViewObject = _shipDataController.AllShips.Where(ship => ship.Model.NameText.En == "Junker").First();
            var playerShipId = shipViewObject.Id;
            var shipPresetIndex = 0;
            var gunId = 195; // Harpoon Gun
            var gunSlotNameAndGunIdDictionary = new Dictionary<string, int>();
            for (var gunSlotIndex = 0; gunSlotIndex < shipViewObject.Model.GunSlots; gunSlotIndex++)
            {
                gunSlotNameAndGunIdDictionary.Add($"gun-slot-{gunSlotIndex + 1}", gunId);
            }
            SaveShipLoadout(playerShipId, shipPresetIndex, gunSlotNameAndGunIdDictionary);
        }
    }
}