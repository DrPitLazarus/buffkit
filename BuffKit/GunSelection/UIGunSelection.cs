using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Muse.Goi2.Entity;
using UnityEngine.EventSystems;

namespace BuffKit.GunSelection
{
    public class UIGunSelection : UIBaseModalDialog
    {

        protected override void Awake()
        {
            base.Awake();
            Instance = this;

            var rt = gameObject.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(.5f, .5f);
            rt.anchorMax = new Vector2(.5f, .5f);
            rt.pivot = new Vector2(.5f, .5f);
            var csf = base.gameObject.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var vlg = gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(7, 7, 7, 7);
            vlg.spacing = 7;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            gameObject.AddComponent<GraphicRaycaster>();

            var obLabel = UI.Builder.BuildLabel(gameObject.transform, "Gun Selection", TextAnchor.MiddleCenter, 20);

            _grid = UIGunSelectionGrid.Build(gameObject.transform, 20, 100);

            var obCancel = UI.Builder.BuildButton(gameObject.transform, delegate { Activated = false; _callback = null; }, "Cancel");
            var le = obCancel.AddComponent<LayoutElement>();
            le.preferredWidth = 100;
            le.preferredHeight = 34;
            //var hlg = obCancel.transform.Find("Label").GetComponent<HorizontalLayoutGroup>();
            //hlg.childAlignment = TextAnchor.MiddleLeft;

            UI.Resources.RegisterGunTextureCallback(MarkIconsForRedraw);
            Activated = false;
        }
        public bool TryHide()
        {
            if (Activated)
            {
                Activated = false;
                return true;
            }
            return false;
        }
        public bool Activated
        {
            get
            {
                return gameObject.activeSelf;
            }
            set
            {
                gameObject.SetActive(value);
            }
        }

        public void DisplayGunSelection(ShipPartSlotSize slotSize, Action<int> callback, List<int> availableGuns)
        {
            var selectedCategories = _gunCategories[NetworkedPlayer.Local.GameType][slotSize];
            Activated = true;
            _grid.SetGunIds(selectedCategories, delegate (int v)
            {
                _callback?.Invoke(v);
                Activated = false;
            }, _gunRows[NetworkedPlayer.Local.GameType][slotSize], availableGuns);
            _callback = callback;
        }

        public static UIGunSelection Instance;
        private static GameObject _obItemSelectionWindow;
        private UIGunSelectionGrid _grid;

        private Action<int> _callback;


        private void MarkIconsForRedraw()
        {
            _grid.MarkForRedraw = true;
        }

        private static Dictionary<GameType, Dictionary<ShipPartSlotSize, List<int>>> _gunCategories;
        private static Dictionary<GameType, Dictionary<ShipPartSlotSize, int>> _gunRows;
        private static int _maxCategoryElements;

        public static void Initialize()
        {
            _obItemSelectionWindow = GameObject.Find("/Menu UI/Standard Canvas/Common Elements/Item Selection Window");

            var gameTypes = new GameType[] { GameType.Skirmish, GameType.Coop };
            var gunSizes = new ShipPartSlotSize[] { ShipPartSlotSize.SMALL, ShipPartSlotSize.MEDIUM };

            // Create sets for all categories
            _gunCategories = new Dictionary<GameType, Dictionary<ShipPartSlotSize, List<int>>>();
            _gunRows = new Dictionary<GameType, Dictionary<ShipPartSlotSize, int>>();
            foreach (var gameType in gameTypes)
            {
                _gunCategories.Add(gameType, new Dictionary<ShipPartSlotSize, List<int>>());
                _gunRows.Add(gameType, new Dictionary<ShipPartSlotSize, int>());
                foreach (var gunSize in gunSizes)
                {
                    _gunCategories[gameType].Add(gunSize, new List<int>());
                }
            }

            // Custom gun list (skirmish light guns)
            _gunRows[GameType.Skirmish][ShipPartSlotSize.SMALL] = 5;
            List<int> listSkirmishLight = new List<int>()
            {
                171, 1013, 199, 1943, -1,       // gat, hades, merc, aten
                204, 206, 117, 114, 1884,       // mortar, banshee, flak, artemis, tempest
                194, 172, 195, 951, 198         // carro, flamer, harpoon, mine, flare
            };
            _gunCategories[GameType.Skirmish][ShipPartSlotSize.SMALL].AddRange(listSkirmishLight);
            // Custom gun list (skirmish heavy guns)
            _gunRows[GameType.Skirmish][ShipPartSlotSize.MEDIUM] = 4;
            List<int> listSkirmishHeavy = new List<int>()
            {
                203, 920, 173, 1885,            // hwacha, lumber, flak1, nemesis
                838, 1086, 1779, 2069           // carro, mino, flak2, detonator
            };
            _gunCategories[GameType.Skirmish][ShipPartSlotSize.MEDIUM].AddRange(listSkirmishHeavy);
            // Other lists (not set for coop)
            _gunRows[GameType.Coop][ShipPartSlotSize.SMALL] = 5;
            _gunRows[GameType.Coop][ShipPartSlotSize.MEDIUM] = 5;

            // Add guns to damage categories
            foreach (var id in Util.Util.GunIds)
            {
                var gi = CachedRepository.Instance.Get<GunItem>(id);

                foreach (var gameType in gameTypes)
                {
                    // GameType is a flag enum
                    if ((gi.GameType & gameType) > 0 && !_gunCategories[gameType][gi.Size].Contains(id))
                    {
                        _gunCategories[gameType][gi.Size].Add(id);
                    }
                }
            }


            // Fill _maxCategoryElements
            _maxCategoryElements = 0;
            foreach (var gameType in gameTypes)
            {
                foreach (var gunSize in gunSizes)
                {
                    var categoryCount = _gunCategories[gameType][gunSize].Count;
                    _maxCategoryElements = Math.Max(_maxCategoryElements, categoryCount);
                }
            }

            // Create custom gun selection window
            var obPanel = UI.Builder.BuildPanel(_obItemSelectionWindow.transform.parent);
            obPanel.name = "Custom Gun Selection";
            obPanel.transform.SetSiblingIndex(_obItemSelectionWindow.transform.GetSiblingIndex() + 1);
            obPanel.AddComponent<UIGunSelection>();
        }


