using BuffKit.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BuffKit.Speedometer
{
    public class Speedometer : MonoBehaviour
    {
        private static readonly string _name = "Speedometer";

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
            Destroy(_mainObject);
        }

        public static void SetActive(bool active)
        {
            _mainObject.SetActive(active);
        }

        private void Update()
        {
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
            var fontSizeSmaller = 10;

            var mainObject = new GameObject(_name);
            mainObject.transform.SetParent(parent, false);
            var rt = mainObject.AddComponent<RectTransform>();
            rt.anchorMax = new Vector2(0.5f, 0.1f);
            rt.anchorMin = new Vector2(0.5f, 0.1f);
            rt.pivot = new Vector2(0.5f, 0);
            var glg = mainObject.AddComponent<GridLayoutGroup>();
            glg.startAxis = GridLayoutGroup.Axis.Vertical;
            glg.startCorner = GridLayoutGroup.Corner.UpperLeft;
            glg.cellSize = new Vector2(75, 15);
            glg.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            glg.constraintCount = 3;
            glg.spacing = new Vector2(4, 0);
            glg.childAlignment = TextAnchor.UpperCenter;

            BuildNumberLabel(mainObject.transform, out _speedometerText);
            BuildNumberLabel(mainObject.transform, out _altitudeSpeedText);
            BuildNumberLabel(mainObject.transform, out _rotationSpeedText);
            Builder.BuildLabel(mainObject.transform, "m/s horizontal", TextAnchor.MiddleLeft, fontSizeSmaller);
            Builder.BuildLabel(mainObject.transform, "m/s vertical", TextAnchor.MiddleLeft, fontSizeSmaller);
            Builder.BuildLabel(mainObject.transform, "deg/s rotation", TextAnchor.MiddleLeft, fontSizeSmaller);

            BuildNumberLabel(mainObject.transform, out _positionXText);
            BuildNumberLabel(mainObject.transform, out _positionYText);
            BuildNumberLabel(mainObject.transform, out _positionZText);
            Builder.BuildLabel(mainObject.transform, "X east/west", TextAnchor.MiddleLeft, fontSizeSmaller);
            Builder.BuildLabel(mainObject.transform, "Y altitude", TextAnchor.MiddleLeft, fontSizeSmaller);
            Builder.BuildLabel(mainObject.transform, "Z north/south", TextAnchor.MiddleLeft, fontSizeSmaller);

            mainObject.AddComponent<Speedometer>();
            return mainObject;
        }

        private static GameObject BuildNumberLabel(Transform transform, out TextMeshProUGUI textMeshProUGUI, TextAnchor position = TextAnchor.MiddleRight, int fontSize = 14)
        {
            var newObject = Builder.BuildLabel(transform, "0", position, fontSize);
            textMeshProUGUI = newObject.GetComponentInChildren<TextMeshProUGUI>();
            textMeshProUGUI.alignment = TextAlignmentOptions.TopRight;
            return newObject;
        }
    }
}