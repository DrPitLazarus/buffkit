using System.Collections.Generic;
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
        private static readonly Color _colorRed = new(1, 0, 0, 1);
        private static readonly Color _colorRedTransparent = new(1, 0, 0, 0.6f);
        private static readonly Color _colorWhite = new(1, 1, 1, 0.8f);
        private static readonly Color _colorWhiteTransparent = new(1, 1, 1, 0.5f);
        private static readonly RectOffset _progressBarPadding = new(2, 2, 0, 0);
        private static readonly List<Indicator> _indicators = [];

        private static bool _firstMainMenuState = true;

        private struct Indicator
        {
            public string Name;
            public Texture2D ComponentTexture;
            public RawImage IconRawImage;
            public LayoutElement ProgressBarLayoutElement;

            public readonly void SetHealth(float percentage)
            {
                ProgressBarLayoutElement.preferredWidth = percentage * (_gridWidth - _progressBarPadding.horizontal);
            }

            public readonly void SetIconNormal()
            {
                IconRawImage.texture = ComponentTexture;
            }

            public readonly void SetIconFire()
            {
                IconRawImage.texture = UIRepairComponentView.instance.fireNormal;
            }

            public readonly void SetIconFireBig()
            {
                IconRawImage.texture = UIRepairComponentView.instance.fire;
            }
            public readonly void SetIconColorNormal()
            {
                IconRawImage.color = _colorWhite;
            }

            public readonly void SetIconColorNormalFaded()
            {
                IconRawImage.color = _colorWhiteTransparent;
            }

            public readonly void SetIconColorDamaged()
            {
                IconRawImage.color = _colorRedTransparent;
            }

            public readonly void SetIconColorDestroyed()
            {
                IconRawImage.color = _colorRed;
            }

            public override readonly string ToString()
            {
                return $"{Name}";
            }
        }

        [HarmonyPatch(typeof(UIManager.UINewMainMenuState), nameof(UIManager.UINewMainMenuState.Enter))]
        [HarmonyPostfix]
        private static void Start()
        {
            if (!_firstMainMenuState) return;
            _firstMainMenuState = false;
            CreateUi();
        }

        private static GameObject CreateUi()
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

            return mainObject;
        }

        private static GameObject CreateIndicator(string name, Texture2D componentTexture, Transform parentTransform)
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
            le.preferredHeight = _gridWidth; // Intentional to make image 1:1.
            le.preferredWidth = _gridWidth;
            var rawImage = mainIconObject.AddComponent<RawImage>();
            rawImage.texture = componentTexture;
            rawImage.color = _colorWhite;

            var progressBarsObject = new GameObject("Progress Bars");
            progressBarsObject.transform.SetParent(indicatorObject.transform, false);
            le = progressBarsObject.AddComponent<LayoutElement>();
            le.preferredHeight = _gridHeight - _gridWidth;
            le.preferredWidth = _gridWidth;
            vlg = progressBarsObject.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = false;
            vlg.padding = _progressBarPadding;

            CreateProgressBar("Health", 4, progressBarsObject.transform, out LayoutElement progressBarLe);

            var indicator = new Indicator
            {
                Name = name,
                ComponentTexture = componentTexture,
                IconRawImage = rawImage,
                ProgressBarLayoutElement = progressBarLe,
            };
            _indicators.Add(indicator);
            return indicatorObject;
        }

        private static GameObject CreateProgressBar(string name, int height, Transform parentTransform, out LayoutElement layoutElement)
        {
            var progressBarObject = new GameObject($"Progress Bar {name}");
            progressBarObject.transform.SetParent(parentTransform, false);
            layoutElement = progressBarObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = height;
            layoutElement.preferredWidth = _gridWidth - _progressBarPadding.horizontal;
            var rawImage = progressBarObject.AddComponent<RawImage>();
            rawImage.color = _colorWhite;
            return progressBarObject;
        }
    }
}