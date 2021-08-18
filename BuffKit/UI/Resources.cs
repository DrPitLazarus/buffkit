using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Muse.Goi2.Entity;

namespace BuffKit.UI
{
    public static class Resources
    {
        public static TMP_FontAsset FontPenumbraHalfSerifStd { get; private set; }
        public static TMP_FontAsset FontGaldeanoRegular { get; private set; }
        public static Sprite BlankIcon { get; private set; }
        public static Sprite ButtonOutline { get; private set; }
        public static Sprite Checkmark { get; private set; }
        public static Sprite Dropdown { get; private set; }
        public static Sprite BetaBanner { get; private set; }
        public static RuntimeAnimatorController ButtonAnimatorController { get; private set; }
        public static ColorBlock ScrollBarColors { get; private set; }
        public static ColorBlock TextFieldColors { get; private set; }
        public static ColorBlock MenuButtonColors { get; private set; }
        public static Color BackgroundColor { get; private set; }
        public static Color OutlineColor { get; private set; }
        public static Color MenuSelectableInteractable { get; private set; }
        public static Sprite EngineerIcon { get; private set; }
        public static Sprite GunnerIcon { get; private set; }
        public static Sprite PilotIcon { get; private set; }

        public static void _Initialize()
        {
            var log = BepInEx.Logging.Logger.CreateLogSource("resources");
            FontPenumbraHalfSerifStd = GameObject.Find("/Menu UI/Standard Canvas/Menu Header Footer/Footer/Footer Social Toggle Group/Options Button/Label")?.GetComponent<TextMeshProUGUI>()?.font;
            if (FontPenumbraHalfSerifStd == null) log.LogWarning("Could not find Font");
            FontGaldeanoRegular = GameObject.Find("/Menu UI/Standard Canvas/Pages/Library/Book Selection Panel/Text Panel/Body Text")?.GetComponent<TextMeshProUGUI>()?.font;
            if (FontGaldeanoRegular == null) log.LogWarning("Could not find FontGaldeanoRegular");
            BlankIcon = GameObject.Find("/Menu UI/Standard Canvas/Pages/UI Match List/Browse Match Panel/Match Information Content/Create Button/Background")?.GetComponent<Image>()?.sprite;
            if (BlankIcon == null) log.LogWarning("Could not find BlankIcon");
            ButtonOutline = GameObject.Find("/Menu UI/Standard Canvas/Pages/UI Match List/Browse Match Panel/Match Information Content/Create Button/Outline")?.GetComponent<Image>()?.sprite;
            if (ButtonOutline == null) log.LogWarning("Could not find ButtonOutline");
            Checkmark = GameObject.Find("/Menu UI/Standard Canvas/Pages/UI Match Create/Container/Options Panel/Scroll View/Viewport/Content/Password Option/Password Label Group/Checkbox Item/Checkbox /Checkmark")?.GetComponent<Image>()?.sprite;
            if (Checkmark == null) log.LogWarning("Could not find Checkmark");
            Dropdown = GameObject.Find("/Menu UI/Standard Canvas/Pages/UI Match List/Browse Match Panel/Match Information Content/Status Filter/Icon")?.GetComponent<Image>()?.sprite;
            if (Dropdown == null) log.LogWarning("Could not find Dropdown");
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
            MenuButtonColors = new ColorBlock
            {
                normalColor = new Color32(0x80, 0x6B, 0x55, 0xFF),
                highlightedColor = new Color32(0xF0, 0xF0, 0xF0, 0xFF),
                pressedColor = new Color32(0x92, 0x7C, 0x64, 0xFF),
                disabledColor = new Color32(0xC8, 0xC8, 0xC8, 0x80),
                fadeDuration = .1f,
                colorMultiplier = 1
            };

            BackgroundColor = new Color32(0x10, 0x0A, 0x06, 0xFF);
            OutlineColor = new Color32(0xA8, 0x90, 0x79, 0xFF);
            MenuSelectableInteractable = new Color(1, 1, 1, .3f);

            EngineerIcon = GameObject.Find("/Menu UI/Standard Canvas/Pages/UI Match Lobby/Lobby Main Panel/Team Group/Subnav Button Group/Engineer Button/Icon")?.GetComponent<Image>()?.sprite;
            if (EngineerIcon == null) log.LogWarning("Could not find EngineerIcon");
            GunnerIcon = GameObject.Find("/Menu UI/Standard Canvas/Pages/UI Match Lobby/Lobby Main Panel/Team Group/Subnav Button Group/Gunner Button/Icon")?.GetComponent<Image>()?.sprite;
            if (GunnerIcon == null) log.LogWarning("Could not find GunnerIcon");
            PilotIcon = GameObject.Find("/Menu UI/Standard Canvas/Pages/UI Match Lobby/Lobby Main Panel/Team Group/Subnav Button Group/Pilot Button/Icon")?.GetComponent<Image>()?.sprite;
            if (PilotIcon == null) log.LogWarning("Could not find PilotIcon");

            //Builder.TestBuilder(GameObject.Find("/Menu UI/Standard Canvas/Main Menu/Main Screen Elements/Game logo")?.transform);

            Util.Util.OnLobbyLoad += ReloadTextures;
        }

        private static Dictionary<int, Texture2D> _gunTextures;
        private static event Util.Util.Notify _gunTextureLoadCallback;
        public static Texture2D GetGunTexture(int gunId)
        {
            if (!_gunTextures.ContainsKey(gunId)) return UIManager.IconForNullOrEmpty;
            return _gunTextures[gunId];
        }
        public static void RegisterGunTextureCallback(Util.Util.Notify gunTextureLoadCallback)
        {
            _gunTextureLoadCallback -= gunTextureLoadCallback;
            _gunTextureLoadCallback += gunTextureLoadCallback;
        }
        private static Dictionary<int, Texture2D> _skillTextures;
        private static event Util.Util.Notify _skillTextureLoadCallback;
        public static Texture2D GetSkillTexture(int skillId)
        {
            if (!_skillTextures.ContainsKey(skillId)) return UIManager.IconForNullOrEmpty;
            return _skillTextures[skillId];
        }
        public static void RegisterSkillTextureCallback(Util.Util.Notify skillTextureLoadCallback)
        {
            _skillTextureLoadCallback -= skillTextureLoadCallback;
            _skillTextureLoadCallback += skillTextureLoadCallback;
        }
        private static void ReloadTextures()
        {
            _gunTextures = new Dictionary<int, Texture2D>();
            foreach (var id in Util.Util.GunIds)
            {
                var gunItem = CachedRepository.Instance.Get<GunItem>(id);
                MuseBundleStore.Instance.LoadObject<Texture2D>(gunItem.GetIcon(), delegate (Texture2D t)
                {
                    _gunTextures[id] = t;
                    _gunTextureLoadCallback?.Invoke();
                }, 0, false);
            }

            _skillTextures = new Dictionary<int, Texture2D>();
            var allSkills = CachedRepository.Instance.GetAll<SkillConfig>();
            foreach (var sk in allSkills)
            {
                var id = sk.ActivationId;
                MuseBundleStore.Instance.LoadObject<Texture2D>(sk.GetIcon(), delegate (Texture2D t)
                {
                    if (t != null)
                    {
                        _skillTextures[id] = t;
                        _skillTextureLoadCallback?.Invoke();
                    }
                }, 0, false);
            }
        }
    }
}
