using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Muse.Goi2.Entity;
using UnityEngine;

namespace BuffKit.LoadoutDnd
{
    public class LoadoutDnd : MonoBehaviour
    {
        public static SortedDictionary<int, SkillConfig> Skills = new SortedDictionary<int, SkillConfig>();

        public static Action<AvatarClass, IList<int>> AcceptCallback = (clazz, ints) =>
        {
            SubDataActions.MyLoadoutQuery(delegate(LoadoutQueryData data)
            {
                var skillInfos =
                    data.GetSkills(clazz, data.CurrentSkillSet[clazz]).Where(
                        info => Skills.ContainsValue(info.Skill));

                MuseLog.Info("=========DATA=========");
                foreach (var info in skillInfos)
                {
                    MuseLog.Info(info.SimpleString);
                    MuseLog.Info(info.Skill.Name);
                }
                
                MuseLog.Info("=========CURRENT SKILLS=========");
                foreach (var skill in Skills)
                {
                    MuseLog.Info(skill.Key.ToString());
                    MuseLog.Info(skill.Value.Name);
                }

                var dab = new Dictionary<SkillType, List<SkillInfo>>();

                foreach (var info in skillInfos)
                {
                    if (!dab.ContainsKey(info.SkillType))
                    {
                        dab.Add(info.SkillType, new List<SkillInfo>());
                    }
                    
                    dab[info.SkillType].Add(info);
                }

                foreach (var uh in dab)
                {
                    MuseLog.Info(uh.Key.ToString());
                    var newSkills = Skills.Where(s => uh.Value.Find(a => a.Skill == s.Value) != null).ToList();
                    
                    foreach (var info in uh.Value)
                    {
                        MuseLog.Info(info.SimpleString);
                        var index = uh.Value.IndexOf(info);
                        info.Skill = newSkills[index].Value;
                        info.Changed = true;
                        MuseLog.Info(info.SimpleString);

                        MuseLog.Info("-----------");
                    }
                }

                var changed = data.GetChangedSkill();
                foreach (var c in changed)
                {
                    MuseLog.Info(c.Value);
                }
                SubDataActions.ChangeSkills(
                    changed, response => { MuseLog.Info("pain"); }
                    );
            });

            SubDataActions.OnAcceptLoadout -= AcceptCallback;
        };
    }
}