using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Muse.Goi2.Entity;
using BuffKit.UI;

namespace BuffKit.LoadoutSort
{
    class UILoadoutSpecificSortPanel : MonoBehaviour
    {
        public static UILoadoutSpecificSortPanel Instance;

        public static void BuildPanel(Transform parent)
        {
            var obPanel = Builder.BuildPanel(parent);
            obPanel.name = "UI Loadout Specific Order Panel";
            var rt = obPanel.GetComponent<RectTransform>();
            rt.pivot = new Vector2(.5f, .5f);
            rt.anchorMin = new Vector2(.5f, .5f);
            rt.anchorMax = new Vector2(.5f, .5f);
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);
            rt.sizeDelta = new Vector2(430, 500);
            var vlg = obPanel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(5, 5, 5, 5);
            vlg.spacing = 10;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childForceExpandHeight = false;

            var panel = obPanel.AddComponent<UILoadoutSpecificSortPanel>();
            obPanel.AddComponent<GraphicRaycaster>();                       // Makes it have UI interaction on top of other UI (I think? It's complicated)

            Builder.BuildVerticalScrollViewFitParent(obPanel.transform, out var obContent);
            vlg = obContent.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 0;

            var btnRow = new GameObject("Add Row");
            var hlg = btnRow.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            btnRow.transform.SetParent(obContent.transform);

            var obAddNew = Builder.BuildButton(btnRow.transform, delegate { panel.AddNewOrder(); }, "+", TextAnchor.MiddleCenter, 44);
            var le = obAddNew.AddComponent<LayoutElement>();
            le.preferredWidth = 40;
            le.preferredHeight = 40;

            UI.Resources.RegisterSkillTextureCallback(delegate { Instance.RefreshTextures(); });

            btnRow = new GameObject("Close Row");
            hlg = btnRow.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            btnRow.transform.SetParent(panel.transform);

            var closeBtn = UI.Builder.BuildButton(btnRow.transform, delegate
            {
                Instance.TryHide();
            }, "Close", TextAnchor.MiddleCenter, 24);
            le = closeBtn.AddComponent<LayoutElement>();
            le.preferredWidth = 90;
            le.preferredHeight = 40;
            le.minHeight = 40;

            panel._content = obContent;
            Instance = panel;
        }

        private GameObject _content;
        private List<UISpecificLoadout> _specificLoadouts = new List<UISpecificLoadout>();

        private UISpecificLoadout AddNewOrder()
        {
            var go = new GameObject("order");
            go.transform.parent = _content.transform;
            go.transform.SetSiblingIndex(go.transform.GetSiblingIndex() - 1);
            var loadout = go.AddComponent<UISpecificLoadout>();
            _specificLoadouts.Add(loadout);
            return loadout;
        }

        public void AddOrders(List<ClassSkillSet> specificOrders)
        {
            foreach (var order in specificOrders)
            {
                var uiLoadout = AddNewOrder();
                uiLoadout.SetLoadout(order);
            }
        }

