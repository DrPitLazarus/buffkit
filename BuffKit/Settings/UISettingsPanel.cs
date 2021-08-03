using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BuffKit.Settings
{
    class UISettingsPanel : MonoBehaviour
    {
        private Sprite _checkbox;
        private Sprite _checkmark;
        private TMP_FontAsset _font;
        private GameObject _content;

        public void Initialize(BepInEx.Logging.ManualLogSource log, TMP_FontAsset font)
        {
            var rt = gameObject.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(.763f, .046f);
            rt.anchorMax = new Vector2(1f, .5f);
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);
            var bg = gameObject.AddComponent<Image>();
            bg.color = new Color32(0x10, 0x0A, 0x06, 0xFF);
            var outline = gameObject.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = new Color32(0xA8, 0x90, 0x79, 0xFF);

            _checkbox = GameObject.Find("/Menu UI/Standard Canvas/Pages/UI Profile Character/Character Content/Loadout Group/Golden Checkbox/Background")?.GetComponent<Image>()?.sprite;
            _checkmark = GameObject.Find("/Menu UI/Standard Canvas/Pages/UI Profile Character/Character Content/Loadout Group/Golden Checkbox/Background/Checkmark")?.GetComponent<Image>()?.sprite;
            if (_checkbox == null) log.LogError("Checkbox not found");
            if (_checkmark == null) log.LogError("Checkmark not found");
            _font = font;

            gameObject.AddComponent<GraphicRaycaster>();            // Makes it have UI interaction on top of other UI (I think? It's complicated)

            // Scrollbar
            var obHandle = new GameObject("Handle");
            var img = obHandle.AddComponent<Image>();
            img.color = new Color(1, 1, 1, 1);
            rt = img.rectTransform;
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);

            var obSlidingArea = new GameObject("Sliding Area");
            obHandle.transform.SetParent(obSlidingArea.transform, false);
            rt = obSlidingArea.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(-5, 10);
            rt.offsetMax = new Vector2(-5, -10);

            var obScrollbarVertical = new GameObject("Scrollbar Vertical");
            obSlidingArea.transform.SetParent(obScrollbarVertical.transform, false);
            rt = obScrollbarVertical.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(-10, 0);
            rt.offsetMax = new Vector2(0, 0);
            var sb = obScrollbarVertical.AddComponent<Scrollbar>();
            sb.handleRect = obHandle.GetComponent<RectTransform>();
            sb.direction = Scrollbar.Direction.BottomToTop;
            sb.image = obHandle.GetComponent<Image>();
            var colorBlock = new ColorBlock
            {
                normalColor = new Color32(0x80, 0x6B, 0x55, 0xFF),
                highlightedColor = new Color32(0xAB, 0x8D, 0x6D, 0xFF),
                pressedColor = new Color32(0x92, 0x7C, 0x64, 0xFF),
                disabledColor = new Color32(0xC8, 0xC8, 0xC8, 0x80),
                fadeDuration = .1f,
                colorMultiplier = 1
            };
            sb.colors = colorBlock;
            sb.transition = Selectable.Transition.None;         // Doesn't update the colour without this
            sb.transition = Selectable.Transition.ColorTint;

            // Viewport
            var obViewport = new GameObject("Viewport");
            rt = obViewport.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(10, 10);
            rt.offsetMax = new Vector2(-20, -10);
            var mask = obViewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            obViewport.AddComponent<Image>();

            _content = new GameObject("Content");
            _content.transform.SetParent(obViewport.transform);
            var vlg = _content.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.LowerRight;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 10;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            var csf = _content.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            rt = _content.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0, 1);

            // Scroll View
            var scrollView = new GameObject("Scroll View");
            rt = scrollView.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);
            obScrollbarVertical.transform.SetParent(scrollView.transform);
            obViewport.transform.SetParent(scrollView.transform);
            var scrollRect = scrollView.AddComponent<ScrollRect>();
            scrollRect.viewport = obViewport.GetComponent<RectTransform>();
            scrollRect.verticalScrollbar = sb;
            scrollRect.content = _content.GetComponent<RectTransform>();
            scrollRect.horizontal = false;
            scrollRect.scrollSensitivity = 20;

            scrollView.transform.SetParent(transform, false);
        }

        private Dictionary<string, int> _entryIndex = new Dictionary<string, int>();
        private List<UISettingsEntry> _entryList = new List<UISettingsEntry>();


        public void AddEntry(string entry, bool value)
        {
            if (_entryIndex.ContainsKey(entry)) return;
            int i = _entryIndex.Count;
            _entryIndex.Add(entry, i);

            var box = UISettingsEntry.Build(_content.transform, _checkmark, _checkbox, _font);
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
        }

        public void ToggleVisibility()
        {
            var vis = !gameObject.activeSelf;
            gameObject.SetActive(vis);
            if (vis)
                foreach (var v in _entryList)
                    v.ResetAlignment();
        }
    }
}
