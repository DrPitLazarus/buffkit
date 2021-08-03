using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

        public delegate void SettingsChanged(bool newSetting);
        private Dictionary<string, bool> _entryValues = new Dictionary<string, bool>();
        private Dictionary<string, List<Action<bool>>> _entryCallbacks = new Dictionary<string, List<Action<bool>>>();

        public void AddEntry(string entry, Action<bool> callback, bool entryValue = false)
        {
            // entryValue is only used if the entry is not yet set
            if (!_entryCallbacks.ContainsKey(entry))
            {
                var actionList = new List<Action<bool>>();
                actionList.Add(callback);
                _entryCallbacks[entry] = actionList;
                _entryValues[entry] = entryValue;
                log.LogInfo($"Added entry [{entry}]");
                _panel.AddEntry(entry, entryValue);
            }
            else
            {
                _entryCallbacks[entry].Add(callback);
                log.LogInfo($"Added callback to entry [{entry}]");
            }
        }

        public void SetEntry(string entry, bool value)
        {
            if (_entryValues.ContainsKey(entry))
            {
                if(_entryValues[entry] != value)
                {
                    log.LogInfo($"Changed value of entry [{entry}] to {value}");
                    _entryValues[entry] = value;
                    foreach (var action in _entryCallbacks[entry])
                        if (action != null)
                            action(value);
                    _panel.SetEntry(entry, value);
                }
            }
        }

        private GameObject _panelObj;
        private UISettingsPanel _panel;
        private void CreatePanel()
        {
            // Font
            var font = GameObject.Find("/Menu UI/Standard Canvas/Menu Header Footer/Footer/Footer Social Toggle Group/Options Button/Label")?.GetComponent<TextMeshProUGUI>()?.font;
            if (font == null) log.LogError("Font not found");


            // Settings panel
            var parentTransform = GameObject.Find("/Menu UI/Standard Canvas/Common Elements")?.transform;
            if (parentTransform == null) log.LogError("Panel parent transform was not found");
            _panelObj = new GameObject("UISettingsPanel");
            _panel = _panelObj.AddComponent<UISettingsPanel>();
            _panelObj.transform.SetParent(parentTransform, false);
            _panel.Initialize(log, font);
            _panelObj.SetActive(false);


            // Settings button
            var buttonParent = GameObject.Find("/Menu UI/Standard Canvas/Menu Header Footer/Footer/Footer Social Toggle Group")?.transform;
            if (buttonParent == null) log.LogError("Button parent transform was not found");

            var settingsButtonGroup = new GameObject("BuffKit Settings");
            var le = settingsButtonGroup.AddComponent<LayoutElement>();
            le.preferredWidth = 90;
            le.preferredHeight = 25;
            var settingsIcon = new GameObject("Icon");
            settingsIcon.transform.SetParent( settingsButtonGroup.transform, false);
            settingsIcon.transform.localPosition = new Vector3(30, 0, 0);
            var icon = settingsIcon.AddComponent<Image>();
            icon.color = new Color32(0xC0, 0x30, 0x80, 0xFF);
            var rt = icon.rectTransform;
            rt.sizeDelta = new Vector2(25, 25);
            rt.anchorMin = new Vector2(0, .5f);
            rt.anchorMax = new Vector2(0, .5f);
            var settingsLabel = new GameObject("Label");
            settingsLabel.transform.SetParent(settingsButtonGroup.transform, false);
            settingsLabel.transform.localPosition = new Vector3(30, 0, 0);
            var label = settingsLabel.AddComponent<TextMeshProUGUI>();
            label.text = "BuffKit";
            label.fontSize = 13;
            label.alignment = TextAlignmentOptions.Center;
            label.font = font;

            settingsButtonGroup.transform.SetParent(buttonParent, false);

            var button = settingsButtonGroup.AddComponent<Button>();
            button.onClick.AddListener(delegate { _panel.ToggleVisibility(); });
        }

    }
}
