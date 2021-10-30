using System.Collections.Generic;
using BuffKit.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BuffKit.Settings
{
    class UISettingsPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private GameObject _content;

        public static GameObject BuildPanel(Transform parent, out UISettingsPanel panel)
        {
            var obPanel = Builder.BuildPanel(parent);
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
            var le = obPanel.AddComponent<LayoutElement>();
            le.minWidth = 350;

            panel = obPanel.AddComponent<UISettingsPanel>();
            obPanel.AddComponent<GraphicRaycaster>();                       // Makes it have UI interaction on top of other UI (I think? It's complicated)

            Builder.BuildVerticalScrollViewFitContent(obPanel.transform, out panel._content);

            return obPanel;
        }

        private Dictionary<string, int> _entryIndex = new Dictionary<string, int>();
        private List<BaseSettingType> _entryList = new List<BaseSettingType>();
        private Dictionary<string, Transform> _headers = new Dictionary<string, Transform>();

        //public Transform GetContent() { return _content.transform; }
        public void AddSetting(BaseSettingType setting, string entry)
        {
            _entryIndex.Add(entry, _entryList.Count);
            _entryList.Add(setting);
        }

        public Transform GetHeaderContent(string header)
        {
            if(!_headers.ContainsKey(header))
            {
                Builder.BuildMenuDropdown(_content.transform, header, out var obContent);
                var vlg = obContent.AddComponent<VerticalLayoutGroup>();
                vlg.spacing = 3;
                vlg.childForceExpandHeight = false;
                vlg.padding = new RectOffset(30, 0, 2, 0);
                _headers.Add(header, obContent.transform);
            }
            return _headers[header];
        }

        private bool _pointerInFrame = false;
        private float _timeUntilDisappear = 0;
        public float disappearTimer = 2;
        private bool _isVisible = false;

        private void Update()
        {
            if (_isVisible && !_pointerInFrame)
            {
                if (_timeUntilDisappear > 0)
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
