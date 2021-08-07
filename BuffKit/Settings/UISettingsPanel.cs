using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BuffKit.Settings
{
    class UISettingsPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private GameObject _content;

        public static GameObject BuildPanel(Transform parent, out UISettingsPanel panel)
        {
            var obPanel = UI.Builder.BuildPanel(parent);
            obPanel.name = "UI Settings Panel";
            var rt = obPanel.GetComponent<RectTransform>();
            rt.pivot = new Vector2(1, 0);
            rt.anchorMin = new Vector2(.763f, .046f);
            rt.anchorMax = new Vector2(1f, .5f);
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);
            var hlg = obPanel.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(5, 5, 5, 5);
            var csf = obPanel.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            panel = obPanel.AddComponent<UISettingsPanel>();
            obPanel.AddComponent<GraphicRaycaster>();                       // Makes it have UI interaction on top of other UI (I think? It's complicated)

            UI.Builder.BuildVerticalScrollViewFitContent(obPanel.transform, out panel._content);

            return obPanel;
        }

        private Dictionary<string, int> _entryIndex = new Dictionary<string, int>();
        private List<UISettingsEntry> _entryList = new List<UISettingsEntry>();


        public void AddEntry(string entry, bool value)
        {
            if (_entryIndex.ContainsKey(entry)) return;
            int i = _entryIndex.Count;
            _entryIndex.Add(entry, i);

            var box = UISettingsEntry.Build(_content.transform);
            box.Text = entry;
            box.Value = value;
            _entryList.Add(box);
        }
        public void SetEntry(string entry, bool value)
        {
            if (!_entryIndex.ContainsKey(entry)) return;
            int i = _entryIndex[entry];
            _entryList[i].Value = value;
        }

        private bool _remakeList = false;
        public void RemoveEntry(string entry)
        {
            if (!_entryIndex.ContainsKey(entry)) return;
            Destroy(_entryList[_entryIndex[entry]].gameObject);
            _remakeList = true;
        }

        private bool _pointerInFrame = false;
        private float _timeUntilDisappear = 0;
        public float disappearTimer = 2;
        private bool _isVisible = false;

        private void Update()
        {
            if (_remakeList)
            {
                _entryIndex = new Dictionary<string, int>();
                var newList = new List<UISettingsEntry>();
                foreach(var uise in _entryList)
                {
                    if(uise!=null)
                    {
                        _entryIndex.Add(uise.Text, newList.Count);
                        newList.Add(uise);
                    }
                }
                _entryList = newList;
                _remakeList = false;
            }
            if(_isVisible && !_pointerInFrame)
            {
                if(_timeUntilDisappear > 0)
                {
                    _timeUntilDisappear -= Time.deltaTime;
                }
                if (_timeUntilDisappear <= 0)
                    SetVisibility(false);
                    
            }
        }

        public void SetVisibility(bool visible)
        {
            _isVisible = visible;
            gameObject.SetActive(_isVisible);
            if (_isVisible)
            {
                foreach (var v in _entryList)
                    v.ResetAlignment();
                _timeUntilDisappear = disappearTimer;
            }
        }
        public void ToggleVisibility()
        {
            SetVisibility(!_isVisible);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _pointerInFrame = true;
            _timeUntilDisappear = disappearTimer;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _pointerInFrame = false;
        }
    }
}