        private void RefreshTextures()
        {
            foreach (var sl in _specificLoadouts)
                sl.SetTextures();
        }
        public void Show()
        {
            gameObject.SetActive(true);
        }
        public bool TryHide()
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
                return true;
            }
            return false;
        }

        public void RemoveSkillSet(UISpecificLoadout skillSet)
        {
            _specificLoadouts.Remove(skillSet);
            Destroy(skillSet.gameObject);
        }

        public List<ClassSkillSet> GetSpecificSkillSets()
        {
            var list = new List<ClassSkillSet>();

            foreach (var loadout in _specificLoadouts)
            {
                var ss = loadout.SkillSet;
                if (ss.IsValid())
                {
                    list.Add(ss);
                }
            }

            return list;
        }

        public bool TrySort(List<int> skills, out List<int> sortedSkills)
        {
            foreach (var set in _specificLoadouts)
            {
                if (set.SkillSet.IsValid() && set.SkillSet.Matches(skills))
                {
                    sortedSkills = set.SkillSet.Skills.ToList();
                    return true;
                }
            }

            sortedSkills = skills;
            return false;
        }
    }

    class UISpecificLoadout : MonoBehaviour
    {
        private static Color s_inactiveColor = new Color(0.6f, 0.6f, 0.6f);
        private static Color s_activeColor = new Color(1.0f, 1.0f, 1.0f);

        public ClassSkillSet SkillSet
        {
            get
            {
                return _skills;
            }
        }

        private ClassSkillSet _skills;
        private List<RawImage> _skillImages;
        private Image _pilotImage;
        private Image _gunnerImage;
        private Image _engineerImage;

        private void Awake()
        {
            // Build UI elements
            var hlg = gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.spacing = 5;
            hlg.childAlignment = TextAnchor.MiddleCenter;

            Builder.BuildSpriteButton(transform, out _pilotImage, delegate
            {
                var wasValid = _skills.IsValid();
                SetActiveClass(AvatarClass.Pilot);
                if(wasValid)
                    LoadoutSort.SaveToFile();
            }, 35, 35).name = "pilot";
            _pilotImage.sprite = UI.Resources.PilotIcon;
            _pilotImage.color = s_inactiveColor;
            Builder.BuildSpriteButton(transform, out _gunnerImage, delegate
            {
                var wasValid = _skills.IsValid();
                SetActiveClass(AvatarClass.Gunner);
                if (wasValid)
                    LoadoutSort.SaveToFile();
            }, 35, 35).name = "gunner";
            _gunnerImage.sprite = UI.Resources.GunnerIcon;
            _gunnerImage.color = s_inactiveColor;
            Builder.BuildSpriteButton(transform, out _engineerImage, delegate
            {
                var wasValid = _skills.IsValid();
                SetActiveClass(AvatarClass.Engineer);
                if (wasValid)
                    LoadoutSort.SaveToFile();
            }, 35, 35).name = "engineer";
            _engineerImage.sprite = UI.Resources.EngineerIcon;
            _engineerImage.color = s_inactiveColor;

            _skillImages = new List<RawImage>();
            for (var i = 0; i < 3; i++)
            {
                var index = i;
                Builder.BuildImageButton(transform, out var im, delegate
                {
                    SkillButtonCallback(index);
                }, 70, 70).name = "skill " + i;
                _skillImages.Add(im);
            }

            var obDelete = Builder.BuildButton(transform, delegate
            {
                var wasValid = _skills.IsValid();
                UILoadoutSpecificSortPanel.Instance.RemoveSkillSet(this);
                if (wasValid)
                    LoadoutSort.SaveToFile();
            }, "-", TextAnchor.MiddleCenter, 72);
            var le = obDelete.AddComponent<LayoutElement>();
            le.preferredWidth = 40;
            le.preferredHeight = 40;

            if (_skills == null)
                _skills = new ClassSkillSet(null, new int[] { -1, -1, -1 });

            SetActiveClass(_skills.SkillClass);
            SetTextures();
        }

        private void SkillButtonCallback(int skillButtonIndex)
        {
            List<int> skillList;
            switch (_skills.SkillClass)
            {
                case AvatarClass.Pilot:
                    skillList = Util.PilotSkillIds;
                    break;
                case AvatarClass.Gunner:
                    skillList = Util.GunnerSkillIds;
                    break;
                case AvatarClass.Engineer:
                    skillList = Util.EngineerSkillIds;
                    break;
                default:
                    return;
            }
            var elementList = new List<SelectionElement>();
            foreach (var skill in skillList)
                elementList.Add(new PlayerSkillSelectionElement(CachedRepository.Instance.Get<SkillConfig>(skill)));

            UISelectionWindow.Instance.Show("Available Skills", elementList, delegate (SelectionElement data)
            {
                if (data == null) return;
                var playerSkillSelectionElement = (PlayerSkillSelectionElement)data;

                var wasValid = _skills.IsValid();

                _skills.Skills[skillButtonIndex] = playerSkillSelectionElement.Skill.ActivationId;
                SetTextures();

                if (_skills.IsValid() || wasValid)
                    LoadoutSort.SaveToFile();
            });
        }
        private void SetActiveClass(AvatarClass? clazz)
        {
            if (clazz != _skills.SkillClass)
            {
                _skills.Skills = new int[] { -1, -1, -1 };
                SetTextures();
            }

            _skills.SkillClass = clazz;

            _pilotImage.color = clazz == AvatarClass.Pilot ? s_activeColor : s_inactiveColor;
            _gunnerImage.color = clazz == AvatarClass.Gunner ? s_activeColor : s_inactiveColor;
            _engineerImage.color = clazz == AvatarClass.Engineer ? s_activeColor : s_inactiveColor;
        }

        public void SetLoadout(ClassSkillSet skills)
        {
            _skills = skills;
        }

        public void SetTextures()
        {
            if (_skills != null && _skills.Skills != null && _skillImages != null)
                for (var i = 0; i < _skills.Skills.Length; i++)
                {
                    _skillImages[i].texture = UI.Resources.GetSkillTexture(_skills.Skills[i]);
                }
        }
    }

    [Serializable]
    public class ClassSkillSet
    {
        public AvatarClass? SkillClass { get; set; }
        public int[] Skills { get; set; }
        public ClassSkillSet(AvatarClass? loadoutClass, int[] skills)
        {
            SkillClass = loadoutClass;
            Skills = skills;
        }

        public bool Matches(List<int> skills)
        {
            return skills.All(Skills.Contains);
        }

        public bool IsValid()
        {
            List<int> skillPool;
            switch (SkillClass)
            {
                case AvatarClass.Pilot:
                    skillPool = Util.PilotSkillIds;
                    break;
                case AvatarClass.Gunner:
                    skillPool = Util.GunnerSkillIds;
                    break;
                case AvatarClass.Engineer:
                    skillPool = Util.EngineerSkillIds;
                    break;
                default:
                    return false;
            }
            foreach (var skill in Skills)
                if (!skillPool.Contains(skill))
                    return false;
            for (var i = 0; i < Skills.Length; i++)
            {
                for (var j = i + 1; j < Skills.Length; j++)
                {
                    if (Skills[i] == Skills[j]) return false;
                }
            }
            return true;
        }

        public override string ToString()
        {
            if (SkillClass.HasValue)
            {
                var str = SkillClass.Value.ToString() + ": ";

                var toolNameList = new List<string>();
                foreach (var skill in Skills)
                {
                    if (CachedRepository.Instance.Get<SkillConfig>(skill) != null)
                        toolNameList.Add(CachedRepository.Instance.Get<SkillConfig>(skill).GetLocalizedName());
                    else
                        toolNameList.Add("[none]");
                }

                str += string.Join(",", toolNameList.ToArray());

                return str;
            }
            else return "invalid";
        }
    }
}
