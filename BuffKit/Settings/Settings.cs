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
                if (_entryValues[entry] != value)
                {
                    //log.LogInfo($"Changed value of entry [{entry}] to {value}");
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
            _panelObj = UISettingsPanel.BuildPanel(parentTransform, out _panel);
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
