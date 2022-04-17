using System.Collections.Generic;
using HarmonyLib;
using Muse.Goi2.Entity;

namespace BuffKit.AutoAcceptLoadout
{
    [HarmonyPatch(typeof(UINewAcceptLoadoutDialog), "Show")]
    class UINewAcceptLoadoutDialog_Show
    {
        private static bool Prefix(
            UINewAcceptLoadoutDialog __instance, ref int ___captainId, ref AvatarClass ___clazz, ref IList<SkillConfig> ___skills,
            int captainId, string captainName, AvatarClass clazz, IList<SkillConfig> skills)
        {
            ___captainId = captainId;
            ___clazz = clazz;
            ___skills = skills;
            __instance.gameObject.SetActive(true);

            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            typeof(UINewAcceptLoadoutDialog).GetMethod("Accept", flags).Invoke(__instance, null);

            return false;
        }
    }
}
