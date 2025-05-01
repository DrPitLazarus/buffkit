using System;
using System.Collections.Generic;
using System.Linq;
using BuffKit.UI;
using Muse.Goi2.Entity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Resources = BuffKit.UI.Resources;
using Text = UnityEngine.UI.Text;

namespace BuffKit.TitleSelection
{
    public class UICustomTitleSelection : UIBaseModalDialog
    {
        public static UICustomTitleSelection Instance { get; private set; }

        private GameObject _obContent;
        private InputField _searchField;

        public override void Awake()
        {
            base.Awake();

            LayoutElement le;
            HorizontalLayoutGroup hlg;
            VerticalLayoutGroup vlg;

            var csf = gameObject.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            vlg = gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(7, 7, 7, 7);
            vlg.spacing = 7;
            vlg.childAlignment = TextAnchor.MiddleCenter;

            Builder.BuildLabel(transform, "Select Title", TextAnchor.MiddleCenter, 20);

            var obTopRow = new GameObject("top row");
            obTopRow.transform.SetParent(transform, false);
            hlg = obTopRow.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.spacing = 5;
            var obSortAZ = Builder.BuildButton(obTopRow.transform, SortByAZ, "A-Z", TextAnchor.MiddleCenter, 13);
            le = obSortAZ.AddComponent<LayoutElement>();
            le.preferredWidth = 40;
            le.preferredHeight = 26;

            var obSearchField = GameObject.Instantiate(GameObject.Find("/Menu UI/Standard Canvas/Pages/UI Match Create/Container/Options Panel/Scroll View/Viewport/Content/Password Option/Input Field"), obTopRow.transform);
            _searchField = obSearchField.GetComponent<InputField>();
            _searchField.onValueChanged.AddListener(SearchValueChanged);
            var txtPlaceholder = _searchField.transform.Find("Placeholder").GetComponent<Text>();
            txtPlaceholder.text = "Search title";
            txtPlaceholder.fontSize = 15;
            _searchField.textComponent.fontSize = 15;
            le = obSearchField.AddComponent<LayoutElement>();
            le.preferredHeight = 26;

            var obScrollviewHolder = new GameObject("panel holder");
            obScrollviewHolder.transform.SetParent(transform, false);
            le = obScrollviewHolder.AddComponent<LayoutElement>();
            le.preferredWidth = 300;
            le.preferredHeight = 500;

            var obScrollview = Builder.BuildVerticalScrollViewFitParent(obScrollviewHolder.transform, out var obContent);
            obScrollview.GetComponent<HorizontalLayoutGroup>().padding = new RectOffset(0, 0, 0, 0);
            _obContent = new GameObject("title list");
            _obContent.transform.SetParent(obContent.transform, false);
            vlg = _obContent.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 1;
            vlg.childForceExpandHeight = false;
            vlg.childAlignment = TextAnchor.MiddleLeft;

            var obButtonGroup = new GameObject("button group");
            obButtonGroup.transform.SetParent(transform, false);
            hlg = obButtonGroup.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.spacing = 5;
            var obSave = Builder.BuildButton(obButtonGroup.transform, Save, "Save");
            le = obSave.AddComponent<LayoutElement>();
            le.preferredWidth = 100;
            le.preferredHeight = 34;
            var obCancel = Builder.BuildButton(obButtonGroup.transform, Cancel, "Cancel");
            le = obCancel.AddComponent<LayoutElement>();
            le.preferredWidth = 100;
            le.preferredHeight = 34;

            var obDropShadow = GameObject.Instantiate(GameObject.Find("Menu UI/Standard Canvas/Common Elements/Item Selection Window/Item Selection Panel/Drop Shadow"), transform);
            var rt = obDropShadow.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(.5f, .5f);
            rt.anchorMax = new Vector2(.5f, .5f);
            rt.offsetMin = new Vector2(-785, -389);
            rt.offsetMax = new Vector2(785, 389);
            obDropShadow.transform.SetAsFirstSibling();

            _titleEntries = new List<UITitleEntry>();

            gameObject.SetActive(false);
        }

