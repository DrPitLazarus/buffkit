using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Muse.Goi2.Entity;
using Newtonsoft.Json;

namespace BuffKit.Settings
{
    public class Settings
    {
        public static Settings Instance { get; private set; }
        public static void Initialize()
        {
            Instance = new Settings();
            Instance.log = BepInEx.Logging.Logger.CreateLogSource("settings");
            Instance.CreatePanel();
            Instance.LoadFromFile();
            UI.Resources.RegisterSkillTextureCallback(() => Instance._icon.texture = UI.Resources.GetSkillTexture(16));
            Instance.log.LogInfo("Settings initialized");
            Util.Util.OnSettingsInitializeTrigger();
        }

        private BepInEx.Logging.ManualLogSource log;

        private Dictionary<string, BaseSettingType> _entries = new Dictionary<string, BaseSettingType>();

        public void AddEntry<T>(string header, string entry, Action<T> callback, T defaultValue)
        {
            bool callCallback = false;
            if (!_entries.ContainsKey(entry))
            {
                log.LogInfo($"Adding entry: {entry}");
                if (AddEntryElement<T>(header.ToUpper(), entry, entry, defaultValue, out callCallback))
                {
                    SaveToFile();
                }
            }
            else if (!(_entries[entry] is SettingType<T>))
                throw new InvalidCastException($"Attempted to add entry {entry} callback with type {typeof(T)} but a different type is already assigned");
            var currentEntry = _entries[entry] as SettingType<T>;
            currentEntry.AddCallback(callback);
            var currentValue = currentEntry.GetValue();
            if (!currentEntry.IsSameValue(defaultValue) || callCallback)
            {
                callback?.Invoke(currentValue);
            }
        }
        public void SetEntry<T>(string entry, T value)
        {
            if (!_entries.ContainsKey(entry))
            {
                return;
            }
            var setting = _entries[entry] as SettingType<T>;
            if (setting == null)
            {
                log.LogInfo($"Setting {entry} of type {typeof(T)} not found");
                return;
            }
            var result = setting.SetValue(value);
            if (result)
            {
                SaveToFile();
                setting.InvokeCallbacks();
            }
        }

        // Returns true if the entry should be saved to file
        private bool AddEntryElement<T>(string header, string entry, string text, object value, out bool callCallback)
        {
            bool r = true;
            callCallback = false;

            Transform parent = _panel.GetHeaderContent(header);
            BaseSettingType settingEntry;
            if (typeof(T) == typeof(bool))
                settingEntry = new SettingToggle(parent, entry, text, (bool)value);
            else if (typeof(T) == typeof(Dummy))
                settingEntry = new SettingButton(parent, entry, text);
            else if (typeof(T) == typeof(ToggleGrid))
                settingEntry = new SettingToggleGrid(parent, entry, text, (ToggleGrid)value);
            else if (typeof(T) == typeof(EnumString))
                settingEntry = new SettingEnumString(parent, entry, text, (EnumString)value);
            else
                throw new NotSupportedException($"No entry type exists for {typeof(T)}");

            if (_loadedSettings.ContainsKey(entry))
            {
                var loadedValue = _loadedSettings[entry];
                if (settingEntry.IsCompatible(loadedValue))
                {
                    if (settingEntry.SetUnknownValue(loadedValue))
                    {
                        callCallback = true;
                    }
                    r = false;
                }
                _loadedSettings.Remove(entry);
            }

            _panel.AddSetting(settingEntry, entry);
            _entries.Add(entry, settingEntry);

            return r;
        }

        // Convenience function for adding a button entry (no need to use Dummy)
        public void AddEntry(string header, string entry, Action callback)
        {
            AddEntry<Dummy>(header, entry, delegate { callback(); }, null);
        }

        private UISettingsPanel _panel;
        private RawImage _icon;
        private void CreatePanel()
        {
            // Font
            var font = GameObject.Find("/Menu UI/Standard Canvas/Menu Header Footer/Footer/Footer Social Toggle Group/Options Button/Label")?.GetComponent<TextMeshProUGUI>()?.font;
            if (font == null) log.LogError("Font not found");


            // Settings panel
            var parentTransform = GameObject.Find("/Menu UI/Standard Canvas/Common Elements")?.transform;
            if (parentTransform == null) log.LogError("Panel parent transform was not found");
            UISettingsPanel.BuildPanel(parentTransform, out _panel);
            _panel.SetVisibility(false);
            if (_panel == null) log.LogError("Panel is null");


            // Settings button
            var buttonParent = GameObject.Find("/Menu UI/Standard Canvas/Menu Header Footer/Footer/Footer Social Toggle Group")?.transform;
            if (buttonParent == null) log.LogError("Button parent transform was not found");

            var obSettingsButtonGroup = new GameObject("BuffKit Settings");
            var im = obSettingsButtonGroup.AddComponent<Image>();               // Invisible image to make the button clicking area cover the whole rect
            im.color = new Color(0, 0, 0, 0);
            var hlg = obSettingsButtonGroup.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 7;
            hlg.childForceExpandHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            var obSettingsIcon = new GameObject("Icon");
            obSettingsIcon.transform.SetParent(obSettingsButtonGroup.transform, false);
            _icon = obSettingsIcon.AddComponent<RawImage>();
            var le = obSettingsIcon.AddComponent<LayoutElement>();
            le.preferredWidth = 25;
            le.preferredHeight = 25;

            var obSettingsLabel = new GameObject("Label");
            obSettingsLabel.transform.SetParent(obSettingsButtonGroup.transform, false);
            obSettingsLabel.transform.localPosition = new Vector3(30, 0, 0);
            var label = obSettingsLabel.AddComponent<TextMeshProUGUI>();
            label.text = "BuffKit";
            label.fontSize = 13;
            label.alignment = TextAlignmentOptions.Center;
            label.font = font;

            obSettingsButtonGroup.transform.SetParent(buttonParent, false);

            var button = obSettingsButtonGroup.AddComponent<Button>();
            button.onClick.AddListener(delegate { _panel.ToggleVisibility(); });
            button.transition = Selectable.Transition.ColorTint;
            button.colors = UI.Resources.ScrollBarColors;
            button.targetGraphic = _icon;
        }

