using System.Linq;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using static BuffKit.Util;
using static UIManager;

namespace BuffKit
{
    [BepInPlugin("me.trgk.buffkit", "Buff Kit", "0.0.1")]
    public class BuffKit : BaseUnityPlugin
    {
        private void Awake()
        {
            var log = BepInEx.Logging.Logger.CreateLogSource("buffkit");
            var harmony = new Harmony("BuffKit");

            var asm = Assembly.GetAssembly(typeof(UIManager));

            var mmsType = asm.GetTypes().First(t => t.Name.Equals("UIMatchMenuState"));
            var mlsType = asm.GetTypes().First(t => t.Name.Equals("UINewMatchLobbyState"));

            var modFeaturesOriginal = mmsType.GetMethods().First(m => m.Name.Equals("ModFeatures"));
            var modFeaturesPatch = typeof(BuffKit).GetMethods().First(m => m.Name.Equals("ModFeatures"));
            harmony.Patch(modFeaturesOriginal, new HarmonyMethod(modFeaturesPatch));

            var paintButtonsOriginal = mlsType.GetMethods().First(m => m.Name.Equals("PaintFooterButtons"));
            var paintButtonsPatch = typeof(BuffKit).GetMethods().First(m => m.Name.Equals("PaintFooterButtons"));
            harmony.Patch(paintButtonsOriginal, new HarmonyMethod(paintButtonsPatch));
            
            harmony.PatchAll();
            log.LogInfo("Buff applied!");
        }

        public static bool ModFeatures()
        {
            TransitionToState(UIModMenuState.Instance);
            return false;
        }

        public static bool PaintFooterButtons()
        {
            var mlv = MatchLobbyView.Instance;
            if (mlv == null || NetworkedPlayer.Local == null) return false;
            var footer = UIPageFrame.Instance.footer;
            footer.ClearButtons();

            if (!HasModPrivilege(mlv)) return false;

            footer.AddButton("CHANGE MAP", delegate { MapPicker.Paint(); });
            footer.AddButton("MOD MATCH", delegate { UINewMatchLobbyState.instance.ModFeatures(); });

            return false;
        }
    }
}