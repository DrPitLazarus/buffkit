using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace BuffKit.UI
{
    public static class Builder
    {
        // Fits parent vertically, fits content horizontally
        public static GameObject BuildVerticalScrollViewFitContent(Transform parent, out GameObject obContent)
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
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);

            var obScrollbarVertical = new GameObject("Scrollbar Vertical");
            obSlidingArea.transform.SetParent(obScrollbarVertical.transform, false);
            var le = obScrollbarVertical.AddComponent<LayoutElement>();
            le.minWidth = 10;
            le.preferredWidth = 10;
            le.flexibleHeight = 1;
            var sb = obScrollbarVertical.AddComponent<Scrollbar>();
            sb.handleRect = obHandle.GetComponent<RectTransform>();
            sb.direction = Scrollbar.Direction.BottomToTop;
            sb.image = obHandle.GetComponent<Image>();
            sb.colors = Resources.ScrollBarColors;
            sb.transition = Selectable.Transition.None;         // Doesn't update the colour without this
            sb.transition = Selectable.Transition.ColorTint;

            // Viewport
            var obViewport = new GameObject("Viewport");
            le = obViewport.AddComponent<LayoutElement>();
            le.minHeight = 0;
            var mask = obViewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            img = obViewport.AddComponent<Image>();
            var vlg = obViewport.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;

            obContent = new GameObject("Content");
            obContent.transform.SetParent(obViewport.transform);
            rt = obContent.AddComponent<RectTransform>();
            rt.pivot = new Vector2(0, 1);
            vlg = obContent.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.LowerRight;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 10;

            // Scroll View
            var obScrollView = new GameObject("Scroll View");
            rt = obScrollView.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);
            var hlg = obScrollView.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 5;
            hlg.childForceExpandWidth = false;
            hlg.childControlHeight = true;
            hlg.childControlWidth = true;
            obViewport.transform.SetParent(obScrollView.transform);
            obScrollbarVertical.transform.SetParent(obScrollView.transform);
            var scrollRect = obScrollView.AddComponent<ScrollRect>();
            scrollRect.viewport = obViewport.GetComponent<RectTransform>();
            scrollRect.verticalScrollbar = sb;
            scrollRect.content = obContent.GetComponent<RectTransform>();
            scrollRect.horizontal = false;
            scrollRect.scrollSensitivity = 20;
            //var csf = obScrollView.AddComponent<ContentSizeFitter>();
            //csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            obScrollView.transform.SetParent(parent, false);

            return obScrollView;
        }

        // Fits parent horizontally and vertically
        public static GameObject BuildVerticalScrollViewFitParent(Transform parent, out GameObject obContent)
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
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);

            var obScrollbarVertical = new GameObject("Scrollbar Vertical");
            obSlidingArea.transform.SetParent(obScrollbarVertical.transform, false);
            var le = obScrollbarVertical.AddComponent<LayoutElement>();
            le.minWidth = 10;
            le.preferredWidth = 10;
            le.flexibleHeight = 1;
            var sb = obScrollbarVertical.AddComponent<Scrollbar>();
            sb.handleRect = obHandle.GetComponent<RectTransform>();
            sb.direction = Scrollbar.Direction.BottomToTop;
            sb.image = obHandle.GetComponent<Image>();
            sb.colors = Resources.ScrollBarColors;
            sb.transition = Selectable.Transition.None;         // Doesn't update the colour without this
            sb.transition = Selectable.Transition.ColorTint;

            // Viewport
            var obViewport = new GameObject("Viewport");
            le = obViewport.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;
            le.flexibleHeight = 1;
            le.minHeight = 0;
            var mask = obViewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            img = obViewport.AddComponent<Image>();
            var vlg = obViewport.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.LowerRight;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = true;

            obContent = new GameObject("Content");
            obContent.transform.SetParent(obViewport.transform);
            rt = obContent.AddComponent<RectTransform>();
            rt.pivot = new Vector2(0, 1);
            vlg = obContent.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.LowerRight;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 10;
            var csf = obContent.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Scroll View
            var obScrollView = new GameObject("Scroll View");
            rt = obScrollView.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);
            var hlg = obScrollView.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 5;
            hlg.padding = new RectOffset(10, 10, 10, 10);
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            obViewport.transform.SetParent(obScrollView.transform, false);
            obScrollbarVertical.transform.SetParent(obScrollView.transform, false);
            var scrollRect = obScrollView.AddComponent<ScrollRect>();
            scrollRect.viewport = obViewport.GetComponent<RectTransform>();
            scrollRect.verticalScrollbar = sb;
            scrollRect.content = obContent.GetComponent<RectTransform>();
            scrollRect.horizontal = false;
            scrollRect.scrollSensitivity = 20;
            //csf = obScrollView.AddComponent<ContentSizeFitter>();
            //csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            obScrollView.transform.SetParent(parent, false);

            return obScrollView;
        }

        public static GameObject BuildLabel(Transform parent, string text, TextAnchor alignment = TextAnchor.MiddleLeft, int fontSize = 18)
        {
            var obLabel = new GameObject("Label");
            var hlg = obLabel.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childAlignment = alignment;

            var obText = new GameObject("Text");
            var cText = obText.AddComponent<TextMeshProUGUI>();
            cText.text = text;
            cText.font = Resources.Font;
            cText.fontSize = fontSize;
            cText.enableWordWrapping = false;
            obText.transform.SetParent(obLabel.transform, false);
            var le = obText.AddComponent<LayoutElement>();
            le.minHeight = fontSize;

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

        public static GameObject BuildButton(Transform parent, UnityAction callback, string text, TextAnchor alignment = TextAnchor.MiddleCenter, int fontSize = 18)
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

        public static GameObject BuildInputField(Transform parent, int fontSize = 13)
        {
            var obInputField = new GameObject("Input Field");
            var hlg = obInputField.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(5, 5, 5, 5);
            var le = obInputField.AddComponent<LayoutElement>();
            le.minHeight = fontSize + 10;
            le.preferredHeight = fontSize + 10;

            var obTextArea = new GameObject("Text Area");
            obTextArea.AddComponent<RectMask2D>();
            obTextArea.transform.SetParent(obInputField.transform, false);
            var rt = obTextArea.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);

            var obText = new GameObject("Text");
            var cText = obText.AddComponent<TextMeshProUGUI>();
            cText.text = "";
            cText.font = Resources.Font;
            cText.fontSize = fontSize;
            cText.enableWordWrapping = false;
            obText.transform.SetParent(obTextArea.transform, false);
            le = obText.AddComponent<LayoutElement>();
            le.minHeight = fontSize;
            rt = obText.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);

            var imgBackground = obInputField.AddComponent<Image>();
            imgBackground.sprite = Resources.BlankIcon;
            imgBackground.color = new Color32(0xFF, 0xFF, 0xFF, 0x80);
            var field = obInputField.AddComponent<TMP_InputField>();
            field.textComponent = cText;
            field.textViewport = obTextArea.GetComponent<RectTransform>();
            field.onFocusSelectAll = false;
            field.transition = Selectable.Transition.ColorTint;
            field.colors = Resources.TextFieldColors;

            obInputField.transform.SetParent(parent, false);
            return obInputField;
        }

        public static void TestBuilder(Transform parent)
        {
            var log = BepInEx.Logging.Logger.CreateLogSource("builder");

            var obPanel = BuildPanel(parent);
            var hlg = obPanel.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(5, 5, 5, 5);
            var le = obPanel.AddComponent<LayoutElement>();
            le.preferredHeight = 150;
            // Auto-fit scrollview to panel
            //var obScrollView = BuildVerticalScrollViewFitParent(obPanel.transform, out GameObject obContent);
            // Auto-fit panel to scrollview (horizontally)
            obPanel.GetComponent<RectTransform>().pivot = new Vector2(.5f, 1f);
            var csf = obPanel.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            var obScrollView = BuildVerticalScrollViewFitContent(obPanel.transform, out GameObject obContent);

            BuildInputField(obContent.transform);

            var obLabel = BuildLabel(obContent.transform, "First label", TextAnchor.MiddleRight, 13);
            BuildLabel(obContent.transform, "My much longer label", TextAnchor.MiddleCenter, 15);
            for (int i = 0; i < 5; i++)
            {
                var v = i;
                BuildLabel(obContent.transform, $"Label {i + 1}", TextAnchor.MiddleLeft, i + 13);
            }
            var button1 = BuildButton(obContent.transform, delegate { log.LogInfo("My First Button callback"); }, "My First Button");
            le = button1.AddComponent<LayoutElement>();
            le.minHeight = 35;
            var button2 = BuildButton(obContent.transform, delegate { log.LogInfo("My Second Button callback"); }, "My Second Button", TextAnchor.MiddleLeft);
            le = button2.AddComponent<LayoutElement>();
            le.minHeight = 35;
            var button3 = BuildButton(obContent.transform, delegate { log.LogInfo("My Third Button callback"); }, "My Third Button", TextAnchor.MiddleLeft, 13);
            le = button3.AddComponent<LayoutElement>();
            le.minHeight = 25;
        }

    }
}
