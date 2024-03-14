using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Muse.Goi2.Entity;
using BuffKit.UI;
using BuffKit.Settings;

namespace BuffKit.LoadoutSort
{
    class UILoadoutSortPanel : MonoBehaviour
    {
        enum PreferredSpottingTool
        {
            CaptainsChoice, Spyglass, Rangefinder
        }
        static PreferredSpottingTool PreferredSpottingToolSettings = PreferredSpottingTool.CaptainsChoice;

        public static UILoadoutSortPanel Instance;
        public static void _Initialize()
        {
            // Load skills from file
            LoadoutSort.LoadFromFile(out var pilotSkills, out var gunnerSkills, out var engineerSkills, out var specificSkillSets);

            var parentTransform = GameObject.Find("/Menu UI/Standard Canvas/Common Elements")?.transform;
            if (parentTransform == null) MuseLog.Error("Panel parent transform was not found");

            UILoadoutSpecificSortPanel.BuildPanel(parentTransform);
            UILoadoutSpecificSortPanel.Instance.TryHide();
            UILoadoutSpecificSortPanel.Instance.AddOrders(specificSkillSets);

            UILoadoutSortPanel.BuildPanel(parentTransform, pilotSkills, gunnerSkills, engineerSkills);
            Instance.TryHide();

            UILoadoutSpecificSortPanel.Instance.transform.SetAsFirstSibling();
            UILoadoutSortPanel.Instance.transform.SetAsFirstSibling();

            Settings.Settings.Instance.AddEntry("loadout manager", "change order", delegate (Settings.Dummy _) { Instance.Show(); }, new Settings.Dummy());
            Settings.Settings.Instance.AddEntry("loadout manager", "sort recommended loadouts", v => Instance.doSort = v, Instance.doSort);

            var changeRecommendedSpottingToolDict = new Dictionary<int, string>();
            changeRecommendedSpottingToolDict.Add((int)PreferredSpottingTool.CaptainsChoice, "captain's choice");
            changeRecommendedSpottingToolDict.Add((int)PreferredSpottingTool.Spyglass, "spyglass");
            changeRecommendedSpottingToolDict.Add((int)PreferredSpottingTool.Rangefinder, "rangefinder");
            var changeRecommendedSpottingTool = new EnumString(
                typeof(PreferredSpottingTool),
                (int)PreferredSpottingToolSettings,
                changeRecommendedSpottingToolDict);
            Settings.Settings.Instance.AddEntry("loadout manager", "change recommended spotting tool", delegate (EnumString enumString)
            {
                PreferredSpottingToolSettings = (PreferredSpottingTool)enumString.SelectedValue;
            }, changeRecommendedSpottingTool);

            SubDataActions.OnAcceptLoadout += delegate (AvatarClass clazz, IList<int> skills)
            {
                if (!Instance.doSort) return;

                skills = Instance.ReplaceSpottingTool(skills);
                Instance.SortSkills(skills, out var ps, out var gs, out var es);

                switch (clazz)
                {
                    case AvatarClass.Pilot:
                        UILoadoutSpecificSortPanel.Instance.TrySort(ps, out ps);
                        break;
                    case AvatarClass.Gunner:
                        UILoadoutSpecificSortPanel.Instance.TrySort(gs, out gs);
                        break;
                    case AvatarClass.Engineer:
                        UILoadoutSpecificSortPanel.Instance.TrySort(es, out es);
                        break;
                }

                var sortedSkillString = Instance.GetSkillChangeString(clazz, ps, gs, es);
                var skillDict = new Dictionary<string, string>();
                skillDict.Add("skill", sortedSkillString);
                SubDataActions.ChangeSkills(skillDict);
            };
        }

        private static void BuildPanel(Transform parent, List<int> pilotSkills, List<int> gunnerSkills, List<int> engineerSkills)
        {
            var obPanel = Builder.BuildPanel(parent);
            obPanel.name = "UI Loadout Sort Panel";
            var rt = obPanel.GetComponent<RectTransform>();
            rt.pivot = new Vector2(.5f, .5f);
            rt.anchorMin = new Vector2(.5f, .5f);
            rt.anchorMax = new Vector2(.5f, .5f);
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);
            var vlg = obPanel.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(5, 5, 5, 5);
            vlg.spacing = 10;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            var csf = obPanel.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var panel = obPanel.AddComponent<UILoadoutSortPanel>();
            obPanel.AddComponent<GraphicRaycaster>();                       // Makes it have UI interaction on top of other UI (I think? It's complicated)

