using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Muse.Common;
using Muse.Goi2.Entity;

namespace BuffKit.ItemSelection
{
    public class UICustomItemSelectionWindow : UIBaseModalDialog
    {
        public static UICustomItemSelectionWindow Instance { get; private set; }

        private Action<SelectionElement> chooseCallback;
        public bool Activated
        {
            get
            {
                return base.gameObject.activeSelf;
            }
        }
        private void Hide()
        {
            base.gameObject.SetActive(false);
            UIItemTooltip.Instance.Hide();
            UIGunTooltip.Instance.Hide();
        }
        private void Cancel()
        {
            this.Hide();
            this.chooseCallback.InvokeSafe(null, null);
        }
        public bool TryCancel()
        {
            if (base.gameObject.activeSelf)
            {
                this.Cancel();
                return true;
            }
            return false;
        }
        private void CellClicked(UISelectionItem cell)
        {
            this.Hide();
            this.chooseCallback.InvokeSafe(cell.Data, null);
        }
        private void CellEntered(UISelectionItem cell)
        {
            this.ShowCellTooltip(cell);
        }
        private void CellExited(UISelectionItem cell)
        {
            UIItemTooltip.Instance.Hide();
            UIGunTooltip.Instance.Hide();
        }
        private void ShowCellTooltip(UISelectionItem cell)
        {
            SelectionElement data = cell.Data;
            Item item = null;
            SkillConfig skillConfig = null;
            if (data is ItemSelectionElement)
            {
                item = ((ItemSelectionElement)data).Item;
            }
            else if (data is UserItemSelectionElement)
            {
                item = ((UserItemSelectionElement)data).Item;
            }
            else if (data is PlayerSkillSelectionElement)
            {
                skillConfig = ((PlayerSkillSelectionElement)data).Skill;
            }
            UIOverlayPanel uioverlayPanel = null;
            if (item != null)
            {
                if (item is GunItem)
                {
                    uioverlayPanel = UIGunTooltip.Instance;
                    UIGunTooltip.Instance.RenderGun(GunItemInfo.FromGunItem((GunItem)item), data.AdditionalTipDescription);
                }
                else
                {
                    uioverlayPanel = UIItemTooltip.Instance;
                    UIItemTooltip.Instance.RenderItem(item, data.AdditionalTipDescription);
                }
            }
            else if (skillConfig != null)
            {
                uioverlayPanel = UIItemTooltip.Instance;
                UIItemTooltip.Instance.RenderSkill(skillConfig, 0, data.AdditionalTipDescription);
            }
            else if (data is ViewStoreSelectionElement)
            {
                uioverlayPanel = UIItemTooltip.Instance;
                UIItemTooltip.Instance.RenderCustom("MORE ITEMS", string.Empty, "Get more items from our market!", null);
            }
            if (uioverlayPanel != null)
            {
                bool flag = cell.Index % 5 < 3;
                bool flag2 = cell.Index / 5 < 2;
                int corner = (!flag || !flag2) ? ((!flag || flag2) ? ((flag || !flag2) ? 0 : 1) : 3) : 2;
                Vector2 value = (!flag || !flag2) ? ((!flag || flag2) ? ((flag || !flag2) ? new Vector2(1f, 0f) : new Vector2(1f, 1f)) : new Vector2(0f, 0f)) : new Vector2(0f, 1f);
                Vector3 b = (!flag) ? new Vector3(-10f, 0f) : new Vector3(10f, 0f);
                uioverlayPanel.ShowAtScreenPosition(cell.GetWorldPosition(corner) + b, new Vector2?(value), 0f);
            }
        }

        private GameObject _obContent;
        private LayoutElement _leMenuPanel;
        protected override void Awake()
        {
            base.Awake();

            gameObject.AddComponent<GraphicRaycaster>();

            var csf = gameObject.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.MinSize;
            _leMenuPanel = gameObject.AddComponent<LayoutElement>();
            var vlg = gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(7, 7, 7, 7);
            vlg.spacing = 7;
            vlg.childAlignment = TextAnchor.UpperCenter;

            var obTitle = UI.Builder.BuildLabel(gameObject.transform, out _lTitle, UI.Resources.FontPenumbraHalfSerifStd, TextAnchor.MiddleCenter, 20);

            UI.Builder.BuildVerticalScrollViewFitContent(gameObject.transform, out var obContent);

            var obButton = UI.Builder.BuildButton(gameObject.transform, delegate { Cancel(); }, "Cancel");
            var le = obButton.AddComponent<LayoutElement>();
            le.minWidth = 100;
            le.minHeight = 34;

            _obContent = new GameObject("item selection grid");
            _obContent.transform.SetParent(obContent.transform, false);

            var glg = _obContent.AddComponent<GridLayoutGroup>();
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 5;
            glg.cellSize = new Vector2(100, 100);
            glg.spacing = new Vector2(3, 3);

            _items = new List<UISelectionItem>();

            var obDropShadow = GameObject.Instantiate(GameObject.Find("Menu UI/Standard Canvas/Common Elements/Item Selection Window/Item Selection Panel/Drop Shadow"), transform);
            var rt = obDropShadow.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(.5f, .5f);
            rt.anchorMax = new Vector2(.5f, .5f);
            rt.offsetMin = new Vector2(-785, -389);
            rt.offsetMax = new Vector2(785, 389);
            obDropShadow.transform.SetAsFirstSibling();

            Hide();
        }


        private List<UISelectionItem> _items;
        private List<SelectionElement> _currentItems;
        private TextMeshProUGUI _lTitle;

        public void ShowItems(string heading, IEnumerable<SelectionElement> choices, Action<SelectionElement> chooseCallback)
        {
            _lTitle.text = heading;
            var choiceCount = choices.Count();
            _leMenuPanel.minHeight = choiceCount <= 10 ? 285 : 500;
            for (var i = _items.Count; i < choiceCount; i++)
            {
                var obSelectionItem = GameObject.Instantiate(UISelectionWindow.Instance.sampleItem, _obContent.transform);
                obSelectionItem.Index = i;
                obSelectionItem.OnMouseClick += this.CellClicked;
                obSelectionItem.OnMouseEnter += this.CellEntered;
                obSelectionItem.OnMouseExit += this.CellExited;
                obSelectionItem.gameObject.SetActive(false);
                _items.Add(obSelectionItem);
            }
            for (var i = choiceCount; i < _items.Count; i++)
                _items[i].gameObject.SetActive(false);

            gameObject.SetActive(true);
            this.chooseCallback = chooseCallback;
            _currentItems = choices.ToList();

            for (var i = 0; i < _currentItems.Count; i++)
            {
                _items[i].Data = _currentItems[i];
                _items[i].gameObject.SetActive(true);
            }
        }
        public void SetEnabled()
        {
            UISelectionWindow.Instance.TryCancel();
        }

        private static GameObject _obItemSelectionWindow;
        public static void Initialize()
        {
            _obItemSelectionWindow = GameObject.Find("/Menu UI/Standard Canvas/Common Elements/Item Selection Window");

            var obPanel = UI.Builder.BuildPanel(_obItemSelectionWindow.transform.parent);
            obPanel.transform.SetSiblingIndex(_obItemSelectionWindow.transform.GetSiblingIndex() + 1);
            obPanel.name = "Custom Item Selection";

            Instance = obPanel.AddComponent<UICustomItemSelectionWindow>();
        }
    }
}
