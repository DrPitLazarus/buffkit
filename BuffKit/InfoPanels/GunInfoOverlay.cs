using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Muse.Goi2.Entity;
using TMPro;
using static BuffKit.Util.Util;

namespace BuffKit.InfoPanels
{
    public static class GunInfoOverlay
    {
        private static BepInEx.Logging.ManualLogSource log;

        private static bool _useOverlay = true;                         // Test whether to use this overlay or regular one

        private static Dictionary<string, int> _infoNameToId;           // Convert from GunItemInfo.name to GunItem.id

        // Regular info displays
        private static TextMeshProUGUI _lName, _lDamageDirect, _lDamageAoE, _lRoF, _lReloadTime, _lClipSize, _lRange;
        private static TextMeshProUGUI _lProjectileSpeed, _lArmingTime, _lBuckshots, _lShellDrop, _lFireDirect, _lFireAoE, _lAoESize, _lSpecialEffect;
        // Arc info displays
        private static Image _arcLeftAngleImage;
        private static TextMeshProUGUI _arcLeftAngleLabel;
        private static Image _arcRightAngleImage;
        private static TextMeshProUGUI _arcRightAngleLabel;
        private static Image _arcUpAngleImage;
        private static TextMeshProUGUI _arcUpAngleLabel;
        private static Image _arcDownAngleImage;
        private static TextMeshProUGUI _arcDownAngleLabel;
        // Other components
        private static GameObject _obPanel;
        private static RectTransform _rectPanel;
        private static Vector2 _defaultRectPivot;

