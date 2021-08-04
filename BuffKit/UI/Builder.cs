using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace BuffKit.UI
{
    public static class Builder
    {
        public static GameObject BuildVerticalScrollView(Transform parent, out GameObject obContent)
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
            //mask.showMaskGraphic = true;
            img = obViewport.AddComponent<Image>();
            //img.color = new Color(1, 0, 0, .5f);

            obContent = new GameObject("Content");
            obContent.transform.SetParent(obViewport.transform);
            rt = obContent.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);
            rt.pivot = new Vector2(0, 1);
            var vlg = obContent.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.LowerRight;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 10;
            //vlg.padding = new RectOffset(10, 10, 10, 10);
            var csf = obContent.AddComponent<ContentSizeFitter>();
            //csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Scroll View
            var obScrollView = new GameObject("Scroll View");
            rt = obScrollView.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);
            obScrollbarVertical.transform.SetParent(obScrollView.transform);
            obViewport.transform.SetParent(obScrollView.transform);
            var scrollRect = obScrollView.AddComponent<ScrollRect>();
            scrollRect.viewport = obViewport.GetComponent<RectTransform>();
            scrollRect.verticalScrollbar = sb;
            scrollRect.content = obContent.GetComponent<RectTransform>();
            scrollRect.horizontal = false;
            scrollRect.scrollSensitivity = 20;
            csf = obScrollView.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            obScrollView.transform.SetParent(parent, false);

            return obScrollView;
        }

        public static GameObject BuildLabel(Transform parent, string text, TextAlignmentOptions alignment = TextAlignmentOptions.Left, int fontSize = 18)
        {
            var obLabel = new GameObject("Label");
            //var le = obLabel.AddComponent<LayoutElement>();
            //le.preferredHeight = 25;
            //le.flexibleWidth = 1;
            var cText = obLabel.AddComponent<TextMeshProUGUI>();
            cText.text = text;
            cText.font = Resources.Font;
            cText.fontSize = fontSize;
            cText.alignment = alignment;
            cText.enableWordWrapping = false;
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

        public static GameObject BuildButton(Transform parent, UnityAction callback, string text, TextAlignmentOptions alignment = TextAlignmentOptions.Center, int fontSize = 18)
        {
            var obButton = new GameObject("Button");

            var obBackground = new GameObject("Background");
            var imgBackground = obBackground.AddComponent<Image>();
            imgBackground.sprite = Resources.BlankIcon;
            var rt = imgBackground.rectTransform;
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);
            obBackground.transform.SetParent(obButton.transform);

            var obLabel = BuildLabel(obButton.transform, text, alignment, fontSize);
            rt = obLabel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(15, 0);
            rt.offsetMax = new Vector2(0, 0);

            var obOutline = new GameObject("Outline");
            var imgOutline = obOutline.AddComponent<Image>();
            imgOutline.sprite = Resources.ButtonOutline;
            imgOutline.type = Image.Type.Sliced;
            rt = imgOutline.rectTransform;
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);
            obOutline.transform.SetParent(obButton.transform);

            var button = obButton.AddComponent<Button>();
            button.onClick.AddListener(callback);
            button.targetGraphic = imgBackground;
            var animator = obButton.AddComponent<Animator>();
            animator.runtimeAnimatorController = Resources.ButtonAnimatorController;
            button.transition = Selectable.Transition.Animation;

            obButton.transform.SetParent(parent, false);

            return obButton;
        }

        public static void TestBuilder(Transform parent)
        {
            var log = BepInEx.Logging.Logger.CreateLogSource("builder");

            var obPanel = BuildPanel(parent);
            var obScrollView = BuildVerticalScrollView(obPanel.transform, out GameObject obContent);
            var obLabel = BuildLabel(obContent.transform, "First label", TextAlignmentOptions.Right, 13);
            for (int i = 0; i < 5; i++)
            {
                var v = i;
                BuildLabel(obContent.transform, $"Label {i + 1}", TextAlignmentOptions.Left, i + 13);
            }
            var button1 = BuildButton(obContent.transform, delegate { log.LogInfo("My First Button callback"); }, "My First Button");
            var le = button1.AddComponent<LayoutElement>();
            le.minHeight = 35;
            var button2 = BuildButton(obContent.transform, delegate { log.LogInfo("My Second Button callback"); }, "My Second Button", TextAlignmentOptions.Left);
            le = button2.AddComponent<LayoutElement>();
            le.minHeight = 35;
            var button3 = BuildButton(obContent.transform, delegate { log.LogInfo("My Third Button callback"); }, "My Third Button", TextAlignmentOptions.Left, 13);
            le = button3.AddComponent<LayoutElement>();
            le.minHeight = 25;
        }

    }
}
