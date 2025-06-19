using System.Collections.Generic;
using HarmonyLib;
using Muse.Goi2.Entity;
using UnityEngine;
using UnityEngine.UI;

namespace BuffKit.RepairCluster
{
    [HarmonyPatch]
    public class RepairCluster : MonoBehaviour
    {
        private static readonly int _armorKitBuffValue = 312;
        private static readonly int _bigFireStacksThreshold = 8;
        private static readonly int _numberOfGuns = 6;
        private static readonly int _numberOfEngines = 4;
        private static readonly int _gridHeight = 42;
        private static readonly int _gridWidth = 32;
        private static readonly Color _colorRedDestroyed = new(1, 0, 0, 1);
        private static readonly Color _colorOrangeDamaged = new(1, 0.5f, 0, 0.7f);
        private static readonly Color _colorWhite = new(1, 1, 1, 0.5f);
        private static readonly Color _colorWhiteTransparent = new(1, 1, 1, 0.3f);
        private static readonly Color _colorCyan = new(0, 1, 1, 0.5f);
        private static readonly Color _colorYellow = new(1, 1, 0, 0.6f);
        private static readonly RectOffset _progressBarPadding = new(2, 2, 0, 0);
        private static readonly List<Indicator> _indicators = [];

        private static bool _enabled = false;
        private static bool _showStatusFailsafe = true;
        private static bool _showStatusFireproof = true;
        private static bool _showBarRepairProgress = true;
        private static bool _showBarActiveBuff = true;
        private static bool _shouldBeEnabled = false;
        private static bool _doUpdateShouldBeEnabled = false;
        private static bool _firstMainMenuState = true;
        private static GameObject _mainObject;
        private static Texture2D _failsafeIconTexture;
        private static Texture2D _fireproofIconTexture;
        private static int _indexBalloon = -1;
        private static int _indexArmor = -1;
        private static List<int> _indexGuns = [];
        private static List<int> _indexEngines = [];

        /// <summary>
        /// Feature initialization. Create settings and <see cref="GameObject"/>.
        /// </summary>
        [HarmonyPatch(typeof(UIManager.UINewMainMenuState), nameof(UIManager.UINewMainMenuState.Enter))]
        [HarmonyPostfix]
        private static void Start()
        {
            if (!_firstMainMenuState) return;
            _firstMainMenuState = false;
            Settings.Settings.Instance.AddEntry("repair cluster", "repair cluster", v => _enabled = v, _enabled);
            Settings.Settings.Instance.AddEntry("repair cluster", "repair cluster/show status failsafe", v => _showStatusFailsafe = v, _showStatusFailsafe);
            Settings.Settings.Instance.AddEntry("repair cluster", "repair cluster/show status fireproof", v => _showStatusFireproof = v, _showStatusFireproof);
            Settings.Settings.Instance.AddEntry("repair cluster", "repair cluster/show bar repair progress", v => _showBarRepairProgress = v, _showBarRepairProgress);
            Settings.Settings.Instance.AddEntry("repair cluster", "repair cluster/show bar active buff", v => _showBarActiveBuff = v, _showBarActiveBuff);

            if (_mainObject == null)
            {
                MuseLog.Info("GameObject created!");
                _mainObject = CreateUi();
                _mainObject.SetActive(false);
            }
        }

        /// <summary>
        /// Activates the repair cluster.
        /// </summary>
        [HarmonyPatch(typeof(UIManager.UIMatchBlockState), nameof(UIManager.UIMatchBlockState.Exit))] // Normal match start.
        [HarmonyPatch(typeof(UIManager.UILoadingBlockState), nameof(UIManager.UILoadingBlockState.Exit))] // Join running match.
        [HarmonyPostfix]
        private static void UIManager_UIMatchBlockState_Exit()
        {
            if (!_enabled) return;
            _doUpdateShouldBeEnabled = true;
            _mainObject.SetActive(true);
        }

        /// <summary>
        /// Deactivates the repair cluster.
        /// </summary>
        [HarmonyPatch(typeof(Mission), nameof(Mission.OnDisable))]
        [HarmonyPostfix]
        private static void Mission_OnDisable()
        {
            if (_mainObject != null && _mainObject.activeSelf) _mainObject.SetActive(false);
        }

