using BepInEx;
using HarmonyLib;

namespace BuffKit
{
    [BepInPlugin("me.trgk.buffkit", "Buff Kit", "0.0.1")]
    public class BuffKit : BaseUnityPlugin
    {
        private void Awake()
        {
            var log = BepInEx.Logging.Logger.CreateLogSource("buffkit");
            var harmony = new Harmony("BuffKit");
            
            harmony.PatchAll();
            log.LogInfo("Buff applied!");
        }
    }
}