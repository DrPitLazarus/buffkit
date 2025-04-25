using System;
using System.Collections.Generic;
using System.Text;
using BuffKit.UI;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace BuffKit.Settings
{

    public abstract class BaseSettingType
    {
        // Used when loading settings from file
        public abstract bool IsCompatible(object value);
        // Used when loading settings from file
        public abstract bool SetUnknownValue(object value);
        // Used when saving settings to value
        public abstract object GetUnknownValue();
        // Used when creating menu entry
        public abstract void CreateUIElement(Transform parent, string entry);
    }
    public abstract class SettingType<T> : BaseSettingType
    {
        private List<Action<T>> _callbacks = new List<Action<T>>();
        public void AddCallback(Action<T> callback)
        {
            _callbacks.Add(callback);
        }
        public void InvokeCallbacks()
        {
            foreach (var c in _callbacks)
                c?.Invoke(_value);
        }
        protected T _value;
        public T GetValue() { return _value; }
        public override object GetUnknownValue() { return GetValue(); }
        // Used when loading settings from file
        // Returns true if the setting was changed, false otherwise
        public override bool SetUnknownValue(object value)
        {
            if (value is T)
                return SetValue((T)value);
            return false;
        }
        // Called by Settings when anything calls to change the value
        // Return true if listeners should be invoked, false otherwise
        public abstract bool SetValue(T value);
        // Used to determine if a new callback should be invoked with the current settings value
        public abstract bool IsSameValue(T value);
    }

    public class SettingToggle : SettingType<bool>
    {
        private Toggle _toggle;
        public SettingToggle(bool value)
        {
            _value = value;
        }
        public override bool SetValue(bool value)
        {
            if (value != _value)
            {
                _value = value;
                if (_toggle != null)
                    _toggle.isOn = value;
                return true;
            }
            return false;
        }
        public override bool IsSameValue(bool value) { return _value == value; }
        public override bool IsCompatible(object value) { return (value is bool); }
        public override void CreateUIElement(Transform parent, string entry)
        {
            // Add ability to change display text from internal entry name. Use zero or one slash!
            // Example: "feature one/enable" will display as "ENABLE"
            // Example: "feature two enable" will display as "FEATURE TWO ENABLE"
            var entrySplit = entry.Split('/');
            var entryDisplay = entrySplit[entrySplit.Length - 1];
            Builder.BuildMenuToggle(parent, out _toggle, entryDisplay, _value,
                delegate (bool v) { Settings.Instance.SetEntry(entry, v); });
        }
    }

    public class Dummy { }
    public class SettingButton : SettingType<Dummy>
    {
        public SettingButton() { }
        public override bool SetValue(Dummy value) { return true; }
        public override bool IsSameValue(Dummy value) { return true; }
        public override bool IsCompatible(object value) { return false; }
        public override void CreateUIElement(Transform parent, string entry)
        {
            // Add ability to change display text from internal entry name. Use zero or one slash!
            // Example: "feature one/enable" will display as "ENABLE"
            // Example: "feature two enable" will display as "FEATURE TWO ENABLE"
            var entrySplit = entry.Split('/');
            var entryDisplay = entrySplit[entrySplit.Length - 1];
            Builder.BuildMenuButton(parent, entryDisplay, entryDisplay,
                delegate { Settings.Instance.SetEntry<Dummy>(entry, null); });
        }
    }

    [Serializable]
    public class ToggleGrid
    {
        public int Cols { get; private set; }
        public int Rows { get; private set; }
        [JsonIgnore]
        public List<Sprite> Icons { get; private set; }
        [JsonIgnore]
        public List<string> Labels { get; private set; }
        public bool[,] Values { get; private set; }
        public ToggleGrid(List<Sprite> colIcons, List<string> rowLabels, bool defaultValue = false)
        {
            Cols = colIcons.Count;
            Rows = rowLabels.Count;
            Icons = colIcons;
            Labels = rowLabels;
            Values = new bool[Rows, Cols];
            for (var r = 0; r < Rows; r++)
                for (var c = 0; c < Cols; c++)
                    Values[r, c] = defaultValue;
        }
        public ToggleGrid(ToggleGrid other)
        {
            Rows = other.Rows;
            Cols = other.Cols;
            Values = other.Values.Clone() as bool[,];
        }
        [JsonConstructor]
        public ToggleGrid(int cols, int rows, bool[,] values)
        {
            Cols = cols;
            Rows = rows;
            Values = values;
        }
        public void SetValues(bool[,] values)
        {
            var rows = values.GetLength(0);
            var cols = values.GetLength(1);
            if (rows != Rows && cols != Cols) throw new IndexOutOfRangeException($"Values {rows}x{cols} was of incorrect size - expected {Rows}x{Cols}");
            Values = values;
        }
        public void SetValue(int row, int col, bool value) { Values[row, col] = value; }
        public bool GetValue(int row, int col) { return Values[row, col]; }
        public bool IsEqual(ToggleGrid other)
        {
            for (var r = 0; r < Rows; r++)
                for (var c = 0; c < Cols; c++)
                    if (Values[r, c] != other.Values[r, c]) return false;
            return true;
        }
        public override string ToString()
        {
            var s = new StringBuilder();
            for (var r = 0; r < Rows; r++)
            {
                s.Append("\n");
                for (var c = 0; c < Cols; c++)
                    s.Append($"\t{Values[r, c]}");
            }
            return s.ToString();
        }
    }
    public class SettingToggleGrid : SettingType<ToggleGrid>
    {
        private Toggle[,] _toggles;
        public SettingToggleGrid(ToggleGrid value)
        {
            _value = value;
        }
        public override bool SetValue(ToggleGrid value)
        {
            if (value.Cols != _value.Cols || value.Rows != _value.Rows)
                return false;
            bool didChange = false;
            for (var r = 0; r < value.Rows; r++)
                for (var c = 0; c < value.Cols; c++)
                {
                    var newVal = value.GetValue(r, c);
                    if (_value.GetValue(r, c) != newVal)
                    {
                        _value.SetValue(r, c, newVal);
                        if (_toggles != null)
                            _toggles[r, c].isOn = newVal;
                        didChange = true;
                    }
                }
            return didChange;
        }
        public override bool IsSameValue(ToggleGrid value) { return _value.IsEqual(value); }
        public override bool IsCompatible(object value)
        {
            if (value is ToggleGrid)
            {
                var v = value as ToggleGrid;
                return (v.Rows == _value.Rows && v.Cols == _value.Cols);
            }
            return false;
        }
        public override void CreateUIElement(Transform parent, string entry)
        {
            LayoutElement le;

            _toggles = new Toggle[_value.Rows, _value.Cols];

            // Add ability to change display text from internal entry name. Use zero or one slash!
            // Example: "feature one/enable" will display as "ENABLE"
            // Example: "feature two enable" will display as "FEATURE TWO ENABLE"
            var entrySplit = entry.Split('/');
            var entryDisplay = entrySplit[entrySplit.Length - 1];
            Builder.BuildMenuDropdown(parent, entryDisplay, out var obContent);
            var vlg = obContent.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 1;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(30, 0, 2, 0);

            var obGridIconBar = new GameObject("icon bar");
            obGridIconBar.transform.SetParent(obContent.transform, false);
            var hlg = obGridIconBar.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 1;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            foreach (var s in _value.Icons)
            {
                var obIcon = new GameObject("icon");
                obIcon.transform.SetParent(obGridIconBar.transform, false);
                hlg = obIcon.AddComponent<HorizontalLayoutGroup>();
                hlg.padding = new RectOffset(3, 3, 3, 3);
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = false;
                var obIconBox = new GameObject("box");
                obIconBox.transform.SetParent(obIcon.transform, false);
                var im = obIconBox.AddComponent<Image>();
                im.sprite = s;
                le = obIconBox.AddComponent<LayoutElement>();
                le.minWidth = 25;
                le.minHeight = 25;
                le.preferredWidth = 25;
                le.preferredHeight = 25;
            }
            var obSpacer = new GameObject("spacer");
            obSpacer.transform.SetParent(obGridIconBar.transform, false);
            le = obSpacer.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;

            for (var r = 0; r < _value.Rows; r++)
            {
                // Add row object
                var currentLabel = _value.Labels[r];
                var obRow = new GameObject($"row {currentLabel}");
                obRow.transform.SetParent(obContent.transform, false);
                hlg = obRow.AddComponent<HorizontalLayoutGroup>();
                hlg.spacing = 1;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = false;
                hlg.childAlignment = TextAnchor.MiddleLeft;
                for (var c = 0; c < _value.Cols; c++)
                {
                    // Add toggle button
                    var row = r;
                    var col = c;
                    Builder.BuildMenuToggle(obRow.transform, out var toggle, _value.Values[r, c],
                        delegate (bool v)
                        {
                            var newSettings = new ToggleGrid(_value);
                            newSettings.SetValue(row, col, v);
                            Settings.Instance.SetEntry(entry, newSettings);
                        }
                    );
                    _toggles[r, c] = toggle;
                }
                // Add label
                Builder.BuildLabel(obRow.transform, currentLabel, TextAnchor.MiddleLeft, 13);
            }
        }
    }

    public class EnumString
    {
        [JsonIgnore]
        public int Count { get; private set; }
        public int SelectedValue { get; set; }
        [JsonIgnore]
        private List<string> _enumNames;
        [JsonIgnore]
        private List<int> _enumValues;
        public EnumString(Type enumType, int value, Dictionary<int, string> enumNames = null)
        {
            _enumNames = new List<string>();
            _enumValues = new List<int>();
            var values = Enum.GetValues(enumType);
            bool valueValid = false;
            foreach (var v in values)
            {
                var vAsInt = (int)v;
                _enumValues.Add(vAsInt);
                var enumName = v.ToString();
                if (enumNames != null)
                {
                    if (enumNames.ContainsKey(vAsInt))
                    {
                        enumName = enumNames[vAsInt];
                    }
                }
                _enumNames.Add(enumName);
                if (vAsInt == value)
                    valueValid = true;
            }
            if (!valueValid) throw new ArgumentOutOfRangeException($"Initial value {value} is invalid");
            SelectedValue = value;
            Count = _enumNames.Count;
        }
        [JsonConstructor]
        public EnumString(int value) { SelectedValue = value; }
        public bool IsValidValue(int v) { return _enumValues.Contains(v); }
        public int GetEnumValue(int index) { return _enumValues[index]; }
        public string GetEnumName(int index) { return _enumNames[index]; }
        public int GetSelectedIndex() { return _enumValues.IndexOf(SelectedValue); }
    }
    public class SettingEnumString : SettingType<EnumString>
    {
        private List<Toggle> _toggles;
        public SettingEnumString(EnumString value)
        {
            _value = value;
        }
        public override bool IsCompatible(object value)
        {
            if (value is EnumString)
            {
                return _value.IsValidValue((value as EnumString).SelectedValue);
            }
            return false;
        }
        public override bool IsSameValue(EnumString value)
        {
            return (_value.SelectedValue == value.SelectedValue);
        }
        public override bool SetValue(EnumString value)
        {
            if (IsSameValue(value))
            {
                return false;
            }
            _value.SelectedValue = value.SelectedValue;
            var ind = _value.GetSelectedIndex();
            if (_toggles != null)
                for (var i = 0; i < _toggles.Count; i++)
                    _toggles[i].isOn = (i == ind);
            return true;
        }
        public override void CreateUIElement(Transform parent, string entry)
        {
            _toggles = new List<Toggle>();

            // Add ability to change display text from internal entry name. Use zero or one slash!
            // Example: "feature one/enable" will display as "ENABLE"
            // Example: "feature two enable" will display as "FEATURE TWO ENABLE"
            var entrySplit = entry.Split('/');
            var entryDisplay = entrySplit[entrySplit.Length - 1];
            Builder.BuildMenuDropdown(parent, entryDisplay, out var obContent);
            var vlg = obContent.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 3;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(30, 0, 2, 0);

            var toggleGroup = obContent.AddComponent<ToggleGroup>();
            toggleGroup.allowSwitchOff = true;

            for (var i = 0; i < _value.Count; i++)
            {
                var val = _value.GetEnumValue(i);
                var name = _value.GetEnumName(i);
                Builder.BuildMenuToggle(obContent.transform, out var toggle, name, false, delegate (bool v)
                {
                    if (v)
                        Settings.Instance.SetEntry(entry, new EnumString(val));
                });
                toggle.isOn = (val == _value.SelectedValue);
                toggle.group = toggleGroup;
                _toggles.Add(toggle);
            }

            toggleGroup.allowSwitchOff = false;
        }
    }
}