        private List<PlayerTitle> _currentTitleList;
        private List<UITitleEntry> _titleEntries;
        private PlayerTitle _selectedTitle;
        private Action<PlayerTitle> _currentCallback;

        private void SortByAZ()
        {
            _currentTitleList = (from title in _currentTitleList
                                 orderby title.TitleText.En
                                 select title).ToList();
            DisplayCurrentTitles();
        }

        public void DisplayMenu(List<PlayerTitle> titles, Action<PlayerTitle> callback)
        {
            // Show menu
            gameObject.SetActive(true);

            _selectedTitle = null;
            _currentCallback = callback;
            _currentTitleList = titles;
            // Build menu items
            for (var i = _titleEntries.Count; i < titles.Count; i++)
            {
                var callbackIndex = i;
                // Transform parent, out TextMeshProUGUI label, UnityAction callback, TMP_FontAsset font, int fontSize = 13
                var obEntryButton = Builder.BuildMenuButton(_obContent.transform, out var _, null, Resources.FontGaldeanoRegular, 15);
                _titleEntries.Add(obEntryButton.AddComponent<UITitleEntry>());
            }
            DisplayCurrentTitles();
        }

        private void DisplayCurrentTitles()
        {
            // Display titles
            for (var i = 0; i < _currentTitleList.Count; i++)
            {
                _titleEntries[i].SetEntry(_currentTitleList[i], SelectTitle);
                _titleEntries[i].Show();
            }
            // Hide unnecessary entries
            for (var i = _currentTitleList.Count; i < _titleEntries.Count; i++)
                _titleEntries[i].Hide();
            if (_searchField.text != "")
                SearchValueChanged(_searchField.text);
        }

        private void SearchValueChanged(string v)
        {
            v = v.ToUpper();
            for (var i = 0; i < _currentTitleList.Count; i++)
                _titleEntries[i].ShowIfMatch(v);
        }

        private void SelectTitle(PlayerTitle title) { _selectedTitle = title; }

        private void Cancel()
        {
            _currentCallback = null;
            _selectedTitle = null;
            _searchField.text = "";
            gameObject.SetActive(false);
        }

        private void Save()
        {
            if (_currentCallback != null && _selectedTitle != null)
                _currentCallback(_selectedTitle);
            Cancel();
        }

        public bool TryHide()
        {
            if (gameObject.activeSelf)
            {
                Cancel();
                return true;
            }
            return false;
        }

        public static void Initialize()
        {
            var obCommonElements = GameObject.Find("/Menu UI/Standard Canvas/Common Elements");

            var obPanel = Builder.BuildPanel(obCommonElements.transform);
            obPanel.name = "custom title selection";

            Instance = obPanel.AddComponent<UICustomTitleSelection>();
        }

        private class UITitleEntry : MonoBehaviour
        {
            private PlayerTitle _title;
            private string _titleTextUpper;
            private TextMeshProUGUI _label;
            private Button _button;
            private Action<PlayerTitle> _callback;

            private void Awake()
            {
                _button = gameObject.GetComponent<Button>();
                _label = gameObject.GetComponentInChildren<TextMeshProUGUI>();
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(OnClick);
            }

            public void Show() { gameObject.SetActive(true); }
            public void Hide() { gameObject.SetActive(false); }
            public void ShowIfMatch(string text) { gameObject.SetActive(_titleTextUpper.Contains(text)); }

            public void SetEntry(PlayerTitle title, Action<PlayerTitle> callback)
            {
                _label.text = title.TitleText.En;
                _title = title;
                _titleTextUpper = title.TitleText.En.ToUpper();
                _callback = callback;
            }

            private void OnClick()
            {
                _callback?.Invoke(_title);
            }
        }

    }
}