        private class UIGunSelectionGrid : MonoBehaviour
        {
            private GridLayoutGroup _gridLayout;
            public static UIGunSelectionGrid Build(Transform parent, int gridElements, int elementSize)
            {
                var obGrid = new GameObject($"gun selection grid");
                obGrid.transform.SetParent(parent, false);

                var uigsr = obGrid.AddComponent<UIGunSelectionGrid>();

                uigsr._gridLayout = obGrid.AddComponent<GridLayoutGroup>();
                uigsr._gridLayout.cellSize = new Vector2(elementSize, elementSize);
                uigsr._gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                uigsr._gridLayout.spacing = new Vector2(3, 3);
                uigsr._gridLayout.padding = new RectOffset(3, 3, 3, 3);

                uigsr._gunSelectionItems = new List<UIGunSelectionItem>();
                for (var i = 0; i < gridElements; i++)
                {
                    var obSelectionElement = new GameObject($"element {i}");
                    obSelectionElement.transform.SetParent(obGrid.transform, false);

                    uigsr._gunSelectionItems.Add(obSelectionElement.AddComponent<UIGunSelectionItem>());
                }

                return uigsr;
            }

            public bool MarkForRedraw { get; set; }
            private void Update()
            {
                if (MarkForRedraw)
                {
                    foreach (var g in _gunSelectionItems)
                        g.FetchTexture();
                    MarkForRedraw = false;
                }
            }

            public void SetGunIds(List<int> id, Action<int> callback, int columnCount, List<int> availableGuns)
            {
                _gridLayout.constraintCount = columnCount;
                var idsToShow = Math.Min(id.Count, _gunSelectionItems.Count);
                for (var i = 0; i < idsToShow; i++)
                {
                    _gunSelectionItems[i].SetGun(id[i], callback, availableGuns.Contains(id[i]));
                    _gunSelectionItems[i].gameObject.SetActive(true);
                }
                for (var i = idsToShow; i < _gunSelectionItems.Count; i++)
                    _gunSelectionItems[i].gameObject.SetActive(false);
            }

            private List<UIGunSelectionItem> _gunSelectionItems;

            private class UIGunSelectionItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
            {
                private int _gunId = -1;
                private GunItemInfo _gunInfo;
                private RawImage _image;
                private Action<int> _callback;
                private Button _button;
                public void FetchTexture()
                {
                    _image.texture = UI.Resources.GetGunTexture(_gunId);
                }
                public void SetGun(int gunId, Action<int> callback, bool isEnabled)
                {
                    _gunId = gunId;
                    _gunInfo = GunItemInfo.FromGunItem(CachedRepository.Instance.Get<GunItem>(gunId));
                    if (_gunId != -1)
                    {
                        _image.texture = UI.Resources.GetGunTexture(_gunId);
                        transform.localScale = new Vector3(1, 1, 1);
                    }
                    else transform.localScale = new Vector3(0, 0, 0);
                    if (isEnabled) _image.color = new Color(1, 1, 1, 1);
                    else _image.color = new Color32(0x60, 0x60, 0x60, 0xD0);
                    _button.interactable = isEnabled;
                    _callback = callback;
                }
                private void Awake()
                {
                    var obIcon = new GameObject("icon");
                    obIcon.transform.SetParent(transform, false);

                    _image = obIcon.AddComponent<RawImage>();

                    var le = gameObject.AddComponent<LayoutElement>();
                    le.preferredWidth = 100;
                    le.preferredHeight = 100;

                    var hlg = gameObject.AddComponent<HorizontalLayoutGroup>();
                    hlg.childForceExpandWidth = true;
                    hlg.childForceExpandHeight = true;
                    hlg.padding = new RectOffset(2, 2, 2, 2);

                    var imBackground = gameObject.AddComponent<Image>();
                    imBackground.color = new Color(1, 1, 1, 1);

                    _button = gameObject.AddComponent<Button>();
                    _button.transition = Selectable.Transition.ColorTint;
                    _button.colors = UI.Resources.MenuButtonColors;
                    _button.targetGraphic = imBackground;
                }

                public void OnPointerEnter(PointerEventData eventData)
                {
                    if (_gunId == -1) return;
                    UIGunTooltip.Instance.RenderGun(_gunInfo);
                    UIGunTooltip.Instance.ShowAtScreenPosition(UIShipCustomizationScreen.Instance.gunTooltipAnchor.position, new Vector2?(new Vector2(0f, 1f)), 0f);
                }
                public void OnPointerExit(PointerEventData eventData)
                {
                    UIGunTooltip.Instance.Hide();
                }
                public void OnPointerClick(PointerEventData eventData)
                {
                    if (!_button.interactable) return;
                    _callback?.Invoke(_gunId);
                    /*
                    if (_gunId == -1) return;
                    UIGunTooltip.Instance.RenderGun(_gunInfo);
                    UIGunTooltip.Instance.ShowAtScreenPosition(UIShipCustomizationScreen.Instance.gunTooltipAnchor.position, new Vector2?(new Vector2(0f, 1f)), 0f);
                    */
                }
            }
        }
    }
}
