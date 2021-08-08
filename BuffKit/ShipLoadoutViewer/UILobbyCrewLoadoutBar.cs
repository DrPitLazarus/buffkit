using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Muse.Goi2.Entity;

namespace BuffKit.ShipLoadoutViewer
{
    class UILobbyCrewLoadoutBar : MonoBehaviour
    {
        public bool MarkForRedraw { set; get; }
        public static GameObject Build(Transform parent, out UILobbyCrewLoadoutBar loadoutBar)
        {
            var obBar = new GameObject("Loadout Bar");

            var hlg = obBar.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.spacing = 1;

            var images = new List<RawImage>();
            for (var i = 0; i < 6; i++)
            {
                var slot = new GameObject($"slot{i}");
                var im = slot.AddComponent<RawImage>();
                im.enabled = false;
                images.Add(im);
                var le = slot.AddComponent<LayoutElement>();
                le.preferredWidth = 21;
                le.preferredHeight = 21;
                slot.transform.SetParent(obBar.transform);
            }

            loadoutBar = obBar.AddComponent<UILobbyCrewLoadoutBar>();
            loadoutBar._loadoutImages = images;

            var img = obBar.AddComponent<Image>();
            img.color = new Color32(0xD8, 0xC8, 0xB1, 0x3C);

            var btn = obBar.AddComponent<Button>();
            btn.colors = new ColorBlock
            {
                normalColor = new Color32(0xFF, 0xFF, 0xFF, 0x82),
                highlightedColor = new Color32(0xFF, 0xFF, 0xFF, 0xFF),
                pressedColor = new Color32(0xFF, 0xFF, 0xFF, 0x82),
                disabledColor = new Color32(0xFF, 0xFF, 0xFF, 0x82),
                colorMultiplier = 1,
                fadeDuration = .1f
            };
            btn.interactable = false;                   // Switch to disabled colour (and prevent highlighting)

            obBar.transform.SetParent(parent);
            return obBar;
        }

        class PlayerLoadoutData
        {
            List<int> _loadoutIds;
            public PlayerLoadoutData(UserAvatarEntity player)
            {
                _loadoutIds = new List<int>();
                if (player != null)
                {
                    foreach (var skill in player.CurrentSkills)
                    {
                        var sc = CachedRepository.Instance.Get<SkillConfig>(skill);
                        if (sc.Type == SkillType.Gun || sc.Type == SkillType.Helm || sc.Type == SkillType.Repair)
                        {
                            _loadoutIds.Add(skill);
                        }
                    }
                }
            }

            public int GetVisibleSlots() { return _loadoutIds.Count; }

            public Texture GetSlotTexture(int slot)
            {
                int skillId = _loadoutIds[slot];
                if (skillId == -1) return UIManager.IconForNullOrEmpty;
                if (!ShipLoadoutViewer.skillIcons.ContainsKey(skillId)) return UIManager.IconForNullOrEmpty;
                return ShipLoadoutViewer.skillIcons[skillId];
            }

            public override string ToString()
            {
                StringBuilder b = new StringBuilder();
                b.Append("Loadout: ");
                foreach (var i in _loadoutIds)
                {
                    var sc = CachedRepository.Instance.Get<SkillConfig>(i);
                    b.Append($"  {sc.NameText.En}");
                }
                return b.ToString();
            }
        }

        void Update()
        {
            if (MarkForRedraw)
            {
                DisplayItemsFromData(_loadoutDataLast);
                MarkForRedraw = false;
            }
        }

        private List<RawImage> _loadoutImages;
        private PlayerLoadoutData _loadoutDataLast = new PlayerLoadoutData(null);

        public void DisplayItems(UserAvatarEntity player)
        {
            _loadoutDataLast = new PlayerLoadoutData(player);
            DisplayItemsFromData(_loadoutDataLast);
        }

        private void DisplayItemsFromData(PlayerLoadoutData data)
        {
            for (int i = 0; i < _loadoutDataLast.GetVisibleSlots(); i++)
            {
                _loadoutImages[i].enabled = true;
                _loadoutImages[i].texture = _loadoutDataLast.GetSlotTexture(i);
            }
            for (int i = _loadoutDataLast.GetVisibleSlots(); i < _loadoutImages.Count; i++)
                _loadoutImages[i].enabled = false;
        }
    }
}
