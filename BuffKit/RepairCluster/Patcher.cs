using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace BuffKit.RepairCluster
{
    [HarmonyPatch]
    public class RepairCluster : MonoBehaviour
    {
        private static readonly int _numberOfGuns = 6;
        private static readonly int _numberOfEngines = 4;
        private static readonly int _gridHeight = 42;
        private static readonly int _gridWidth = 32;
        private static readonly Color _colorRedDestroyed = new(1, 0, 0, 1);
        private static readonly Color _colorOrangeDamaged = new(1, 0.5f, 0, 0.7f);
        private static readonly Color _colorWhite = new(1, 1, 1, 0.5f);
        private static readonly Color _colorWhiteTransparent = new(1, 1, 1, 0.3f);
        private static readonly RectOffset _progressBarPadding = new(2, 2, 0, 0);
        private static readonly List<Indicator> _indicators = [];

        private static bool _firstMainMenuState = true;
        private static int _indexBalloon = -1;
        private static int _indexArmor = -1;
        private static List<int> _indexGuns = [];
        private static List<int> _indexEngines = [];

        [HarmonyPatch(typeof(UIManager.UINewMainMenuState), nameof(UIManager.UINewMainMenuState.Enter))]
        [HarmonyPostfix]
        private static void Start()
        {
            if (!_firstMainMenuState) return;
            _firstMainMenuState = false;
            CreateUi();
        }

        private void LateUpdate()
        {
            if (NetworkedPlayer.Local?.CurrentShip == null) return;

            UpdateIndicators();
        }

        private static void UpdateIndicators()
        {
            var repairables = NetworkedPlayer.Local.CurrentShip.Repairables;
            _indexBalloon = -1;
            _indexArmor = -1;
            _indexGuns = [];
            _indexEngines = [];

            for (var index = 0; index < repairables.Count; index++)
            {
                var repairable = repairables[index];
                if (repairable as Turret != null) { _indexGuns.Add(index); continue; }
                if (repairable as Engine != null) { _indexEngines.Add(index); continue; }
                if (repairable as Balloon != null) { _indexBalloon = index; continue; }
                if (repairable as Hull != null) { _indexArmor = index; continue; }
            }

            var gunsDone = 0;
            var enginesDone = 0;

            for (var index = 0; index < _indicators.Count; index++)
            {
                var indicator = _indicators[index];
                var name = indicator.Name.ToLower();
                if (name.Contains("gun"))
                {
                    if (_indexGuns.Count > gunsDone)
                    {
                        indicator.Active = true;
                        var repairableIndex = _indexGuns[gunsDone];
                        indicator.SetHealth(repairables[repairableIndex].NormalizedHealth);
                        gunsDone++;
                        continue;
                    }
                    indicator.Active = false;
                    continue;
                }
                if (name.Contains("engine"))
                {
                    if (_indexEngines.Count > enginesDone)
                    {
                        indicator.Active = true;
                        var repairableIndex = _indexEngines[enginesDone];
                        indicator.SetHealth(repairables[repairableIndex].NormalizedHealth);
                        enginesDone++;
                        continue;
                    }
                    indicator.Active = false;
                    continue;
                }
                if (name.Contains("balloon"))
                {
                    if (_indexBalloon > -1)
                    {
                        indicator.Active = true;
                        indicator.SetHealth(repairables[_indexBalloon].NormalizedHealth);
                        continue;
                    }
                    indicator.Active = false;
                    continue;
                }
                if (name.Contains("armor"))
                {
                    if (_indexArmor > -1)
                    {
                        indicator.Active = true;
                        indicator.SetHealth(repairables[_indexArmor].NormalizedHealth);
                        continue;
                    }
                    indicator.Active = false;
                    continue;
                }
            }
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

            mainObject.AddComponent<RepairCluster>();
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

            CreateProgressBar("Health", 4, progressBarsObject.transform, out LayoutElement healthBarLe, out RawImage healthBarRawImage);

            var indicator = new Indicator
            {
                Name = name,
                IndicatorObject = indicatorObject,
                ComponentTexture = componentTexture,
                IconRawImage = rawImage,
                HealthBarLayoutElement = healthBarLe,
                HealthBarRawImage = healthBarRawImage,
            };
            _indicators.Add(indicator);
            return indicatorObject;
        }

        private static GameObject CreateProgressBar(string name, int height, Transform parentTransform, out LayoutElement layoutElement, out RawImage rawImage)
        {
            var progressBarObject = new GameObject($"Progress Bar {name}");
            progressBarObject.transform.SetParent(parentTransform, false);
            layoutElement = progressBarObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = height;
            layoutElement.preferredWidth = _gridWidth - _progressBarPadding.horizontal;
            rawImage = progressBarObject.AddComponent<RawImage>();
            rawImage.color = _colorWhite;
            return progressBarObject;
        }

        private class Indicator
        {
            public string Name;
            public GameObject IndicatorObject;
            public Texture2D ComponentTexture;
            public RawImage IconRawImage;
            public LayoutElement HealthBarLayoutElement;
            public RawImage HealthBarRawImage;

            public bool Active { get => IndicatorObject.activeSelf; set { if (IndicatorObject.activeSelf != value) IndicatorObject.SetActive(value); } }

            public void SetHealth(float percentage)
            {
                // Set color.
                if (percentage == 0f)
                {
                    IconRawImage.color = _colorRedDestroyed;
                }
                else if (percentage <= .5f)
                {
                    IconRawImage.color = _colorOrangeDamaged;
                    HealthBarRawImage.color = _colorOrangeDamaged;
                }
                else
                {
                    IconRawImage.color = _colorWhiteTransparent;
                    HealthBarRawImage.color = _colorWhiteTransparent;
                }
                // Hide bar if health is full.
                if (percentage == 1f) percentage = 0;
                HealthBarLayoutElement.preferredWidth = percentage * (_gridWidth - _progressBarPadding.horizontal);
            }

            public void SetIconNormal()
            {
                IconRawImage.texture = ComponentTexture;
            }

            public void SetIconFire()
            {
                IconRawImage.texture = UIRepairComponentView.instance.fireNormal;
            }

            public void SetIconFireBig()
            {
                IconRawImage.texture = UIRepairComponentView.instance.fire;
            }

            public override string ToString()
            {
                return $"{Name}";
            }
        }
    }
}