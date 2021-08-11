using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Muse.Goi2.Entity;

namespace BuffKit.Settings
{
    public class Settings
    {
        public static Settings Instance { get; private set; }
        public static void _Initialize()
        {
            Instance = new Settings();
            Instance.log = BepInEx.Logging.Logger.CreateLogSource("settings");
            Instance.CreatePanel();
            Instance.log.LogInfo("Settings initialized");
        }

        private BepInEx.Logging.ManualLogSource log;

        private Dictionary<string, BaseSettingType> _entries = new Dictionary<string, BaseSettingType>();

        public void AddEntry<T>(string entry, Action<T> callback, T defaultValue)
        {
            if (!_entries.ContainsKey(entry))
            {
                AddEntryElement<T>(entry, entry, defaultValue);
            }
            else if (!(_entries[entry] is SettingType<T>))
                throw new InvalidCastException($"Attempted to add entry {entry} callback with type {typeof(T)} but a different type is already assigned");
            var currentEntry = _entries[entry] as SettingType<T>;
            currentEntry.AddCallback(callback);
            var currentValue = currentEntry.GetValue();
            if (!currentEntry.IsSameValue(defaultValue))
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
                setting.InvokeCallbacks();
            }
        }

        public void AddEntryElement<T>(string entry, string text, object value)
        {
            Transform parent = _panel.GetContent();
            BaseSettingType settingEntry = null;
            if (typeof(T) == typeof(bool))
                settingEntry = new SettingToggle(parent, entry, text, (bool)value);
            else if (typeof(T) == typeof(Dummy))
                settingEntry = new SettingButton(parent, entry, text);
            else if (typeof(T) == typeof(ToggleGrid))
                settingEntry = new SettingToggleGrid(parent, entry, text, (ToggleGrid)value);
            else
                throw new NotSupportedException($"No entry type exists for {typeof(T)}");

            _panel.AddSetting(settingEntry, entry);
            _entries.Add(entry, settingEntry);
        }

        // Concenience function for adding a button entry (no need to use Dummy)
        public void AddEntry(string entry, Action callback)
        {
            AddEntry<Dummy>(entry, delegate { callback(); }, null);
        }


        private UISettingsPanel _panel;
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
            var icon = obSettingsIcon.AddComponent<RawImage>();
            var le = obSettingsIcon.AddComponent<LayoutElement>();
            le.preferredWidth = 25;
            le.preferredHeight = 25;
            MuseBundleStore.Instance.LoadObject<Texture2D>(CachedRepository.Instance.Get<SkillConfig>(16).GetIcon(), delegate (Texture2D t)
            {
                icon.texture = t;
            }, 0, false);

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
            button.targetGraphic = icon;
        }

    }
}
