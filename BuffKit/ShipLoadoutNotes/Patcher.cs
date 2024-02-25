using System;
using System.Collections.Generic;
using System.IO;
using BuffKit.UI;
using HarmonyLib;
using Muse.Goi2.Entity;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BuffKit.ShipLoadoutNotes
{
    [HarmonyPatch]
    public class ShipLoadoutNotes
    {
        private static readonly string _name = "Ship Loadout Notes";
        private static readonly string _announceButtonText = "Announce to Crew";
        private static readonly string _saveButtonText = "Save";
        private static readonly string _jsonFilePath = @"BepInEx\plugins\BuffKit\shipLoadoutNotes.json";
        private static readonly string _jsonFullFilePath = Path.Combine(Directory.GetCurrentDirectory(), _jsonFilePath);
        private static TMP_InputField _inputField;
        private static GameObject _announceToCrewButtonObj;
        private static GameObject _saveButtonObj;
        private static List<ShipLoadoutNoteData> _allNotes = [];

        #region Settings
        [Serializable]
        private struct ShipLoadoutNoteData
        {
            public GameType gameType;
            public int shipModelId;
            public int presetIndex;
            public string note;
        }

        private static bool ReadFromFile()
        {
            try
            {
                var file = File.ReadAllText(_jsonFullFilePath);
                _allNotes = JsonConvert.DeserializeObject<List<ShipLoadoutNoteData>>(file);

                MuseLog.Info($"Read {_allNotes.Count} notes from file!");

                return true;
            }
            catch (FileNotFoundException)
            {
                MuseLog.Info("JSON file not found.");
            }
            catch (JsonReaderException e)
            {
                MuseLog.Info($"Failed to read JSON file:\n{e.Message}");
            }
            catch (JsonSerializationException e)
            {
                MuseLog.Info($"Failed to deserialise JSON file:\n{e.Message}");
            }
            return false;
        }

        private static void SaveToFile()
        {
            var dataString = JsonConvert.SerializeObject(_allNotes, Formatting.Indented);
            File.WriteAllText(_jsonFullFilePath, dataString);

            MuseLog.Info($"Saved {_allNotes.Count} notes to file!");
        }

        private static void SaveNote()
        {
            var data = GetCurrentNoteData();
            var index = GetNoteIndex(data);

            if (index == -1)
            {
                MuseLog.Info("Note missing! Adding a new note.");
                _allNotes.Add(data);
            }
            else
            {
                MuseLog.Info("Note found! Updating existing note.");
                _allNotes[index] = data;
            }

            SaveToFile();
        }

        private static void LoadNote()
        {
            var data = GetCurrentNoteData();
            var index = GetNoteIndex(data);

            if (index == -1)
            {
                MuseLog.Info("Note missing! Clearing the input field.");
                _inputField.text = "";
            }
            else
            {
                MuseLog.Info("Note found! Updating the input field.");
                _inputField.text = _allNotes[index].note;
            }

            // Show announce to crew button only if they are the captain.
            _announceToCrewButtonObj.SetActive(NetworkedPlayer.Local.IsCaptain);
        }

        private static ShipLoadoutNoteData GetCurrentNoteData()
        {
            return new ShipLoadoutNoteData
            {
                gameType = NetworkedPlayer.Local.GameType,
                shipModelId = UIShipCustomizationScreen.Instance.currentShip.ModelId,
                presetIndex = UIShipCustomizationScreen.Instance.currentShip.CurrentPresetIndex,
                note = _inputField.text
            };
        }

        private static int GetNoteIndex(ShipLoadoutNoteData data)
        {
            return _allNotes.FindIndex(note => note.gameType.Equals(data.gameType) && note.shipModelId.Equals(data.shipModelId) && note.presetIndex.Equals(data.presetIndex));
        }

        #endregion

        #region Patching and UI

        [HarmonyPatch(typeof(UIShipCustomizationScreen), "SetActiveShip")]
        private static void Postfix()
        {
            // Called when entering the ship customization screen and when a ship loadout is selected.
            var recommendedLoadoutObj = GameObject.Find("/Menu UI/Standard Canvas/Pages/UI Profile Ship/Content/Recommended Loadout Group/");
            var shipLoadoutNotesObj = GameObject.Find("/Menu UI/Standard Canvas/Pages/UI Profile Ship/Content/Recommended Loadout Group/" + _name);
            if (shipLoadoutNotesObj == null)
            {
                ReadFromFile();
                BuildPanel(recommendedLoadoutObj.transform);
                // Update the recommended loadout obj to put the new ship loadout notes in the right place.
                LayoutRebuilder.ForceRebuildLayoutImmediate(recommendedLoadoutObj.GetComponent<RectTransform>());
            }
            LoadNote();
        }

        [HarmonyPatch(typeof(UIShipCustomizationScreen), "UpdateCamera")]
        private static bool Prefix()
        {
            // Prevent ship preview camera from moving when the text box is focused.
            return !_inputField?.isFocused ?? true;
        }

        private static void BuildPanel(Transform parent)
        {
            var panelObj = new GameObject(_name);
            panelObj.transform.SetParent(parent, false);
            var vlg = panelObj.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(5, 0, 8, 5);
            vlg.spacing = 3;
            vlg.childAlignment = TextAnchor.UpperRight;

            // Panel title
            var panelLabelObj = Builder.BuildLabel(panelObj.transform, _name, TextAnchor.MiddleRight, 13);

            // Text box
            var inputObj = Builder.BuildInputField(panelObj.transform);
            inputObj.GetComponent<LayoutElement>().minHeight = 80; // 5 lines of text (10 padding + lines + (13 fontsize * lines))
            _inputField = inputObj.GetComponent<TMP_InputField>();
            _inputField.fontAsset = UI.Resources.FontGaldeanoRegular;
            _inputField.lineType = TMP_InputField.LineType.MultiLineNewline;
            _inputField.restoreOriginalTextOnEscape = false;
            _inputField.richText = false;
            _inputField.scrollSensitivity = 3;
            _inputField.enabled = false; // Needed to make text input work when drawn for the first time.
            _inputField.enabled = true;

            // Button row
            var buttonRowObj = new GameObject("Button Row");
            buttonRowObj.transform.SetParent(panelObj.transform);
            var buttonRowRt = buttonRowObj.AddComponent<RectTransform>();
            buttonRowRt.localScale = new Vector3(1, 1, 1); // I have no idea why it's not 1...
            var buttonRowHlg = buttonRowObj.AddComponent<HorizontalLayoutGroup>();
            buttonRowHlg.childForceExpandHeight = false;
            buttonRowHlg.childForceExpandWidth = false;
            buttonRowHlg.spacing = 3;

            // Annouce to crew button
            _announceToCrewButtonObj = Builder.BuildButton(buttonRowObj.transform, AnnouceToCrewChat, _announceButtonText, fontSize: 13);
            var annouceToCrewButtonLe = _announceToCrewButtonObj.AddComponent<LayoutElement>();
            annouceToCrewButtonLe.minHeight = 30;
            annouceToCrewButtonLe.preferredWidth = 200;

            // Save button
            _saveButtonObj = Builder.BuildButton(buttonRowObj.transform, SaveNote, _saveButtonText, fontSize: 13);
            var saveButtonLe = _saveButtonObj.AddComponent<LayoutElement>();
            saveButtonLe.minHeight = 30;
            saveButtonLe.minWidth = 60;
            saveButtonLe.flexibleWidth = 200;
        }

        private static void AnnouceToCrewChat()
        {
            // Chatbox does not see \r\n as a newline.
            var note = _inputField.text.Replace("\r", "").Trim();
            // Get player names from non-pilot slots.
            var slot1Name = NetworkedPlayer.Local.CrewEntity.Slots[1].PlayerEntity?.Name ?? "<slot1>";
            var slot2Name = NetworkedPlayer.Local.CrewEntity.Slots[2].PlayerEntity?.Name ?? "<slot2>";
            var slot3Name = NetworkedPlayer.Local.CrewEntity.Slots[3].PlayerEntity?.Name ?? "<slot3>";
            // Remove platform tag from name.
            slot1Name = slot1Name.Replace(" [PC]", "").Replace(" [PS]", "");
            slot2Name = slot2Name.Replace(" [PC]", "").Replace(" [PS]", "");
            slot3Name = slot3Name.Replace(" [PC]", "").Replace(" [PS]", "");
            // Replace placeholders with names.
            note = note.Replace("<slot1>", slot1Name).Replace("<slot2>", slot2Name).Replace("<slot3>", slot3Name);

            if (note.Length == 0) return;

            if (note.Length <= 490)
            {
                Util.ForceSendMessage(note, "crew");
                return;
            }

            // Note is longer than 490 characters, split at double newline. If no double newline, note is cut off.
            var noteParts = note.Split(new string[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var notePart in noteParts)
            {
                Util.ForceSendMessage(notePart, "crew");
            }
        }

        #endregion
    }
}