        /// <summary>
        /// Main <see cref="GameObject"/> update loop.
        /// </summary>
        private void LateUpdate()
        {
            if (_doUpdateShouldBeEnabled && Mission.Instance != null)
            {
                _doUpdateShouldBeEnabled = false;
                LoadIcons();
                UpdateShouldBeEnabled();
            }
            if (!_doUpdateShouldBeEnabled && !_shouldBeEnabled)
            {
                _mainObject.SetActive(false);
                return;
            }

            // Hide indicators if ship doesn't exist.
            if (NetworkedPlayer.Local?.CurrentShip == null)
            {
                for (var index = 0; index < _indicators.Count; index++)
                {
                    _indicators[index].Active = false;
                }
                return;
            }

            UpdateIndicators();
        }

        /// <summary>
        /// The game unloads resources when starting a match. Call this after the match starts to load and apply textures.
        /// </summary>
        private static void LoadIcons()
        {
            _failsafeIconTexture = Resources.Load<Texture2D>("goi_resource_icon_medicine");
            _fireproofIconTexture = Resources.Load<Texture2D>("goi_resource_icon_water");
            foreach (var indicator in _indicators)
            {
                indicator.FailsafeIconRawImage.texture = _failsafeIconTexture;
                indicator.FireproofIconRawImage.texture = _fireproofIconTexture;
            }
        }

        /// <summary>
        /// Determines if the repair cluster should be active. Triggered by <see cref="_doUpdateShouldBeEnabled"/> flag in <see cref="LateUpdate"/>.
        /// </summary>
        private static void UpdateShouldBeEnabled()
        {
            if (Mission.Instance == null)
            {
                MuseLog.Info("UpdateShouldBeEnabled(): Mission.Instance is null!");
                return;
            }
            var currentGameMode = Mission.Instance.Map.GameMode;
            var isCoop = RegionGameModeUtil.IsCoop(currentGameMode);
            var isPractice = Util.PracticeGameModes.Contains(currentGameMode);
            var shouldBeEnabled = isCoop || isPractice;
            MuseLog.Info($"_shouldBeEnabled: {shouldBeEnabled}, isCoop: {isCoop} || isPractice: {isPractice}");
            _shouldBeEnabled = shouldBeEnabled = true;
        }

        /// <summary>
        /// Finds the indexes of each component type and apply data to the indicators.
        /// </summary>
        private static void UpdateIndicators()
        {
            // Method assumes CurrentShip exists.
            var repairables = NetworkedPlayer.Local.CurrentShip.Repairables;
            // Reset indexes.
            _indexBalloon = -1;
            _indexArmor = -1;
            _indexGuns = [];
            _indexEngines = [];
            // Find the indexes of each repairable type.
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
            // Update the indicators using the found indexes.
            for (var index = 0; index < _indicators.Count; index++)
            {
                var indicator = _indicators[index];
                var indicatorName = indicator.Name.ToLower();
                var repairableIndex = -1;

                if (indicatorName.Contains("gun") && _indexGuns.Count > gunsDone)
                {
                    repairableIndex = _indexGuns[gunsDone];
                    gunsDone++;
                }
                else if (indicatorName.Contains("engine") && _indexEngines.Count > enginesDone)
                {
                    repairableIndex = _indexEngines[enginesDone];
                    enginesDone++;
                }
                else if (indicatorName.Contains("balloon") && _indexBalloon > -1)
                {
                    repairableIndex = _indexBalloon;
                }
                else if (indicatorName.Contains("armor") && _indexArmor > -1)
                {
                    repairableIndex = _indexArmor;

                }
                // Apply data to indicator or deactivate it.
                if (repairableIndex > -1)
                {
                    var repairable = repairables[repairableIndex];
                    indicator.Active = true;
                    indicator.SetFireStacks(repairable.FireCharges);
                    indicator.SetHealth(repairable.NormalizedHealth);
                    indicator.SetRepairable(repairable);
                }
                else
                {
                    indicator.Active = false;
                }
            }
        }

