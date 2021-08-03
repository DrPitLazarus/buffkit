using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BuffKit.Settings
{
    class UISettingsEntry : MonoBehaviour
    {
        private TextMeshProUGUI _label;
        private Toggle _toggle;
        public string Text
        {
            set { _label.text = value; }
            get { return _label.text; }
        }
        public bool Value
        {
            set { _toggle.isOn = value; }
            get { return _toggle.isOn; }
        }

        public static UISettingsEntry Build(Transform parent, Sprite tick, Sprite background, TMP_FontAsset font)
        {
            GameObject go = new GameObject("Settings Entry");
            go.transform.SetParent(parent, false);
            var uise = go.AddComponent<UISettingsEntry>();
            uise.Build(tick, background, font);
            return uise;
        }

        private void Build(Sprite tick, Sprite background, TMP_FontAsset font)
        {
            var hlg = gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.spacing = 10;
            hlg.padding = new RectOffset(3, 3, 3, 3);
            hlg.childAlignment = TextAnchor.MiddleLeft;

            var childBox = new GameObject("Box");
            var le = childBox.AddComponent<LayoutElement>();
            le.preferredWidth = 25;
            le.preferredHeight = 25;
            childBox.transform.SetParent(transform, false);
            var childBoxImg = childBox.AddComponent<Image>();
            childBoxImg.sprite = background;
            childBoxImg.color = new Color(1, 1, 1, .45f);

            var checkmark = new GameObject("Checkmark");
            checkmark.transform.SetParent(childBox.transform, false);
            var checkmarkImg = checkmark.AddComponent<Image>();
            checkmarkImg.sprite = tick;
            var rt = checkmarkImg.rectTransform;
            rt.anchorMin = new Vector2(.5f, .5f);
            rt.anchorMax = new Vector2(.5f, .5f);
            rt.sizeDelta = new Vector2(20, 20);

            var childLabel = new GameObject("Label");
            le = childLabel.AddComponent<LayoutElement>();
            le.preferredHeight = 25;
            le.flexibleWidth = 1;
            childLabel.transform.SetParent(transform, false);
            _label = childLabel.AddComponent<TextMeshProUGUI>();
            _label.text = "";
            _label.fontSize = 13;
            _label.font = font;
            _label.alignment = TextAlignmentOptions.Left;

            _toggle = gameObject.AddComponent<Toggle>();
            _toggle.graphic = checkmarkImg;
            _toggle.targetGraphic = childBoxImg;
            _toggle.onValueChanged.AddListener(delegate (bool value) { Settings.Instance.SetEntry(_label.text, value); });

            var i = gameObject.AddComponent<Image>();
            i.color = new Color32(0xA8, 0x90, 0x79, 0x4E);
        }

        public void ResetAlignment()
        {
            _label.alignment = TextAlignmentOptions.Left;
        }
    }
}
