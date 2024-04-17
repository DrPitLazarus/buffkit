using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace BuffKit.RepairCluster
{
    [HarmonyPatch]
    public class RepairCluster
    {
        private static readonly int _numberOfGuns = 6;
        private static readonly int _numberOfEngines = 4;
        private static readonly int _gridHeight = 42;
        private static readonly int _gridWidth = 32;
        private static readonly RectOffset _progressBarPadding = new(2, 2, 0, 0);

        private static bool _firstMainMenuState = true;

        [HarmonyPatch(typeof(UIManager.UINewMainMenuState), nameof(UIManager.UINewMainMenuState.Enter))]
        [HarmonyPostfix]
        private static void Start()
        {
            if (!_firstMainMenuState) return;
            _firstMainMenuState = false;
            CreateUi();
        }

        private static void CreateUi()
        {
            var parentObject = GameObject.Find("/Game UI/Match UI/UI HUD Canvas/UI HUD/UI Ship Health Display/Pilot Skill and Buff");

            var mainObject = new GameObject("RepairCluster");
            mainObject.transform.SetParent(parentObject.transform, false);
            var glg = mainObject.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(_gridWidth, _gridHeight);
            glg.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            glg.constraintCount = 2;
            glg.startAxis = GridLayoutGroup.Axis.Vertical;
            glg.startCorner = GridLayoutGroup.Corner.UpperLeft;

            CreateIndicator("Balloon", UIRepairComponentView.instance.zeppelinBig, mainObject.transform);
            CreateIndicator("Armor", UIRepairComponentView.instance.armorBig, mainObject.transform);
            for (var index = 0; index < _numberOfGuns; index++)
            {
                CreateIndicator($"Gun {index + 1}", UIRepairComponentView.instance.gunsBig, mainObject.transform);
            }
            for (var index = 0; index < _numberOfEngines; index++)
            {
                CreateIndicator($"Engine {index + 1}", UIRepairComponentView.instance.enginesBig, mainObject.transform);
            }
        }

        private static void CreateIndicator(string name, Texture2D componentTexture, Transform parentTransform)
        {
            LayoutElement le;
            VerticalLayoutGroup vlg;

            var indicatorObject = new GameObject($"Indicator {name}");
            indicatorObject.transform.SetParent(parentTransform, false);
            vlg = indicatorObject.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = false;

            var mainIconObject = new GameObject("Main Icon");
            mainIconObject.transform.SetParent(indicatorObject.transform, false);
            le = mainIconObject.AddComponent<LayoutElement>();
            le.preferredHeight = _gridWidth;
            le.preferredWidth = _gridWidth; // Intentional to make image 1:1.
            var rawImage = mainIconObject.AddComponent<RawImage>();
            rawImage.texture = componentTexture;

            var progressBarsObject = new GameObject("Progress Bars");
            progressBarsObject.transform.SetParent(indicatorObject.transform, false);
            le = progressBarsObject.AddComponent<LayoutElement>();
            le.preferredHeight = _gridHeight - _gridWidth;
            le.preferredWidth = _gridWidth;
            vlg = progressBarsObject.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = false;
            vlg.padding = _progressBarPadding;

            CreateProgressBar("Health", 4, ProgressBarPercentToWidth(1f), progressBarsObject.transform);
            //CreateProgressBar("Buff", 3, ProgressBarPercentToWidth(.5f), progressBarsObject.transform);
            //CreateProgressBar("Cooldown", 3, ProgressBarPercentToWidth(.25f), progressBarsObject.transform);
        }

        private static void CreateProgressBar(string name, int height, float width, Transform parentTransform)
        {
            var progressBarObject = new GameObject($"Progress Bar {name}");
            progressBarObject.transform.SetParent(parentTransform, false);
            var le = progressBarObject.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.preferredWidth = width;
            progressBarObject.AddComponent<RawImage>();
        }

        private static float ProgressBarPercentToWidth(float percentage)
        {
            return percentage * (_gridWidth - _progressBarPadding.horizontal);
        }
    }
}