        /// <summary>
        /// Patched method to remove offscreen repair indicators. These are the ones the stick to the edge of the screen and move annoyingly.
        /// The indicators that appear on components are unaffected.
        /// </summary>
        /// <param name="repairables"></param>
        /// <param name="__instance"></param>
        /// <returns></returns>
        [HarmonyPatch(typeof(UIRepairComponentView), nameof(UIRepairComponentView.DrawIndicators))]
        [HarmonyPrefix]
        private static bool UIRepairComponentView_DrawIndicators(IList<Repairable> repairables, UIRepairComponentView __instance)
        {
            if (!_enabled || !_shouldBeEnabled) return true;
            // Original method but with offscreen logic removed.
            int i = 0;
            int j = 0;
            while (j < repairables.Count)
            {
                if (i >= __instance.indicatorCache.Length)
                {
                    MuseLog.Error("{0} >= {1}".F(
                    [
                        repairables.Count,
                        __instance.indicatorCache.Length
                    ]), null);
                    break;
                }
                Repairable repairable = repairables[j];
                RepairIndicator repairIndicator = __instance.indicatorCache[i];
                int tickCount = 1;
                float normalizedHealth = repairable.NormalizedHealth;
                __instance.SetIndicatorBarTickCount(repairIndicator, RepairComponentView.HEALTH_BAR_ONE, tickCount);
                __instance.SetIndicatorBarPercentage(repairIndicator, RepairComponentView.HEALTH_BAR_ONE, normalizedHealth);
                __instance.SetOrRotateIndicatorIcon(repairIndicator, repairable);
                Vector3 position = repairable.IndicatorPosition + __instance.GetDisplayOffset(repairable);
                Vector3 vector = CameraControl.Camera.WorldToScreenPoint(position) / UITransform.UIScale;
                float num = vector.x;
                float num2 = UITransform.ScreenRect.height - vector.y;
                if (vector.z < 0f)
                {
                    Vector3 vector2 = CameraControl.Transform.InverseTransformPoint(position) / UITransform.UIScale;
                    float x = Mathf.Sqrt(vector2.x * vector2.x + vector2.z * vector2.z);
                    float num3 = 57.29578f * Mathf.Atan2(vector2.x, vector2.z);
                    float num4 = -57.29578f * Mathf.Atan2(vector2.y, x);
                    num = UITransform.ScreenRect.width * (0.5f + num3 / CameraControl.HorizontalDegrees);
                    num2 = UITransform.ScreenRect.height * (0.5f + num4 / CameraControl.VerticalDegrees);
                }
                repairable.currentUIPosition.x = num;
                repairable.currentUIPosition.y = num2;
                repairable.UIVelocityX = 0f;
                repairable.UIVelocityY = 0f;
                __instance.indicatorCache[i].icon.SetPosition(repairable.currentUIPosition.x, repairable.currentUIPosition.y);
                if (__instance.indicatorCache[i].arrow.Activated)
                {
                    __instance.indicatorCache[i].arrow.Deactivate(0f);
                    __instance.indicatorCache[i].arrow.Fade = 0f;
                }
                j++;
                i++;
            }
            while (i < __instance.indicatorCache.Length)
            {
                if (__instance.indicatorCache[i].icon.Activated)
                {
                    __instance.indicatorCache[i].icon.Deactivate(0f);
                }
                i++;
            }
            return false;
        }

        private static GameObject CreateUi()
        {
            var parentObject = GameObject.Find("/Game UI/Match UI/UI HUD Canvas/UI HUD/UI Ship Health Display/Pilot Skill and Buff");

            var mainObject = new GameObject("RepairCluster");
            mainObject.transform.SetParent(parentObject.transform, false);
            var hlg = mainObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandHeight = false;
            hlg.childForceExpandWidth = false;

            var balloonArmorGunsGridObject = new GameObject("Balloon Armor Guns Grid");
            balloonArmorGunsGridObject.transform.SetParent(mainObject.transform);
            balloonArmorGunsGridObject.AddComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            var glg = balloonArmorGunsGridObject.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(_gridWidth, _gridHeight);
            glg.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            glg.constraintCount = 2;
            glg.startAxis = GridLayoutGroup.Axis.Vertical;
            glg.startCorner = GridLayoutGroup.Corner.UpperLeft;

            var enginesGridObject = Instantiate(balloonArmorGunsGridObject, mainObject.transform, false);
            enginesGridObject.name = "Engines Grid";

            CreateIndicator("Balloon", UIRepairComponentView.instance.zeppelinBig, balloonArmorGunsGridObject.transform);
            CreateIndicator("Armor", UIRepairComponentView.instance.armorBig, balloonArmorGunsGridObject.transform);
            for (var index = 0; index < _numberOfGuns; index++)
            {
                CreateIndicator($"Gun {index + 1}", UIRepairComponentView.instance.gunsBig, balloonArmorGunsGridObject.transform);
            }

            for (var index = 0; index < _numberOfEngines; index++)
            {
                CreateIndicator($"Engine {index + 1}", UIRepairComponentView.instance.enginesBig, enginesGridObject.transform);
            }

            mainObject.AddComponent<RepairCluster>();
            return mainObject;
        }

