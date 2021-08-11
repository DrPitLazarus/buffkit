using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Muse.Goi2.Entity;

namespace BuffKit.ShipLoadoutViewer
{
    class UILobbyCrewLoadoutBar : MonoBehaviour
    {
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
            public PlayerLoadoutData(UserAvatarEntity player, bool[,] showTools)
            {
                var _pilotIds = new List<int>();
                var _gunnerIds = new List<int>();
                var _engineerIds = new List<int>();
                _loadoutIds = new List<int>();
                if (player != null)
                {
                    int column = 0;
                    switch (player.CurrentClass)
                    {
                        case AvatarClass.Pilot:
                            column = 0;
                            break;
                        case AvatarClass.Gunner:
                            column = 1;
                            break;
                        case AvatarClass.Engineer:
                            column = 2;
                            break;
                    }

                    foreach (var skill in player.CurrentSkills)
                    {
                        var sc = CachedRepository.Instance.Get<SkillConfig>(skill);
                        switch (sc.Type)
                        {
                            case SkillType.Helm:
                                if (showTools[0, column])
                                    _pilotIds.Add(skill);
                                break;
                            case SkillType.Gun:
                                if (showTools[1, column])
                                    _gunnerIds.Add(skill);
                                break;
                            case SkillType.Repair:
                                if (showTools[2, column])
                                    _engineerIds.Add(skill);
                                break;
                        }
                    }
                }

                _loadoutIds.AddRange(_pilotIds);
                _loadoutIds.AddRange(_gunnerIds);
                _loadoutIds.AddRange(_engineerIds);
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

        private List<RawImage> _loadoutImages;

        public void DisplayItems(UserAvatarEntity player, bool[,] showTools)
        {
            var data = new PlayerLoadoutData(player, showTools);
            for (int i = 0; i < data.GetVisibleSlots(); i++)
            {
                _loadoutImages[i].enabled = true;
                _loadoutImages[i].texture = data.GetSlotTexture(i);
            }
            for (int i = data.GetVisibleSlots(); i < _enabledToolSlots; i++)
                _loadoutImages[i].enabled = false;

            for (int i = 0; i < _enabledToolSlots; i++)
                _loadoutImages[i].gameObject.SetActive(true);
            for (int i = _enabledToolSlots; i < _loadoutImages.Count; i++)
                _loadoutImages[i].gameObject.SetActive(false);
        }

        private static int _enabledToolSlots = 6;
        public static void SetEnabledToolSlotCount(bool[,] showTools)
        {
            var classCount = new int[] { 0, 0, 0 };
            if (showTools[0, 0]) classCount[0] += 3;    // Pilot has 3 pilot tools
            if (showTools[1, 0]) classCount[0] += 1;    // Pilot has 1 gunner tool
            if (showTools[2, 0]) classCount[0] += 1;    // Pilot has 1 engineer tool
            if (showTools[0, 1]) classCount[1] += 1;    // Gunner has 1 pilot tool
            if (showTools[1, 1]) classCount[1] += 3;    // Gunner has 3 gunner tools
            if (showTools[2, 1]) classCount[1] += 2;    // Gunner has 2 engineer tools
            if (showTools[0, 2]) classCount[2] += 1;    // Engineer has 1 pilot tool
            if (showTools[1, 2]) classCount[2] += 1;    // Engineer has 1 gunner tool
            if (showTools[2, 2]) classCount[2] += 3;    // Engineer has 3 engineer tools

            _enabledToolSlots = Mathf.Max(classCount);
        }
    }
}
