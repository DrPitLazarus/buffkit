﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using BuffKit.UI;
using Muse.Goi2.Entity;

namespace BuffKit.LoadoutSort
{
    class UILoadoutSortPanel : MonoBehaviour
    {
        public static UILoadoutSortPanel Instance;
        public static void _Initialize()
        {
            var parentTransform = GameObject.Find("/Menu UI/Standard Canvas/Common Elements")?.transform;
            if (parentTransform == null) MuseLog.Error("Panel parent transform was not found");
            UILoadoutSortPanel.BuildPanel(parentTransform);
            Instance.SetVisibility(false);

            Settings.Settings.Instance.AddEntry("loadout manager", "change order", delegate (Settings.Dummy _) { Instance.SetVisibility(true); }, new Settings.Dummy());
            Settings.Settings.Instance.AddEntry("loadout manager", "sort recommended loadouts", v => Instance.doSort = v, Instance.doSort);

            SubDataActions.OnAcceptLoadout += delegate (AvatarClass clazz, IList<int> skills)
            {
                if (!Instance.doSort) return;
                Instance.SortSkills(skills, out var ps, out var gs, out var es);
                var sortedSkillString = Instance.GetSkillChangeString(clazz, ps, gs, es);
                var skillDict = new Dictionary<string, string>();
                skillDict.Add("skill", sortedSkillString);
                SubDataActions.ChangeSkills(skillDict);
            };
        }

        public static void BuildPanel(Transform parent)
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

            // Load skills from file
            LoadoutSort.LoadFromFile(out var pilotSkills, out var gunnerSkills, out var engineerSkills);
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
            if (skillsAdded)
            {
                MuseLog.Info("Added some missing skills, saving new order");
                LoadoutSort.SaveToFile(pilotSkills, gunnerSkills, engineerSkills);
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
                Instance.SaveToolOrders();
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
                Instance.SaveToolOrders();
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
                Instance.SaveToolOrders();
            };

            UI.Resources.RegisterSkillTextureCallback(delegate { Instance.RefreshTextures(); });

            var btnRow = new GameObject("Save Row");
            hlg = btnRow.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.spacing = 10;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            btnRow.transform.SetParent(panel.transform);

            var closeBtn = UI.Builder.BuildButton(btnRow.transform, delegate
            {
                Instance.SetVisibility(false);
            }, "Close", TextAnchor.MiddleCenter, 24);
            var le = closeBtn.AddComponent<LayoutElement>();
            le.preferredWidth = 90;
            le.preferredHeight = 40;

            Instance = panel;
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

        private void SaveToolOrders()
        {
            LoadoutSort.SaveToFile(_pilotToolOrder, _gunnerToolOrder, _engineerToolOrder);
        }

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

        public void SetVisibility(bool visible)
        {
            gameObject.SetActive(visible);
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
