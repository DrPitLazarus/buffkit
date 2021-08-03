using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BuffKit.UI
{
    public static class Builder
    {
        public static GameObject BuildScrollView(Transform parent, out GameObject obContent)
        {
            // Scrollbar
            var obHandle = new GameObject("Handle");
            var img = obHandle.AddComponent<Image>();
            img.color = new Color(1, 1, 1, 1);
            var rt = img.rectTransform;
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

            obContent = new GameObject("Content");
            obContent.transform.SetParent(obViewport.transform);
            var vlg = obContent.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.LowerRight;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 10;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            var csf = obContent.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            rt = obContent.GetComponent<RectTransform>();
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
            scrollRect.content = obContent.GetComponent<RectTransform>();
            scrollRect.horizontal = false;
            scrollRect.scrollSensitivity = 20;

            scrollView.transform.SetParent(parent, false);

            return scrollView;
        }

        public static GameObject BuildLabel(Transform parent, string text, int fontSize = 18)
        {
            var obLabel = new GameObject("Label");
            //var le = obLabel.AddComponent<LayoutElement>();
            //le.preferredHeight = 25;
            //le.flexibleWidth = 1;
            var cText = obLabel.AddComponent<TextMeshProUGUI>();
            cText.text = text;
            cText.font = Resources.Font;
            cText.fontSize = fontSize;
            cText.alignment = TextAlignmentOptions.Left;
            obLabel.transform.SetParent(parent, false);
            return obLabel;
        }

        public static GameObject BuildPanel(Transform parent)
        {
            var obPanel = new GameObject("Panel");
            var img = obPanel.AddComponent<Image>();
            img.color = new Color32(0x10, 0x0A, 0x06, 0xFF);
            var rt = img.rectTransform;
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);
            var outline = obPanel.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = new Color32(0xA8, 0x90, 0x79, 0xFF);
            obPanel.transform.SetParent(parent, false);
            return obPanel;
        }

    }
}
