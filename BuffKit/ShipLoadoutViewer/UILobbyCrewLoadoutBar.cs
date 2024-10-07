﻿using System.Collections.Generic;
using Muse.Goi2.Entity;
using UnityEngine;
using UnityEngine.UI;
using Resources = BuffKit.UI.Resources;

namespace BuffKit.ShipLoadoutViewer
{
    class UILobbyCrewLoadoutBar : MonoBehaviour
    {
        private static Color sSpacerColor = new Color32(0x52, 0x3E, 0x3F, 0xFF);
        public bool MarkForRedraw { set; get; }
        private static readonly int _numberOfToolTypes = 4; // Should be the same number of rows in Patcher.cs toggleGrid

        public static GameObject Build(Transform parent, out UILobbyCrewLoadoutBar loadoutBar)
        {
            Image img;
            LayoutElement le;

            var obBar = new GameObject("Loadout Bar");

            var hlg = obBar.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.spacing = 1;

            var subBars = new List<GameObject>();
            var subBarImages = new List<List<RawImage>>();

            for (var i = 0; i < _numberOfToolTypes; i++)
            {
                var subImages = new List<RawImage>();

                var obSubBar = new GameObject($"bar {i}");
                obSubBar.transform.SetParent(obBar.transform, false);
                hlg = obSubBar.AddComponent<HorizontalLayoutGroup>();
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = false;
                hlg.spacing = 3;
                subBars.Add(obSubBar);
                for (var j = 0; j < _numberOfToolTypes; j++)
                {
                    var slot = new GameObject($"slot{j}");
                    var im = slot.AddComponent<RawImage>();
                    subImages.Add(im);
                    le = slot.AddComponent<LayoutElement>();
                    le.preferredWidth = 21;
                    le.preferredHeight = 21;
                    slot.transform.SetParent(obSubBar.transform, false);
                }
                subBarImages.Add(subImages);
            }

            var factionIconObject = new GameObject("Faction Icon");
            var factionIconImage = factionIconObject.AddComponent<Image>();
            factionIconObject.AddComponent<LayoutElement>().preferredWidth = 21;
            factionIconObject.transform.SetParent(obBar.transform, false);

            loadoutBar = obBar.AddComponent<UILobbyCrewLoadoutBar>();
            loadoutBar._loadoutBarObjects = subBars;
            loadoutBar._loadoutBarImages = subBarImages;
            loadoutBar._spacer1 = new GameObject("spacer");
            loadoutBar._imSpacer1 = loadoutBar._spacer1.AddComponent<Image>();
            loadoutBar._imSpacer1.color = new Color(0, 0, 0, 0);
            le = loadoutBar._spacer1.AddComponent<LayoutElement>();
            le.preferredWidth = 1;
            le.preferredHeight = 21;
            loadoutBar._spacer1.transform.SetParent(obBar.transform, false);
            loadoutBar._spacer1.transform.SetSiblingIndex(1);
            loadoutBar._spacer2 = new GameObject("spacer");
            loadoutBar._imSpacer2 = loadoutBar._spacer2.AddComponent<Image>();
            loadoutBar._imSpacer2.color = new Color(0, 0, 0, 0);
            le = loadoutBar._spacer2.AddComponent<LayoutElement>();
            le.preferredWidth = 1;
            le.preferredHeight = 21;
            loadoutBar._spacer2.transform.SetParent(obBar.transform, false);
            loadoutBar._spacer2.transform.SetSiblingIndex(3);
            loadoutBar._factionIconObject = factionIconObject;
            loadoutBar._factionIconImage = factionIconImage;

            img = obBar.AddComponent<Image>();
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
            obBar.transform.SetSiblingIndex(obBar.transform.GetSiblingIndex() - 1);
            return obBar;
        }