        private enum DataType
        {
            Invalid,
            Bool,
            ToggleGrid,
            Button,
            EnumString
        }
        private static DataType GetDataType(object data)
        {
            if (data is bool)
                return DataType.Bool;
            if (data is Dummy)
                return DataType.Button;
            if (data is ToggleGrid)
                return DataType.ToggleGrid;
            if (data is EnumString)
                return DataType.EnumString;
            return DataType.Invalid;
        }

        [Serializable]
        class SerializableEntry
        {
            public string Entry { get; private set; }
            public DataType Type { get; private set; }
            public string Data { get; private set; }
            public SerializableEntry(string entry, object data)
            {
                Entry = entry;

                Type = GetDataType(data);
                switch (Type)
                {
                    case DataType.Bool:
                        Data = JsonConvert.SerializeObject(data);
                        break;
                    case DataType.ToggleGrid:
                        Data = JsonConvert.SerializeObject(data);
                        break;
                    case DataType.EnumString:
                        Type = DataType.EnumString;
                        Data = JsonConvert.SerializeObject(data);
                        break;
                    case DataType.Button:
                        Type = DataType.Invalid;
                        Data = "";
                        break;
                    case DataType.Invalid:
                        Data = "";
                        break;
                }
            }
            [JsonConstructor]
            public SerializableEntry(string entry, DataType type, string data)
            {
                Entry = entry;
                Type = type;
                Data = data;
            }
            public override string ToString() { return JsonConvert.SerializeObject(this); }
        }

        public void SaveToFile()
        {
            var data = new List<SerializableEntry>();

            // Add active entries
            foreach (var kvp in _entries)
            {
                var entry = kvp.Key;
                var entryValue = kvp.Value;

                var serializableEntry = new SerializableEntry(entry, entryValue.GetUnknownValue());
                if (serializableEntry.Type != DataType.Invalid)
                {
                    data.Add(serializableEntry);
                }
            }
            // Add inactive entries
            foreach (var kvp in _loadedSettings)
            {
                var entry = kvp.Key;
                var entryValue = kvp.Value;

                var serializableEntry = new SerializableEntry(entry, entryValue);
                if (serializableEntry.Type != DataType.Invalid)
                {
                    data.Add(serializableEntry);
                }
            }
            // Stringify data
            var dataString = JsonConvert.SerializeObject(data, Formatting.Indented);

            var filePath = @"BepInEx\plugins\BuffKit\settings.json";
            var gp = Directory.GetCurrentDirectory();
            var path = Path.Combine(gp, filePath);
            File.WriteAllText(path, dataString);

            log.LogInfo("Saved settings to file");
        }

        private Dictionary<string, object> _loadedSettings;
        public void LoadFromFile()
        {
            _loadedSettings = new Dictionary<string, object>();

            try
            {
                var filePath = @"BepInEx\plugins\BuffKit\settings.json";
                var gp = Directory.GetCurrentDirectory();
                var path = Path.Combine(gp, filePath);
                var savedData = File.ReadAllText(path);
                var data = JsonConvert.DeserializeObject<List<SerializableEntry>>(savedData);
                foreach (var d in data)
                {
                    switch (d.Type)
                    {
                        case DataType.Bool:
                            _loadedSettings.Add(d.Entry, JsonConvert.DeserializeObject<bool>(d.Data));
                            break;
                        case DataType.ToggleGrid:
                            _loadedSettings.Add(d.Entry, JsonConvert.DeserializeObject<ToggleGrid>(d.Data));
                            break;
                        case DataType.EnumString:
                            _loadedSettings.Add(d.Entry, JsonConvert.DeserializeObject<EnumString>(d.Data));
                            break;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                log.LogInfo("Settings file was not found");
            }
            catch (JsonReaderException e)
            {
                log.LogInfo($"Failed to read settings file:\n{e.Message}");
            }
        }
    }
}
