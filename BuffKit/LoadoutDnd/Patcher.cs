using System;
using System.Collections.Generic;
using BuffKit.UI;
using HarmonyLib;
using Muse.Goi2.Entity;
using UnityEngine;

namespace BuffKit.LoadoutDnd
{
    [HarmonyPatch(typeof(UIManager.UINewCharacterState), "Enter")]
    public class UINewCharacterState_Enter
    {
        public static void Postfix(LoadoutQueryData ___data)
        {
            var cc = UIPageFrame.Instance.characterCustomizer.loadoutBlock;
            var skillGroups = new GameObject[]
            {
                UIPageFrame.Instance.characterCustomizer.pilotSkillContainer,
                UIPageFrame.Instance.characterCustomizer.engineerSkillContainer,
                UIPageFrame.Instance.characterCustomizer.gunnerSkillContainer,
                UIPageFrame.Instance.characterCustomizer.specialSkillContainer
            };

            foreach (var skillGroup in skillGroups)
            {
                var handler = skillGroup.GetComponent<DropHandler>();

                if (handler != null) continue;
                
                handler = skillGroup.AddComponent<DropHandler>();
                handler.OnDropped += (previous, current) =>
                {
                    var slotOffset = int.MaxValue;

                    for (int i = 0; i < current.Count; i++)
                    {
                        var ss = current.Values[i].GetComponent<UICharacterSkillSlot>();
                        if (ss.Skill.Slot < slotOffset) slotOffset = ss.Skill.Slot;
                    }
                    
                    for (int i = 0; i < current.Count; i++)
                    {
                        var ss = current.Values[i].GetComponent<UICharacterSkillSlot>();

                        ss.Skill.Slot = slotOffset + i;
                        ss.Skill.Changed = true;
                        ss.Skill.Enabled = true;
                    }
                };
            }
        }
    }

    [HarmonyPatch(typeof(UINewAcceptLoadoutDialog), "Show")]
    public class UINewAcceptLoadoutDialog_Show
    {
        public static void Postfix(IList<SkillConfig> ___skills)
        {
            var bg = UINewAcceptLoadoutDialog.Instance.transform.GetChild(2);

            if (bg.gameObject.GetComponent<DropHandler>() is null)
            {
                var dh = bg.gameObject.AddComponent<DropHandler>();
                dh.OnDropped +=
                    (previous, current) =>
                    {
                        var oldSkills = new List<SkillConfig>(___skills);
                        MuseLog.Info("OLD SKILLS LENGTH");
                        MuseLog.Info(oldSkills.Count.ToString());

                        LoadoutDnd.Skills.Clear();
                        foreach (var kvp in current)
                        {
                            MuseLog.Info("PROCESSING SKILL");
                            var oldIndex = previous.Keys[previous.IndexOfValue(kvp.Value)];
                            var newIndex = current.IndexOfKey(kvp.Key);
                            
                            MuseLog.Info(oldIndex.ToString());
                            MuseLog.Info(newIndex.ToString());
                            
                            try
                            {
                                LoadoutDnd.Skills.Add(newIndex, oldSkills[oldIndex]);
                            }
                            catch (Exception e)
                            {
                                MuseLog.Info(e.ToString());
                            }
                        }

                        SubDataActions.OnAcceptLoadout += LoadoutDnd.AcceptCallback;
                    };
            }
        }
    }
}