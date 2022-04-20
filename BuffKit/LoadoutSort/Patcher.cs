using HarmonyLib;
using System.Collections.Generic;
using Muse.Goi2.Entity;

namespace BuffKit.LoadoutSort
{
    [HarmonyPatch(typeof(LoadoutQueryData), "GetChangedSkill")]
    class LoadoutQueryData_GetChangedSkills
    {
        static bool firstCall = true;

        private static void Prepare()
        {
            if (firstCall)
            {
                firstCall = false;
                Util.OnGameInitialize += UILoadoutSortPanel._Initialize;
            }
        }
    }

    // TODO: add description label to panel
    //       specific class toolset overrides
    //          UI panel button to open new panel (vertical scrollbar, add new (+), delete (-?))
    //       remove LoadoutSort and move behaviour into UILoadoutSortPanel?
}