        class PlayerLoadoutData
        {
            public List<List<int>> AllIds { get; private set; }
            public int PlayerClass { get; private set; }
            public int PlayerId { get; private set; }
            public PlayerLoadoutData(UserAvatarEntity player)
            {
                var pilotIds = new List<int>();
                var gunnerIds = new List<int>();
                var engineerIds = new List<int>();
                var specialIds = new List<int>();

                if (player != null)
                {
                    switch (player.CurrentClass)
                    {
                        case AvatarClass.Pilot:
                            PlayerClass = 0;
                            break;
                        case AvatarClass.Gunner:
                            PlayerClass = 1;
                            break;
                        case AvatarClass.Engineer:
                            PlayerClass = 2;
                            break;
                    }

                    PlayerId = player.Id;

                    foreach (var skill in player.CurrentSkills)
                    {
                        var sc = CachedRepository.Instance.Get<SkillConfig>(skill);
                        switch (sc.Type)
                        {
                            case SkillType.Helm:
                                pilotIds.Add(skill);
                                break;
                            case SkillType.Gun:
                                gunnerIds.Add(skill);
                                break;
                            case SkillType.Repair:
                                engineerIds.Add(skill);
                                break;
                            case SkillType.SpecialPilot:
                            case SkillType.SpecialGunner:
                            case SkillType.SpecialEngineer:
                                specialIds.Add(skill);
                                break;
                        }
                    }
                }
                else
                {
                    PlayerClass = -1;
                }
                AllIds = new List<List<int>> { pilotIds, gunnerIds, engineerIds, specialIds };
            }
        }

        public void SetVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }

        public void SetFactionIconVisibility(bool isVisible)
        {
            _factionIconObject.SetActive(isVisible);
        }

        void Update()
        {
            if (MarkForRedraw)
            {
                DisplayItemsFromData(_loadoutDataLast);
                MarkForRedraw = false;
            }
        }

        private GameObject _spacer1;
        private GameObject _spacer2;
        private Image _imSpacer1;
        private Image _imSpacer2;
        private List<GameObject> _loadoutBarObjects;
        private List<List<RawImage>> _loadoutBarImages;
        private PlayerLoadoutData _loadoutDataLast = new PlayerLoadoutData(null);
        private GameObject _factionIconObject;
        private Image _factionIconImage;

        public void DisplayItems(UserAvatarEntity player)
        {
            _loadoutDataLast = new PlayerLoadoutData(player);
            DisplayItemsFromData(_loadoutDataLast);
        }

        public void SetSeparatorVisibility(bool isVisible)
        {
            if (isVisible)
            {
                _imSpacer1.color = sSpacerColor;
                _imSpacer2.color = sSpacerColor;
            }
            else
            {
                _imSpacer1.color = new Color(0, 0, 0, 0);
                _imSpacer2.color = new Color(0, 0, 0, 0);
            }
        }

        private void DisplayItemsFromData(PlayerLoadoutData data)
        {
            if (data.PlayerClass != -1)
            {
                gameObject.SetActive(true);

                int column = data.PlayerClass;

                _spacer1.SetActive(showTools[0, column] && showTools[1, column]);
                _spacer2.SetActive(showTools[2, column] && (showTools[0, column] || showTools[1, column]));

                _factionIconImage.sprite = ShipLoadoutViewer.GetPlayerFactionSprite(data.PlayerId);

                for (var i = 0; i < _numberOfToolTypes; i++)
                {
                    if (showTools[i, column])
                    {
                        // Show this sub-bar
                        _loadoutBarObjects[i].SetActive(true);
                        var classItemCount = data.AllIds[i].Count;
                        for (var j = 0; j < classItemCount; j++)
                        {
                            //_loadoutBarImages[i][j].enabled = true;
                            _loadoutBarImages[i][j].gameObject.SetActive(true);
                            _loadoutBarImages[i][j].texture = Resources.GetSkillTexture(data.AllIds[i][j]);
                        }
                        for (var j = classItemCount; j < _numberOfToolTypes; j++)
                        {
                            _loadoutBarImages[i][j].gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        // Hide this sub-bar
                        _loadoutBarObjects[i].SetActive(false);
                    }
                }
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private static bool[,] showTools;
        public static void SetEnabledToolSlotCount(bool[,] newShowTools)
        {
            showTools = newShowTools;
        }
    }
}