        private static GameObject CreateIndicator(string name, Texture2D componentTexture, Transform parentTransform)
        {
            LayoutElement le;
            RectTransform rt;
            VerticalLayoutGroup vlg;

            var indicatorObject = new GameObject($"Indicator {name}");
            indicatorObject.transform.SetParent(parentTransform, false);
            vlg = indicatorObject.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = false;

            var mainIconObject = new GameObject("Main Icon");
            mainIconObject.transform.SetParent(indicatorObject.transform, false);
            var mainIconRt = mainIconObject.AddComponent<RectTransform>();
            le = mainIconObject.AddComponent<LayoutElement>();
            le.preferredHeight = _gridWidth; // Intentional to make image 1:1.
            le.preferredWidth = _gridWidth;
            var mainIconRawImage = mainIconObject.AddComponent<RawImage>();
            mainIconRawImage.texture = componentTexture;
            mainIconRawImage.color = _colorWhite;
            // I would have made these status icons in a grid layout group, but I
            // don't feel like messing with that. Manual positioning works for now. -Pit
            var fireIconObject = new GameObject("Fire Icon");
            fireIconObject.transform.SetParent(indicatorObject.transform, false);
            rt = fireIconObject.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 4); // Manual positioning...
            rt.sizeDelta = new Vector2(16, 16);
            rt.pivot = new Vector2(0, 0);
            le = fireIconObject.AddComponent<LayoutElement>();
            le.ignoreLayout = true;
            var fireRawImage = fireIconObject.AddComponent<RawImage>();
            fireRawImage.texture = UIRepairComponentView.instance.fireNormal;
            fireRawImage.color = _colorOrangeDamaged;

            var failsafeIconObject = new GameObject("Failsafe Icon");
            failsafeIconObject.transform.SetParent(indicatorObject.transform, false);
            rt = failsafeIconObject.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(-16, 4); // Manual positioning...
            rt.sizeDelta = new Vector2(16, 16);
            rt.pivot = new Vector2(0, 0);
            le = failsafeIconObject.AddComponent<LayoutElement>();
            le.ignoreLayout = true;
            var failsafeIconRawImage = failsafeIconObject.AddComponent<RawImage>();
            failsafeIconRawImage.texture = _failsafeIconTexture;
            failsafeIconRawImage.color = _colorWhite;

            var fireproofIconObject = new GameObject("Fireproof Icon");
            fireproofIconObject.transform.SetParent(indicatorObject.transform, false);
            rt = fireproofIconObject.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(-16, 4 - 15); // Manual positioning...
            rt.sizeDelta = new Vector2(16, 16);
            rt.pivot = new Vector2(0, 0);
            le = fireproofIconObject.AddComponent<LayoutElement>();
            le.ignoreLayout = true;
            var fireproofIconRawImage = fireproofIconObject.AddComponent<RawImage>();
            fireproofIconRawImage.texture = _fireproofIconTexture;
            fireproofIconRawImage.color = _colorWhite;

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
            CreateProgressBar("Cooldown", 2, progressBarsObject.transform, out LayoutElement cooldownBarLe, out RawImage cooldownBarRawImage);
            CreateProgressBar("ActiveBuff", 2, progressBarsObject.transform, out LayoutElement activeBuffBarLe, out RawImage activeBuffBarRawImage);

