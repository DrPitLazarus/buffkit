using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

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
        public SettingToggle(Transform parent, string entry, string text, bool value)
        {
            _value = value;
            UI.Builder.BuildMenuToggle(parent, out _toggle, text, _value,
                delegate (bool v) { Settings.Instance.SetEntry(entry, v); });
        }
        public override bool SetValue(bool value)
        {
            if (value != _value)
            {
                _value = value;
                _toggle.isOn = value;
                return true;
            }
            return false;
        }
        public override bool IsSameValue(bool value) { return _value == value; }
        public override bool IsCompatible(object value) { return (value is bool); }
    }

    public class Dummy { }
    public class SettingButton : SettingType<Dummy>
    {
        public SettingButton(Transform parent, string entry, string text)
        {
            UI.Builder.BuildMenuButton(parent, entry, text,
                delegate { Settings.Instance.SetEntry<Dummy>(entry, null); });
        }
        public override bool SetValue(Dummy value) { return true; }
        public override bool IsSameValue(Dummy value) { return true; }
        public override bool IsCompatible(object value) { return false; }
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
            var s = new System.Text.StringBuilder();
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
        public SettingToggleGrid(Transform parent, string entry, string title, ToggleGrid value)
        {
            _value = value;

            LayoutElement le;

            _toggles = new Toggle[value.Rows, value.Cols];

            UI.Builder.BuildMenuDropdown(parent, title, out var obContent);
            var vlg = obContent.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 1;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(3, 3, 3, 3);

            var obGridIconBar = new GameObject("icon bar");
            obGridIconBar.transform.SetParent(obContent.transform, false);
            var hlg = obGridIconBar.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 1;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            foreach (var s in value.Icons)
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
                le.preferredHeight = 25;
                le.preferredWidth = 25;
            }
            var obSpacer = new GameObject("spacer");
            obSpacer.transform.SetParent(obGridIconBar.transform, false);
            le = obSpacer.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;

            for (var r = 0; r < value.Rows; r++)
            {
                // Add row object
                var currentLabel = value.Labels[r];
                var obRow = new GameObject($"row {currentLabel}");
                obRow.transform.SetParent(obContent.transform, false);
                hlg = obRow.AddComponent<HorizontalLayoutGroup>();
                hlg.spacing = 1;
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = false;
                hlg.childAlignment = TextAnchor.MiddleLeft;
                for (var c = 0; c < value.Cols; c++)
                {
                    // Add toggle button
                    var row = r;
                    var col = c;
                    UI.Builder.BuildMenuToggle(obRow.transform, out var toggle, value.Values[r, c],
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
                UI.Builder.BuildLabel(obRow.transform, currentLabel, TextAnchor.MiddleLeft, 13);
            }
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
    }
}
