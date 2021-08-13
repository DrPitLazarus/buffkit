using System;
using System.Collections.Generic;
using System.Linq;
using BuffKit.UI;
using HarmonyLib;
using Muse.Goi2.Entity;
using UnityEngine;
using UnityEngine.UI;

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

    // [HarmonyPatch(typeof(UINewAcceptLoadoutDialog), "Show")]
    // public class UINewAcceptLoadoutDialog_OnEnable
    // {
    //     public static void Postfix(IList<SkillConfig> ___skills)
    //     {
    //         var bg = UINewAcceptLoadoutDialog.Instance.transform.GetChild(2);
    //
    //         if (bg.gameObject.GetComponent<DropHandler>() is null)
    //         {
    //             var dh = bg.gameObject.AddComponent<DropHandler>();
    //             dh.OnDropped +=
    //                 (previous, current) =>
    //                 {
    //                     var oldSkills = new List<SkillConfig>(___skills);
    //                     MuseLog.Info("OLD SKILLS LENGTH");
    //                     MuseLog.Info(oldSkills.Count.ToString());
    //                     var indices = new SortedDictionary<int, int>();
    //
    //                     MuseLog.Info("CURRENT LENGTH");
    //                     MuseLog.Info(current.Count.ToString());
    //                     
    //                     for (int i = 0; i < current.Count; i++)
    //                     {
    //                         MuseLog.Info("ADDING KEY");
    //                         MuseLog.Info(i.ToString());
    //                         indices.Add(i, current.Keys[i]);
    //                     }
    //                     
    //                     MuseLog.Info("INDICES LENGTH");
    //                     MuseLog.Info(indices.Count.ToString());
    //                     
    //                     ___skills.Clear();
    //                     oldSkills.Reverse();
    //                     foreach (var skill in oldSkills)
    //                     {
    //                         ___skills.Add(skill);
    //                     }
                        
                        // MuseLog.Info("SKILLS LENGTH");
                        // MuseLog.Info(indices.Count.ToString());
                        //
                        // foreach (var t in indices)
                        // {
                        //     MuseLog.Info("ADDING SKILL");
                        //     MuseLog.Info(t.Key.ToString());
                        //     MuseLog.Info(t.Value.ToString());
                        //     MuseLog.Info(oldSkills[t.Value].Name);
                        //     try
                        //     {
                        //         var skill = oldSkills[t.Value];
                        //         ___skills.Add(skill);
                        //     }
                        //     catch (ArgumentOutOfRangeException)
                        //     {
                        //     }
                        // }
                    };
            }
        }
    }
}