        public static bool DisplayGun(GunItemInfo gunInfo)
        {
            if (!_useOverlay) return true;
            if (_obPanel == null) return true;
            var gunId = _infoNameToId[gunInfo.name];
            var gunItem = CachedRepository.Instance.Get<GunItem>(gunId);

            _lName.text = gunInfo.name;
            _lDamageDirect.text = $"{gunInfo.directDamage} {GetDamageTypeName(gunInfo.directDamageType)}";
            BasicDisplayDecision(_lDamageAoE, $"{gunInfo.areaDamage} {GetDamageTypeName(gunInfo.areaDamageType)}", gunInfo.areaDamage, 0);
            _lRoF.text = $"{String.Format("{0:0.###}", gunInfo.rateOfFire)} rounds/s";
            _lReloadTime.text = $"{1f / gunInfo.reloadSpeed}s";
            _lClipSize.text = $"{gunInfo.magazineSize}";
            _lRange.text = $"{gunInfo.range}m ({gunInfo.RangeString})";

            BasicDisplayDecision(_lProjectileSpeed, $"{gunInfo.projectileSpeed}m/s", gunInfo.projectileSpeed, 0);
            var arming = gunItem.GetFloatParam("fArmingDelay");
            BasicDisplayDecision(_lArmingTime, $"{String.Format("{0:0.###}", arming)}s", arming, 0);
            BasicDisplayDecision(_lBuckshots, $"{gunInfo.buckshots}", gunInfo.buckshots, 1);
            BasicDisplayDecision(_lShellDrop, $"{gunInfo.shellDrop}m/s²", gunInfo.shellDrop, 0);
            BasicDisplayDecision(_lFireDirect, $"{gunInfo.directFireChance * 100f}% chance of {gunInfo.directFireStacks} stack{(gunInfo.directFireStacks != 1 ? "s" : "")}", gunInfo.directFireChance * gunInfo.directFireStacks, 0);
            BasicDisplayDecision(_lFireAoE, $"{gunInfo.areaFireChance * 100f}% chance of {gunInfo.areaFireStacks} stack{(gunInfo.areaFireStacks != 1 ? "s" : "")}", gunInfo.areaFireChance * gunInfo.areaFireStacks, 0);
            if (gunInfo.aoeRangeMax != gunInfo.aoeRangeMin)
                _lAoESize.text = $"{gunInfo.aoeRangeMin}m to {gunInfo.aoeRangeMax}m";
            else
                _lAoESize.text = $"{gunInfo.aoeRangeMin}m";

            string special = "";
            string strengthName = null;
            float strengthValue = 0;
            string strengthUnit = null;
            string durationName = null;
            float durationValue = 0;
            string durationUnit = null;
            if (gunInfo.TryGetSpecialEfx(ref strengthName, ref strengthValue, ref strengthUnit, ref durationName, ref durationValue, ref durationUnit))
            {
                bool flag1 = !string.IsNullOrEmpty(strengthName);
                bool flag2 = !string.IsNullOrEmpty(durationName);
                if (flag1)
                {
                    special += $"{strengthName}: {strengthValue}{strengthUnit}";
                    if (flag2)
                        special += "\n";
                }
                if (flag2)
                    special += $"{durationName}: {durationValue}{durationUnit}";
            }
            BasicDisplayDecision(_lSpecialEffect, special, special, "");

            _arcLeftAngleImage.fillAmount = gunInfo.leftAngle / 90f;
            _arcRightAngleImage.fillAmount = gunInfo.rightAngle / 90f;
            _arcUpAngleImage.fillAmount = gunInfo.upAngle / 90f;
            _arcDownAngleImage.fillAmount = gunInfo.downAngle / 90f;
            _arcLeftAngleLabel.text = $"{gunInfo.leftAngle}°";
            _arcRightAngleLabel.text = $"{gunInfo.rightAngle}°";
            _arcUpAngleLabel.text = $"{gunInfo.upAngle}°";
            _arcDownAngleLabel.text = $"{gunInfo.downAngle}°";

            return false;
        }

        private static void BasicDisplayDecision<T>(TextMeshProUGUI label, string display, T data, T dataTest)
            where T : IEquatable<T>
        {
            if (!data.Equals(dataTest))
            {
                label.text = display;
                label.transform.parent.parent.gameObject.SetActive(true);
            }
            else
                label.transform.parent.parent.gameObject.SetActive(false);
        }

        public static bool ShowAtScreenPosition(Vector3 position, Vector2? pivot)
        {
            if (!_useOverlay) return true;
            if (_obPanel == null) return true;
            _obPanel.SetActive(true);
            _rectPanel.pivot = ((pivot == null) ? _defaultRectPivot : pivot.Value);
            _rectPanel.position = position;
            return false;
        }
        public static bool Hide()
        {
            if (!_useOverlay) return true;
            if (_obPanel == null) return true;
            _obPanel.SetActive(false);
            return false;
        }

        private static void CreatePanel()
        {
            var parent = GameObject.Find("/Menu UI/Standard Canvas/Common Elements/Info Overlay Window (don't hide)")?.transform;
            _obPanel = UI.Builder.BuildPanel(parent);
            _obPanel.name = "Custom Gun Tooltip";
            _rectPanel = _obPanel.GetComponent<RectTransform>();
            _defaultRectPivot = _rectPanel.pivot;
            var csf = _obPanel.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var vlg = _obPanel.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 3;
            vlg.padding = new RectOffset(6, 6, 5, 5);

            // Title
            var obName = UI.Builder.BuildLabel(_obPanel.transform, out _lName, UI.Resources.FontGaldeanoRegular, TextAnchor.MiddleLeft, 18);
            obName.name = "name label";
            var le = obName.AddComponent<LayoutElement>();
            le.minWidth = 250;

            // Content
            var obContent = new GameObject("content");
            obContent.transform.SetParent(_obPanel.transform, false);
            vlg = obContent.AddComponent<VerticalLayoutGroup>();
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 2;

            // Regular rows
            BuildRow(obContent.transform, "Primary", out _lDamageDirect);
            BuildRow(obContent.transform, "Secondary", out _lDamageAoE);
            BuildRow(obContent.transform, "RoF", out _lRoF);
            BuildRow(obContent.transform, "Reload time", out _lReloadTime);
            BuildRow(obContent.transform, "Clip size", out _lClipSize);
            BuildRow(obContent.transform, "Range", out _lRange);
            BuildRow(obContent.transform, "Projectile speed", out _lProjectileSpeed);
            BuildRow(obContent.transform, "Arming time", out _lArmingTime);
            BuildRow(obContent.transform, "Buckshots", out _lBuckshots);
            BuildRow(obContent.transform, "Shell drop", out _lShellDrop);
            BuildRow(obContent.transform, "Primary fire chance", out _lFireDirect);
            BuildRow(obContent.transform, "Secondary fire chance", out _lFireAoE);
            BuildRow(obContent.transform, "AoE", out _lAoESize);
            BuildRow(obContent.transform, "Special effect", out _lSpecialEffect);
            // Arc display row
            var sampleArcDisplay = GameObject.Find("/Menu UI/Standard Canvas/Pages/Library/Gun Selection Panel/Gun Information Panel/Gun Arc Group");
            var obArcDisplay = GameObject.Instantiate(sampleArcDisplay, obContent.transform);
            if (obArcDisplay != null)
            {
                // Get arc components, replacing Text components with TextMeshProUGUI
                _arcLeftAngleImage = obArcDisplay.transform.GetChild(0).GetChild(0).GetComponent<Image>();
                var obLabel = obArcDisplay.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
                GameObject.DestroyImmediate(obLabel.GetComponent<UnityEngine.UI.Text>());
                _arcLeftAngleLabel = obLabel.AddComponent<TextMeshProUGUI>();
                _arcLeftAngleLabel.alignment = TextAlignmentOptions.Center;

                _arcRightAngleImage = obArcDisplay.transform.GetChild(0).GetChild(1).GetComponent<Image>();
                obLabel = obArcDisplay.transform.GetChild(0).GetChild(1).GetChild(0).gameObject;
                GameObject.DestroyImmediate(obLabel.GetComponent<UnityEngine.UI.Text>());
                _arcRightAngleLabel = obLabel.AddComponent<TextMeshProUGUI>();

                _arcUpAngleImage = obArcDisplay.transform.GetChild(2).GetChild(0).GetComponent<Image>();
                obLabel = obArcDisplay.transform.GetChild(2).GetChild(0).GetChild(0).gameObject;
                GameObject.DestroyImmediate(obLabel.GetComponent<UnityEngine.UI.Text>());
                _arcUpAngleLabel = obLabel.AddComponent<TextMeshProUGUI>();

                _arcDownAngleImage = obArcDisplay.transform.GetChild(2).GetChild(1).GetComponent<Image>();
                obLabel = obArcDisplay.transform.GetChild(2).GetChild(1).GetChild(0).gameObject;
                GameObject.DestroyImmediate(obLabel.GetComponent<UnityEngine.UI.Text>());
                _arcDownAngleLabel = obLabel.AddComponent<TextMeshProUGUI>();

                // Remake row display with HLG
                GameObject.Destroy(obArcDisplay.GetComponent<LayoutElement>());
                var hlg = obArcDisplay.GetComponent<HorizontalLayoutGroup>();
                hlg.enabled = true;
                hlg.spacing = 0;
                hlg.padding = new RectOffset(0, 0, 10, 0);

                // Resize arc displays
                le = obArcDisplay.transform.GetChild(0).gameObject.GetComponent<LayoutElement>();
                le.minHeight = 100;
                le.minWidth = 120;
                le = obArcDisplay.transform.GetChild(2).gameObject.GetComponent<LayoutElement>();
                le.minHeight = 100;
                le.minWidth = 120;

                // Recreate spacer
                var obSpacer = obArcDisplay.transform.GetChild(1).gameObject;
                GameObject.Destroy(obSpacer.GetComponent<Image>());
                GameObject.Destroy(obSpacer.GetComponent<CanvasRenderer>());
                le = obSpacer.GetComponent<LayoutElement>();
                le.preferredHeight = -1;
                le.preferredWidth = -1;
                le.flexibleWidth = 1;

                GameObject.Destroy(obArcDisplay.transform.GetChild(0).GetChild(3).gameObject);      // Destroy "Horizontal" label object
                GameObject.Destroy(obArcDisplay.transform.GetChild(2).GetChild(3).gameObject);      // Destroy "Vertical" label object

                // Set up TextMeshProUGUI components with wanted parameters
                _arcLeftAngleLabel.font = UI.Resources.FontGaldeanoRegular;
                _arcLeftAngleLabel.fontSize = 13;
                _arcLeftAngleLabel.enableWordWrapping = false;
                _arcLeftAngleLabel.alignment = TextAlignmentOptions.BottomGeoAligned;
                _arcRightAngleLabel.font = UI.Resources.FontGaldeanoRegular;
                _arcRightAngleLabel.fontSize = 13;
                _arcRightAngleLabel.enableWordWrapping = false;
                _arcRightAngleLabel.alignment = TextAlignmentOptions.BottomGeoAligned;
                _arcUpAngleLabel.font = UI.Resources.FontGaldeanoRegular;
                _arcUpAngleLabel.fontSize = 13;
                _arcUpAngleLabel.enableWordWrapping = false;
                _arcUpAngleLabel.alignment = TextAlignmentOptions.Left;
                _arcDownAngleLabel.font = UI.Resources.FontGaldeanoRegular;
                _arcDownAngleLabel.fontSize = 13;
                _arcDownAngleLabel.enableWordWrapping = false;
                _arcDownAngleLabel.alignment = TextAlignmentOptions.Left;
            }
            else log.LogInfo("Failed to find sample arc display");

            _obPanel.SetActive(false);
        }

        private static void BuildRow(Transform parent, string name, out TextMeshProUGUI label)
        {
            var row = new GameObject($"row {name}");
            row.transform.SetParent(parent, false);
            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleRight;

            TextMeshProUGUI tmp;
            UI.Builder.BuildLabel(row.transform, out tmp, UI.Resources.FontGaldeanoRegular, TextAnchor.MiddleLeft, 13).name = "label text";
            tmp.text = name;

            var spacer = new GameObject("spacer");
            spacer.transform.SetParent(row.transform, false);
            var le = spacer.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;
            le.minWidth = 10;

            UI.Builder.BuildLabel(row.transform, out label, UI.Resources.FontGaldeanoRegular, TextAnchor.MiddleLeft, 13).name = "label info";
        }

        public static void Initialize()
        {
            Settings.Settings.Instance.AddEntry("info panels", "detailed gun info", delegate (bool v)
            {
                if (!v) Hide();
                _useOverlay = v;
            }, _useOverlay);

            log = BepInEx.Logging.Logger.CreateLogSource("guninfo");

            _infoNameToId = new Dictionary<string, int>();
            foreach (var id in GunIds)
            {
                var gunItem = CachedRepository.Instance.Get<GunItem>(id);
                var gunInfo = GunItemInfo.FromGunItem(gunItem);
                var gunName = gunInfo.name;
                if (!string.IsNullOrEmpty(gunName))
                    _infoNameToId.Add(gunName, id);
            }
            var obInfoOverlayWindow = GameObject.Find("/Menu UI/Standard Canvas/Common Elements/Info Overlay Window (don't hide)");

            CreatePanel();
            _obPanel.GetComponentsInChildren<Graphic>().ForEach(delegate (Graphic g)
            {
                g.raycastTarget = false;
            });
        }
    }
}