            Builder.BuildLabel(obPanel.transform, "Loadout Sorting Settings", TextAnchor.MiddleCenter, 32);

            Builder.BuildLabel(obPanel.transform, out var text, UI.Resources.FontGaldeanoRegular, TextAnchor.MiddleCenter, 16);
            text.text = "Rearrange default loadout sorting order by dragging and dropping the tools below.\nSpecific tool combinations to override this order can be added.";

            // Add any missing skills
            bool skillsAdded = false;
            foreach (var skill in Util.PilotSkillIds)
                if (!pilotSkills.Contains(skill))
                {
                    pilotSkills.Add(skill);
                    skillsAdded = true;
                }
            foreach (var skill in Util.GunnerSkillIds)
                if (!gunnerSkills.Contains(skill))
                {
                    gunnerSkills.Add(skill);
                    skillsAdded = true;
                }
            foreach (var skill in Util.EngineerSkillIds)
                if (!engineerSkills.Contains(skill))
                {
                    engineerSkills.Add(skill);
                    skillsAdded = true;
                }

            panel._pilotToolOrder = pilotSkills;
            panel._gunnerToolOrder = gunnerSkills;
            panel._engineerToolOrder = engineerSkills;

            // Add content
            var obRowPilot = new GameObject("Pilot Tools");
            var hlg = obRowPilot.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 5;
            foreach (var skill in pilotSkills)
            {
                var label = Builder.BuildImageLabel(obRowPilot.transform, out var im, 70, 70);
                label.name = CachedRepository.Instance.Get<SkillConfig>(skill).NameText.En;
                panel._pilotToolImages.Add(im);
                panel._transformNameToPilotSkillId.Add(label.name, skill);
            }
            obRowPilot.transform.SetParent(panel.transform);
            obRowPilot.AddComponent<DropHandler>().OnDropped += delegate (SortedList<int, Transform> originalOrder, SortedList<int, Transform> newOrder)
            {
                Instance._pilotToolOrder = new List<int>(newOrder.Count);
                foreach (var kvp in newOrder)
                {
                    Instance._pilotToolOrder.Add(Instance._transformNameToPilotSkillId[kvp.Value.name]);
                }
                LoadoutSort.SaveToFile();
            };

            var obRowGunner = new GameObject("Gunner Tools");
            hlg = obRowGunner.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 5;
            foreach (var skill in gunnerSkills)
            {
                var label = Builder.BuildImageLabel(obRowGunner.transform, out var im, 70, 70);
                label.name = CachedRepository.Instance.Get<SkillConfig>(skill).NameText.En;
                panel._gunnerToolImages.Add(im);
                panel._transformNameToGunnerSkillId.Add(label.name, skill);
            }
            obRowGunner.transform.SetParent(panel.transform);
            obRowGunner.AddComponent<DropHandler>().OnDropped += delegate (SortedList<int, Transform> originalOrder, SortedList<int, Transform> newOrder)
            {
                Instance._gunnerToolOrder = new List<int>(newOrder.Count);
                foreach (var kvp in newOrder)
                {
                    Instance._gunnerToolOrder.Add(Instance._transformNameToGunnerSkillId[kvp.Value.name]);
                }
                LoadoutSort.SaveToFile();
            };

            var obRowEngineer = new GameObject("Engineer Tools");
            hlg = obRowEngineer.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.spacing = 5;
            foreach (var skill in engineerSkills)
            {
                var label = Builder.BuildImageLabel(obRowEngineer.transform, out var im, 70, 70);
                label.name = CachedRepository.Instance.Get<SkillConfig>(skill).NameText.En;
                panel._engineerToolImages.Add(im);
                panel._transformNameToEngineerSkillId.Add(label.name, skill);
            }
            obRowEngineer.transform.SetParent(panel.transform);
            obRowEngineer.AddComponent<DropHandler>().OnDropped += delegate (SortedList<int, Transform> originalOrder, SortedList<int, Transform> newOrder)
            {
                Instance._engineerToolOrder = new List<int>(newOrder.Count);
                foreach (var kvp in newOrder)
                {
                    Instance._engineerToolOrder.Add(Instance._transformNameToEngineerSkillId[kvp.Value.name]);
                }
                LoadoutSort.SaveToFile();
            };

