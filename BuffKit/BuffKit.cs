using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace BuffKit
{
    [BepInPlugin("me.trgk.buffkit", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class BuffKit : BaseUnityPlugin
    {
        public static GameObject GameObject { get; private set; }

        private void Awake()
        {
            var harmony = new Harmony(PluginInfo.PLUGIN_NAME);
            harmony.PatchAll();
            GameObject = new GameObject(PluginInfo.PLUGIN_NAME);
            MuseLog.Info("Buff applied!");
        }
    }
}