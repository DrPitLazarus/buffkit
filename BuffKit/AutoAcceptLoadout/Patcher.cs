using System.Collections.Generic;
using HarmonyLib;
using Muse.Goi2.Entity;

namespace BuffKit.AutoAcceptLoadout
{
    [HarmonyPatch(typeof(UINewAcceptLoadoutDialog), "Show")]
    class UINewAcceptLoadoutDialog_Show
    {
        private static bool enableAutoAccept = false;

        private static bool firstPrepare = true;
        private static void Prepare()
        {
            if (firstPrepare)
            {
                Settings.Settings.Instance.AddEntry("misc", "auto accept loadouts", v => enableAutoAccept = v, enableAutoAccept);
                firstPrepare = false;
            }
        }
        private static bool Prefix(
            UINewAcceptLoadoutDialog __instance, ref int ___captainId, ref AvatarClass ___clazz, ref IList<SkillConfig> ___skills,
            int captainId, string captainName, AvatarClass clazz, IList<SkillConfig> skills)
        {
            if (!enableAutoAccept) return true;

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