            UI.Resources.RegisterSkillTextureCallback(delegate { Instance.RefreshTextures(); });

            var btnRow = new GameObject("Bottom Row");
            hlg = btnRow.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            btnRow.transform.SetParent(panel.transform);

            var specificBtn = Builder.BuildButton(btnRow.transform, delegate
              {
                  UILoadoutSpecificSortPanel.Instance.Show();
              }, "Specific Orders", TextAnchor.MiddleCenter, 24);
            var le = specificBtn.AddComponent<LayoutElement>();
            le.preferredWidth = 215;
            le.preferredHeight = 40;

            var closeBtn = Builder.BuildButton(btnRow.transform, delegate
            {
                Instance.TryHide();
            }, "Close", TextAnchor.MiddleCenter, 24);
            le = closeBtn.AddComponent<LayoutElement>();
            le.preferredWidth = 90;
            le.preferredHeight = 40;

            var obDropShadow = GameObject.Instantiate(GameObject.Find("Menu UI/Standard Canvas/Common Elements/Item Selection Window/Item Selection Panel/Drop Shadow"), panel.transform);
            rt = obDropShadow.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(.5f, .5f);
            rt.anchorMax = new Vector2(.5f, .5f);
            rt.offsetMin = new Vector2(-785, -389);
            rt.offsetMax = new Vector2(785, 389);
            obDropShadow.transform.SetAsFirstSibling();

            Instance = panel;

            if (skillsAdded)
            {
                MuseLog.Info("Added some missing skills, saving new order");
                LoadoutSort.SaveToFile();
            }
        }

        private void PrintToolOrders()
        {
            MuseLog.Info("New Tool Orders");
            MuseLog.Info("  Pilot Tools Order");
            foreach (var skill in _pilotToolOrder)
                MuseLog.Info("    " + CachedRepository.Instance.Get<SkillConfig>(skill).NameText.En);
            MuseLog.Info("  Gunner Tools Order");
            foreach (var skill in _gunnerToolOrder)
                MuseLog.Info("    " + CachedRepository.Instance.Get<SkillConfig>(skill).NameText.En);
            MuseLog.Info("  Engineer Tools Order");
            foreach (var skill in _engineerToolOrder)
                MuseLog.Info("    " + CachedRepository.Instance.Get<SkillConfig>(skill).NameText.En);
        }

        public List<int> PilotToolOrder { get { return _pilotToolOrder; } }
        public List<int> GunnerToolOrder { get { return _gunnerToolOrder; } }
        public List<int> EngineerToolOrder { get { return _engineerToolOrder; } }

        private bool doSort = false;

        private Dictionary<string, int> _transformNameToPilotSkillId = new Dictionary<string, int>();
        private Dictionary<string, int> _transformNameToGunnerSkillId = new Dictionary<string, int>();
        private Dictionary<string, int> _transformNameToEngineerSkillId = new Dictionary<string, int>();

        private List<RawImage> _pilotToolImages = new List<RawImage>();
        private List<RawImage> _gunnerToolImages = new List<RawImage>();
        private List<RawImage> _engineerToolImages = new List<RawImage>();

        private List<int> _pilotToolOrder = new List<int>();
        private List<int> _gunnerToolOrder = new List<int>();
        private List<int> _engineerToolOrder = new List<int>();

