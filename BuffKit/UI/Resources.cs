using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BuffKit.UI
{
    public static class Resources
    {
        public static TMP_FontAsset Font { get; private set; }
        public static Sprite BlankIcon { get; private set; }
        public static Sprite ButtonOutline { get; private set; }
        public static Sprite Checkmark { get; private set; }
        public static Sprite BetaBanner { get; private set; }
        public static RuntimeAnimatorController ButtonAnimatorController { get; private set; }
        public static ColorBlock ScrollBarColors { get; private set; }
        public static ColorBlock TextFieldColors { get; private set; }

        public static void _Initialize()
        {
            var log = BepInEx.Logging.Logger.CreateLogSource("resources");
            Font = GameObject.Find("/Menu UI/Standard Canvas/Menu Header Footer/Footer/Footer Social Toggle Group/Options Button/Label")?.GetComponent<TextMeshProUGUI>()?.font;
            if (Font == null) log.LogWarning("Could not find Font");
            BlankIcon = GameObject.Find("/Menu UI/Standard Canvas/Pages/UI Match List/Browse Match Panel/Match Information Content/Create Button/Background")?.GetComponent<Image>()?.sprite;
            if (BlankIcon == null) log.LogWarning("Could not find BlankIcon");
            ButtonOutline = GameObject.Find("/Menu UI/Standard Canvas/Pages/UI Match List/Browse Match Panel/Match Information Content/Create Button/Outline")?.GetComponent<Image>()?.sprite;
            if (ButtonOutline == null) log.LogWarning("Could not find ButtonOutline");
            Checkmark = GameObject.Find("/Menu UI/Standard Canvas/Pages/UI Match Create/Container/Options Panel/Scroll View/Viewport/Content/Password Option/Password Label Group/Checkbox Item/Checkbox /Checkmark")?.GetComponent<Image>()?.sprite;
            if (Checkmark == null) log.LogWarning("Could not find Checkmark");
            BetaBanner = GameObject.Find("/Game UI/Menu Canvas/UI Prototype Banner/Banner Image")?.GetComponent<Image>()?.sprite;
            if (BetaBanner == null) log.LogWarning("Could not find BetaBanner");
            ButtonAnimatorController = GameObject.Find("/Menu UI/Standard Canvas/Pages/UI Match List/Browse Match Panel/Match Information Content/Create Button")?.GetComponent<Animator>()?.runtimeAnimatorController;
            if (BetaBanner == null) log.LogWarning("Could not find ButtonAnimatorController");
            ScrollBarColors = new ColorBlock
            {
                normalColor = new Color32(0x80, 0x6B, 0x55, 0xFF),
                highlightedColor = new Color32(0xAB, 0x8D, 0x6D, 0xFF),
                pressedColor = new Color32(0x92, 0x7C, 0x64, 0xFF),
                disabledColor = new Color32(0xC8, 0xC8, 0xC8, 0x80),
                fadeDuration = .1f,
                colorMultiplier = 1
            };
            TextFieldColors = new ColorBlock
            {
                normalColor = new Color32(0x80, 0x6B, 0x55, 0xFF),
                highlightedColor = new Color32(0xAB, 0x8D, 0x6D, 0xFF),
                pressedColor = new Color32(0xAB, 0x8D, 0x6D, 0xFF),
                disabledColor = new Color32(0xC8, 0xC8, 0xC8, 0x80),
                fadeDuration = .1f,
                colorMultiplier = 1
            };

            //Builder.TestBuilder(GameObject.Find("/Menu UI/Standard Canvas/Main Menu/Main Screen Elements/Game logo")?.transform);
        }
    }
}
