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
        private List<BaseSettingType> _entryList = new List<BaseSettingType>();

        public Transform GetContent() { return _content.transform; }
        public void AddSetting(BaseSettingType setting, string entry)
        {
            _entryIndex.Add(entry, _entryList.Count);
            _entryList.Add(setting);
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
