using System;
using System.Collections.Generic;
using System.IO;
using BuffKit.UI;
using Muse.Goi2.Entity;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BuffKit.ShipLoadoutNotes
{
    public class ShipLoadoutNotes : MonoBehaviour
    {
        private static readonly string _name = "Ship Loadout Notes";
        private static readonly string _announceButtonText = "Announce to Crew";
        private static readonly string _saveButtonText = "Save";
        private static readonly string _jsonFilePath = @"BepInEx\plugins\BuffKit\shipLoadoutNotes.json";
        private static readonly string _jsonFullFilePath = Path.Combine(Directory.GetCurrentDirectory(), _jsonFilePath);

        public static bool IsInitialized { get { return _shipLoadoutNotesObject != null; } }
        public static bool NoteInputFieldIsFocused { get { return _noteInputField?.isFocused ?? false; } }

        private static TMP_InputField _noteInputField;
        private static GameObject _shipLoadoutNotesObject;
        private static GameObject _announceToCrewButtonObject;
        private static GameObject _saveButtonObject;
        private static List<ShipLoadoutNoteData> _allNotes = [];
        private static int _currentNoteIndex = -1;
        private static bool _isCaptain = false;

        [Serializable]
        private struct ShipLoadoutNoteData
        {
            public GameType gameType;
            public int shipModelId;
            public int presetIndex;
            public string note;
        }

        #region Settings

        public static bool ReadFromFile()
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
            var data = GetCurrentGameData();
            _currentNoteIndex = GetNoteIndex(data);

            if (_currentNoteIndex == -1)
            {
                MuseLog.Info("Note missing! Adding a new note.");
                _allNotes.Add(data);
            }
            else
            {
                MuseLog.Info("Note found! Updating existing note.");
                _allNotes[_currentNoteIndex] = data;
            }

            SaveToFile();
        }

        public static void LoadNote()
        {
            var data = GetCurrentGameData();
            _currentNoteIndex = GetNoteIndex(data);

            if (_currentNoteIndex == -1)
            {
                MuseLog.Info("Note missing! Clearing the input field.");
                _noteInputField.text = "";
            }
            else
            {
                MuseLog.Info("Note found! Updating the input field.");
                _noteInputField.text = _allNotes[_currentNoteIndex].note;
            }

            _isCaptain = NetworkedPlayer.Local.IsCaptain;
        }

        private static ShipLoadoutNoteData GetCurrentGameData()
        {
            return new ShipLoadoutNoteData
            {
                gameType = NetworkedPlayer.Local.GameType,
                shipModelId = UIShipCustomizationScreen.Instance.currentShip.ModelId,
                presetIndex = UIShipCustomizationScreen.Instance.currentShip.CurrentPresetIndex,
                note = _noteInputField.text
            };
        }

        private static int GetNoteIndex(ShipLoadoutNoteData data)
        {
            return _allNotes.FindIndex(note => note.gameType.Equals(data.gameType) && note.shipModelId.Equals(data.shipModelId) && note.presetIndex.Equals(data.presetIndex));
        }

        #endregion

        #region UI

        public static void Initialize()
        {
            var recommendedLoadoutObj = GameObject.Find("/Menu UI/Standard Canvas/Pages/UI Profile Ship/Content/Recommended Loadout Group/");
            _shipLoadoutNotesObject = GameObject.Find("/Menu UI/Standard Canvas/Pages/UI Profile Ship/Content/Recommended Loadout Group/" + _name);
            if (_shipLoadoutNotesObject == null)
            {
                ReadFromFile();
                BuildUi(recommendedLoadoutObj.transform);
                // Update the recommended loadout obj to put the new ship loadout notes in the right place.
                LayoutRebuilder.ForceRebuildLayoutImmediate(recommendedLoadoutObj.GetComponent<RectTransform>());
            }
        }

        private void FixedUpdate()
        {
            // Show announce to crew button only if they are the captain and input field is not empty.
            var isAllowedToAnnounce = _isCaptain && _noteInputField.text.Trim().Length > 0;
            _announceToCrewButtonObject.SetActive(isAllowedToAnnounce);
            // Show save button only when the saved note is not the same as input field.
            var noteString = _currentNoteIndex == -1 ? "" : _allNotes[_currentNoteIndex].note;
            _saveButtonObject.SetActive(noteString != _noteInputField.text);
        }

        public static void BuildUi(Transform parent)
        {
            var mainObj = new GameObject(_name);
            mainObj.transform.SetParent(parent, false);
            var vlg = mainObj.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(5, 0, 8, 5);
            vlg.spacing = 3;
            vlg.childAlignment = TextAnchor.UpperRight;

            // Panel title
            var panelLabelObj = Builder.BuildLabel(mainObj.transform, _name, TextAnchor.MiddleRight, 13);

            // Text box
            var inputObj = Builder.BuildInputField(mainObj.transform);
            inputObj.name = "Note Input Field";
            inputObj.GetComponent<LayoutElement>().minHeight = 80; // 5 lines of text (10 padding + lines + (13 fontsize * lines))
            _noteInputField = inputObj.GetComponent<TMP_InputField>();
            _noteInputField.fontAsset = UI.Resources.FontGaldeanoRegular;
            _noteInputField.lineType = TMP_InputField.LineType.MultiLineNewline;
            _noteInputField.restoreOriginalTextOnEscape = false;
            _noteInputField.richText = false;
            _noteInputField.scrollSensitivity = 3;
            _noteInputField.enabled = false; // Needed to make text input work when drawn for the first time.
            _noteInputField.enabled = true;

            // Button row
            var buttonRowObj = new GameObject("Button Row");
            buttonRowObj.transform.SetParent(mainObj.transform, false);
            var buttonRowRt = buttonRowObj.AddComponent<RectTransform>();
            buttonRowRt.localScale = new Vector3(1, 1, 1); // I have no idea why it's not 1...
            var buttonRowHlg = buttonRowObj.AddComponent<HorizontalLayoutGroup>();
            buttonRowHlg.childForceExpandHeight = false;
            buttonRowHlg.childForceExpandWidth = false;
            buttonRowHlg.spacing = 3;

            // Annouce to crew button
            _announceToCrewButtonObject = Builder.BuildButton(buttonRowObj.transform, AnnouceToCrewChat, _announceButtonText, fontSize: 13);
            _announceToCrewButtonObject.name = $"{_announceButtonText} Button";
            var annouceToCrewButtonLe = _announceToCrewButtonObject.AddComponent<LayoutElement>();
            annouceToCrewButtonLe.minHeight = 30;
            annouceToCrewButtonLe.preferredWidth = 200;
            annouceToCrewButtonLe.flexibleWidth = 100;

            // Save button
            _saveButtonObject = Builder.BuildButton(buttonRowObj.transform, SaveNote, _saveButtonText, fontSize: 13);
            _saveButtonObject.name = $"{_saveButtonText} Button";
            var saveButtonLe = _saveButtonObject.AddComponent<LayoutElement>();
            saveButtonLe.minHeight = 30;
            saveButtonLe.minWidth = 60;
            saveButtonLe.flexibleWidth = 200;

            mainObj.AddComponent<ShipLoadoutNotes>();
        }

        private static void AnnouceToCrewChat()
        {
            // Chatbox does not see \r\n as a newline.
            var note = _noteInputField.text.Replace("\r", "").Trim();
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