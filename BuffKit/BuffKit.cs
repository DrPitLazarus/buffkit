using BepInEx;
using HarmonyLib;

namespace BuffKit
{
    [BepInPlugin("me.trgk.buffkit", "Buff Kit", "0.0.2")]
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