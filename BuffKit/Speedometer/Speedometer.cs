using System.Collections.Generic;
using BuffKit.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BuffKit.Speedometer
{
    public class Speedometer : MonoBehaviour
    {
        private static readonly string _name = "Speedometer";
        private static readonly Color _transparentWhite = new(1, 1, 1, 0.6f);

        private static readonly List<GameObject> _meterObjects = [];
        private static GameObject _mainObject;
        private static TextMeshProUGUI _speedometerText;
        private static TextMeshProUGUI _altitudeSpeedText;
        private static TextMeshProUGUI _rotationSpeedText;
        private static TextMeshProUGUI _positionXText;
        private static TextMeshProUGUI _positionYText;
        private static TextMeshProUGUI _positionZText;

        public static void Initialize()
        {
            if (_mainObject != null) return;

            var parentObjectPath = "/Game UI/Match UI/UI HUD Canvas/UI HUD/";
            var parentObject = GameObject.Find(parentObjectPath);

            _mainObject = BuildUi(parentObject.transform);
            MuseLog.Info("Initialized!");
        }

        public static void Destroy()
        {
            if (_mainObject == null) return;
            Destroy(_mainObject);
        }

        public static void SetActive(bool active)
        {
            if (_mainObject == null) return;
            _mainObject.SetActive(active);
        }

        public static void UpdateVisibility()
        {
            if (_mainObject == null) return;

            var currentClass = -1;
            switch (NetworkedPlayer.Local.PlayerClass)
            {
                case Muse.Goi2.Entity.AvatarClass.Pilot:
                    currentClass = 0; break;
                case Muse.Goi2.Entity.AvatarClass.Gunner:
                    currentClass = 1; break;
                case Muse.Goi2.Entity.AvatarClass.Engineer:
                    currentClass = 2; break;
            }
            if (currentClass == -1) return;

            var settings = SpeedometerPatcher.DisplaySettings;

            for (var index = 0; index < settings.Rows; index++)
            {
                var setting = settings.Values[index, currentClass];
                _meterObjects[index].SetActive(setting);
            }
        }

        private void Update()
        {
            if (!SpeedometerPatcher.Enabled) SetActive(false);
            if (NetworkedPlayer.Local.IsSpectator) SetActive(false);

            var currentShip = NetworkedPlayer.Local.CurrentShip;
            if (currentShip == null) return;

            var baseText = "<mspace=10px>{0:0.0}";
            _speedometerText.text = baseText.F([currentShip.LocalVelocity.z]);
            _altitudeSpeedText.text = baseText.F([currentShip.LocalVelocity.y]);
            _rotationSpeedText.text = baseText.F([currentShip.AngularVelocity * Mathf.Rad2Deg]);

            var baseTextNoDecimal = "<mspace=10px>{0:0}";
            _positionXText.text = baseTextNoDecimal.F([currentShip.Position.x]);
            _positionYText.text = baseTextNoDecimal.F([currentShip.Position.y]);
            _positionZText.text = baseTextNoDecimal.F([currentShip.Position.z]);
        }

        private static GameObject BuildUi(Transform parent)
        {
            var mainObject = new GameObject(_name);
            mainObject.transform.SetParent(parent, false);
            var rt = mainObject.AddComponent<RectTransform>();
            rt.anchorMax = new Vector2(0.5f, 0.1f);
            rt.anchorMin = new Vector2(0.5f, 0.1f);
            rt.pivot = new Vector2(0.5f, 0);
            var glg = mainObject.AddComponent<GridLayoutGroup>();
            glg.startAxis = GridLayoutGroup.Axis.Vertical;
            glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
            glg.cellSize = new Vector2(150, 15);
            glg.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            glg.constraintCount = 3;
            glg.spacing = new Vector2(4, 0);
            glg.childAlignment = TextAnchor.UpperCenter;

            BuildMeterItem(mainObject.transform, "Horizontal Speed", out _speedometerText, "m/s horizontal");
            BuildMeterItem(mainObject.transform, "Vertical Speed", out _altitudeSpeedText, "m/s vertical");
            BuildMeterItem(mainObject.transform, "Rotation Speed", out _rotationSpeedText, "deg/s rotation");

            BuildMeterItem(mainObject.transform, "X Position EastWest", out _positionXText, "X east/west");
            BuildMeterItem(mainObject.transform, "Y Position Altitude", out _positionYText, "Y altitude");
            BuildMeterItem(mainObject.transform, "Z Position NorthSouth", out _positionZText, "Z north/south");

            mainObject.AddComponent<Speedometer>();
            return mainObject;
        }

        private static GameObject BuildMeterItem(Transform transform, string objectName, out TextMeshProUGUI numberTextMesh, string labelText, TextAnchor position = TextAnchor.MiddleLeft, int fontSize = 14)
        {
            var containerObject = new GameObject(objectName);
            containerObject.transform.SetParent(transform, false);
            var hlg = containerObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth = false;
            hlg.childAlignment = position;
            hlg.spacing = 4;

            var numberObject = Builder.BuildLabel(containerObject.transform, "0", position, fontSize);
            numberObject.GetComponentInChildren<LayoutElement>().minWidth = 50;
            numberTextMesh = numberObject.GetComponentInChildren<TextMeshProUGUI>();
            numberTextMesh.alignment = TextAlignmentOptions.TopRight;

            var labelObject = Builder.BuildLabel(containerObject.transform, labelText, position, 10);
            labelObject.GetComponentInChildren<TextMeshProUGUI>().color = _transparentWhite;

            _meterObjects.Add(containerObject);
            return containerObject;
        }
    }
}