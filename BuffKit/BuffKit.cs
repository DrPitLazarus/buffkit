using BepInEx;
using HarmonyLib;

namespace BuffKit
{
    [BepInPlugin("me.trgk.buffkit", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
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