            var indicator = new Indicator
            {
                Name = name,
                IndicatorObject = indicatorObject,
                RepairableTexture = componentTexture,
                IconRawImage = mainIconRawImage,
                FireIconObject = fireIconObject,
                FireIconRawImage = fireRawImage,
                FailsafeIconObject = failsafeIconObject,
                FailsafeIconRawImage = failsafeIconRawImage,
                FireproofIconObject = fireproofIconObject,
                FireproofIconRawImage = fireproofIconRawImage,
                HealthBarLayoutElement = healthBarLe,
                HealthBarRawImage = healthBarRawImage,
                CooldownBarLayoutElement = cooldownBarLe,
                CooldownBarRawImage = cooldownBarRawImage,
                ActiveBuffBarLayoutElement = activeBuffBarLe,
                ActiveBuffBarRawImage = activeBuffBarRawImage,
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
            public Texture2D RepairableTexture;
            public RawImage IconRawImage;
            public GameObject FireIconObject;
            public RawImage FireIconRawImage;
            public GameObject FailsafeIconObject;
            public RawImage FailsafeIconRawImage;
            public GameObject FireproofIconObject;
            public RawImage FireproofIconRawImage;
            public LayoutElement HealthBarLayoutElement;
            public RawImage HealthBarRawImage;
            public LayoutElement CooldownBarLayoutElement;
            public RawImage CooldownBarRawImage;
            public LayoutElement ActiveBuffBarLayoutElement;
            public RawImage ActiveBuffBarRawImage;

            public bool Active { get => IndicatorObject.activeSelf; set { if (IndicatorObject.activeSelf != value) IndicatorObject.SetActive(value); } }

            public void SetFireStacks(int stacks)
            {
                if (stacks == 0)
                {
                    if (FireIconObject.activeSelf) FireIconObject.SetActive(false);
                    return;
                }

                if (!FireIconObject.activeSelf) FireIconObject.SetActive(true);

                if (stacks >= _bigFireStacksThreshold) FireIconRawImage.texture = UIRepairComponentView.instance.fire;
                else FireIconRawImage.texture = UIRepairComponentView.instance.fireNormal;
            }

            public void SetHealth(float percentage)
            {
                // Set color.
                if (percentage == 0f)
                {
                    IconRawImage.color = _colorRedDestroyed;
                }
                else if (percentage <= .5f)
                {
                    IconRawImage.color = _colorWhiteTransparent;
                    HealthBarRawImage.color = _colorOrangeDamaged;
                }
                else
                {
                    IconRawImage.color = _colorWhiteTransparent;
                    HealthBarRawImage.color = _colorWhiteTransparent;
                }
                // Hide bar if health is full.
                if (percentage == 1f) percentage = 0;
                HealthBarLayoutElement.preferredWidth = CalculatePreferredWidth(percentage);
            }

            public void SetRepairable(Repairable repairable)
            {
                float newPercentage;
                float newPreferredWidth;

                // CooldownBar: Shows repair cooldown or rebuild progress if destroyed.
                // Only display if setting enabled. Hide bar if cooldown is done.
                if (!_showBarRepairProgress) newPercentage = 0f;
                else if (repairable.RepairProgress == 1f) newPercentage = 0f;
                else newPercentage = repairable.RepairProgress;
                newPreferredWidth = CalculatePreferredWidth(newPercentage);
                if (CooldownBarLayoutElement.preferredWidth != newPreferredWidth) CooldownBarLayoutElement.preferredWidth = newPreferredWidth;

                // ActiveBuffBar. Only display if setting enabled.
                if (!_showBarActiveBuff) newPercentage = 0f;
                // ActiveBuffBar: Armor Kit.
                else if (repairable.ActiveBuffType == Muse.Icarus.Common.SkillEffectType.AddBonusArmorBuff)
                {
                    if (!ActiveBuffBarRawImage.color.Equals(_colorCyan)) ActiveBuffBarRawImage.color = _colorCyan;
                    newPercentage = (float)repairable.BuffStackCount / _armorKitBuffValue;
                }
                // ActiveBuffBar: DynaBuff.
                else if (repairable.ActiveBuffType == Muse.Icarus.Common.SkillEffectType.BuffComponent)
                {
                    if (!ActiveBuffBarRawImage.color.Equals(_colorYellow)) ActiveBuffBarRawImage.color = _colorYellow;
                    newPercentage = repairable.BuffDuration;
                }
                // No ActiveBuff.
                else newPercentage = 0f;
                newPreferredWidth = CalculatePreferredWidth(newPercentage);
                if (ActiveBuffBarLayoutElement.preferredWidth != newPreferredWidth) ActiveBuffBarLayoutElement.preferredWidth = newPreferredWidth;

                // FailsafeIcon. Only display if setting enabled.
                // This doesn't only detect failsafe applied. Any effect that heals over time will display the icon.
                // if (repairable.Status.ActiveStatusEffects.Contains(Muse.Icarus.Common.SkillEffectType.HealPart))
                // Never mind, found this:
                if (_showStatusFailsafe && (bool)repairable.repairEffectControls?.spawnedAttachedEquipment)
                {
                    if (!FailsafeIconObject.activeSelf) FailsafeIconObject.SetActive(true);
                }
                else
                {
                    if (FailsafeIconObject.activeSelf) FailsafeIconObject.SetActive(false);
                }

                // ChemSprayIcon. Only display if setting enabled.
                if (_showStatusFireproof && repairable.Status.ActiveStatusEffects.Contains(Muse.Icarus.Common.SkillEffectType.ModifyIgnitionChance))
                {
                    if (!FireproofIconObject.activeSelf) FireproofIconObject.SetActive(true);
                }
                else
                {
                    if (FireproofIconObject.activeSelf) FireproofIconObject.SetActive(false);
                }
            }

            public static float CalculatePreferredWidth(float percentage)
            {
                return percentage * (_gridWidth - _progressBarPadding.horizontal);
            }

            public override string ToString()
            {
                return $"{Name}";
            }
        }
    }
}