        private void RefreshTextures()
        {
            for (var i = 0; i < _pilotToolImages.Count; i++)
            {
                var id = _transformNameToPilotSkillId[_pilotToolImages[i].transform.parent.name];
                _pilotToolImages[i].texture = UI.Resources.GetSkillTexture(id);
            }
            for (var i = 0; i < _gunnerToolImages.Count; i++)
            {
                var id = _transformNameToGunnerSkillId[_gunnerToolImages[i].transform.parent.name];
                _gunnerToolImages[i].texture = UI.Resources.GetSkillTexture(id);
            }
            for (var i = 0; i < _engineerToolImages.Count; i++)
            {
                var id = _transformNameToEngineerSkillId[_engineerToolImages[i].transform.parent.name];
                _engineerToolImages[i].texture = UI.Resources.GetSkillTexture(id);
            }
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

        public void SortSkills(IList<int> skills, out List<int> sortedPilotSkills, out List<int> sortedGunnerSkills, out List<int> sortedEngineerSkills)
        {
            var pilotSkills = new List<int>();
            var gunnerSkills = new List<int>();
            var engineerSkills = new List<int>();

            foreach (var id in skills)
            {
                var skillConfig = CachedRepository.Instance.Get<SkillConfig>(id);
                if (skillConfig != null)
                    switch (skillConfig.Type)
                    {
                        case SkillType.Helm:
                            pilotSkills.Add(id);
                            break;
                        case SkillType.Gun:
                            gunnerSkills.Add(id);
                            break;
                        case SkillType.Repair:
                            engineerSkills.Add(id);
                            break;
                        default:
                            MuseLog.Warn("Failed to sort unexpected skill: (" + id + ") " + skillConfig.NameText.En);
                            break;
                    }
            }

            sortedPilotSkills = pilotSkills.OrderBy(id => _pilotToolOrder.IndexOf(id)).ToList();
            sortedGunnerSkills = gunnerSkills.OrderBy(id => _gunnerToolOrder.IndexOf(id)).ToList();
            sortedEngineerSkills = engineerSkills.OrderBy(id => _engineerToolOrder.IndexOf(id)).ToList();
        }
        
        public IList<int> ReplaceSpottingTool(IList<int> skills)
        {
            int spyglassId = 4;
            int rangefinderId = 27;
            switch(PreferredSpottingToolSettings)
            {
                case PreferredSpottingTool.Spyglass:
                    if (skills.Contains(rangefinderId) && !skills.Contains(spyglassId))
                        skills[skills.IndexOf(rangefinderId)] = spyglassId;
                    break;
                case PreferredSpottingTool.Rangefinder:
                    if (skills.Contains(spyglassId) && !skills.Contains(rangefinderId))
                        skills[skills.IndexOf(spyglassId)] = rangefinderId;
                    break;
                case PreferredSpottingTool.CaptainsChoice:
                    break;
            }
            return skills;
        }

        public string GetSkillChangeString(AvatarClass clazz, List<int> pilotSkills, List<int> gunnerSkills, List<int> engineerSkills)
        {
            var skillEntries = new List<string>();

            switch (clazz)
            {
                case AvatarClass.Pilot:
                    skillEntries.Add("Pilot/-1/0/" + pilotSkills[0]);
                    skillEntries.Add("Pilot/-1/1/" + pilotSkills[1]);
                    skillEntries.Add("Pilot/-1/2/" + pilotSkills[2]);
                    skillEntries.Add("Pilot/-1/3/" + gunnerSkills[0]);
                    skillEntries.Add("Pilot/-1/4/" + engineerSkills[0]);
                    break;
                case AvatarClass.Gunner:
                    skillEntries.Add("Gunner/-1/0/" + gunnerSkills[0]);
                    skillEntries.Add("Gunner/-1/1/" + gunnerSkills[1]);
                    skillEntries.Add("Gunner/-1/2/" + gunnerSkills[2]);
                    skillEntries.Add("Gunner/-1/3/" + pilotSkills[0]);
                    skillEntries.Add("Gunner/-1/4/" + engineerSkills[0]);
                    skillEntries.Add("Gunner/-1/6/" + engineerSkills[1]);
                    break;
                case AvatarClass.Engineer:
                    skillEntries.Add("Engineer/-1/0/" + engineerSkills[0]);
                    skillEntries.Add("Engineer/-1/1/" + engineerSkills[1]);
                    skillEntries.Add("Engineer/-1/2/" + engineerSkills[2]);
                    skillEntries.Add("Engineer/-1/3/" + pilotSkills[0]);
                    skillEntries.Add("Engineer/-1/4/" + gunnerSkills[0]);
                    break;
            }

            var str = string.Join(",", skillEntries.ToArray());

            return str;
        }
    }
}
