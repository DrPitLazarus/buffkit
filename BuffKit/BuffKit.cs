using BepInEx;
using HarmonyLib;

namespace BuffKit
{
    [BepInPlugin("me.trgk.buffkit", "Buff Kit", "364.1.0")]
    public class BuffKit : BaseUnityPlugin
    {
        private void Awake()
        {
            var harmony = new Harmony("BuffKit");
            
            harmony.PatchAll();
            MuseLog.Info("Buff applied!");
        }
    }
}