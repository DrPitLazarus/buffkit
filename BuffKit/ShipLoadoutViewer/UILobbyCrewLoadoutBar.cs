using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Muse.Goi2.Entity;

namespace BuffKit.ShipLoadoutViewer
{
    class UILobbyCrewLoadoutBar : MonoBehaviour
    {
        private static Color sSpacerColor = new Color32(0x52, 0x3E, 0x3F, 0xFF);
        public bool MarkForRedraw { set; get; }
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

            for (var i = 0; i < 3; i++)
            {
                var subImages = new List<RawImage>();

                var obSubBar = new GameObject($"bar {i}");
                obSubBar.transform.SetParent(obBar.transform, false);
                hlg = obSubBar.AddComponent<HorizontalLayoutGroup>();
                hlg.childForceExpandWidth = false;
                hlg.childForceExpandHeight = false;
                hlg.spacing =3;
                subBars.Add(obSubBar);
                for (var j = 0; j < 3; j++)
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
            public PlayerLoadoutData(UserAvatarEntity player)
            {
                var pilotIds = new List<int>();
                var gunnerIds = new List<int>();
                var engineerIds = new List<int>();
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
                        }
                    }
                }
                else
                {
                    PlayerClass = -1;
                }
                AllIds = new List<List<int>> { pilotIds, gunnerIds, engineerIds };
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

        private GameObject _spacer1;
        private GameObject _spacer2;
        private Image _imSpacer1;
        private Image _imSpacer2;
        private List<GameObject> _loadoutBarObjects;
        private List<List<RawImage>> _loadoutBarImages;
        private PlayerLoadoutData _loadoutDataLast = new PlayerLoadoutData(null);

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

                for (var i = 0; i < 3; i++)
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
                            _loadoutBarImages[i][j].texture = UI.Resources.GetSkillTexture(data.AllIds[i][j]);
                        }
                        for (var j = classItemCount; j < 3; j++)
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
                //for (var i = 0; i < 3; i++)
                //    _loadoutBarObjects[i].SetActive(false);
                //_spacer1.SetActive(false);
                //_spacer2.SetActive(false);
            }
        }

        //private static int _enabledToolSlots = 6;
        private static bool[,] showTools;
        public static void SetEnabledToolSlotCount(bool[,] newShowTools)
        {
            /*
            var classCount = new int[] { 0, 0, 0 };
            if (newShowTools[0, 0]) classCount[0] += 3;     // Pilot has 3 pilot tools
            if (newShowTools[1, 0]) classCount[0] += 1;     // Pilot has 1 gunner tool
            if (newShowTools[2, 0]) classCount[0] += 1;     // Pilot has 1 engineer tool
            if (newShowTools[0, 1]) classCount[1] += 1;     // Gunner has 1 pilot tool
            if (newShowTools[1, 1]) classCount[1] += 3;     // Gunner has 3 gunner tools
            if (newShowTools[2, 1]) classCount[1] += 2;     // Gunner has 2 engineer tools
            if (newShowTools[0, 2]) classCount[2] += 1;     // Engineer has 1 pilot tool
            if (newShowTools[1, 2]) classCount[2] += 1;     // Engineer has 1 gunner tool
            if (newShowTools[2, 2]) classCount[2] += 3;     // Engineer has 3 engineer tools
            */

            showTools = newShowTools;

            //_enabledToolSlots = Mathf.Max(classCount);
        }
    }
}
