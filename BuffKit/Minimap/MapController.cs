using UnityEngine;

namespace BuffKit.Minimap
{
    public enum State
    {
        Disabled,
        Minimap,
        Full
    }

    public class MapController : MonoBehaviour
    {
        private State _state;
        public State State
        {
            get => _state;
            set => _state = value;
        }

        public bool MinimapEnabled = true;
        public static MapController Instance;

        private static Transform _container;
        private static Transform _background;
        private static Transform _grid;
        private static Transform _labels;
        private static RectTransform _rt;
        // private static List<ShipLabel> _labels = new List<ShipLabel>();
        private static Vector2 _initialAnchorMin;
        private static Vector2 _initialAnchorMax;
        private static Vector2 _initialOffsetMin;
        private static Vector2 _initialOffsetMax;

        private static int _size = 300;
        private static int _offset = 10;
        private static float _labelScale = 1.0f;
        private static bool _showLabels = true;
        private static bool _showGrid = true;

        private static bool _settingsChanged = true;
        private static bool _initialized = false;
        private static KeyBinding _kb;

        public void Start()
        {
            _kb = new KeyBinding(KeyCode.F5);
        }
        
        public void Update()
        {
            if (_kb.GetDown())
            {
                MinimapEnabled = !MinimapEnabled;
            }
        }        
        public static void Initialize()
        {
            if (_initialized) return;

            Instance = UIMapSpawnDisplay.instance.gameObject.AddComponent<MapController>();

            _container = Instance.transform.FindChild("Map Container");
            _background = Instance.transform.FindChild("Background");
            _grid = Instance.transform.FindChild("Map Container/Map Border/Map Display Mask/Lines");
            _labels = Instance.transform.FindChild("Map Container/Map Border/Map Display Mask/Labels");

            _rt = _container.GetComponent<RectTransform>();
            _initialAnchorMin = _rt.anchorMin;
            _initialAnchorMax = _rt.anchorMax;
            _initialOffsetMin = _rt.offsetMin;
            _initialOffsetMax = _rt.offsetMax;
            _initialized = true;
        }

        private void UpdateMiniSettings()
        {
            _labels.gameObject.SetActive(_showLabels);
            _grid.gameObject.SetActive(_showGrid);
            for (int i = 0; i < _labels.childCount; i++)
            {
                var l = _labels.GetChild(i);
                l.GetComponent<RectTransform>().localScale = new Vector3(_labelScale, _labelScale);
            }
            _settingsChanged = false;
        }

        private void SetFullSettings()
        {
            _labels.gameObject.SetActive(true);
            _grid.gameObject.SetActive(true);
            for (int i = 0; i < _labels.childCount; i++)
            {
                var l = _labels.GetChild(i);
                l.GetComponent<RectTransform>().localScale = Vector3.one;
            }
        }

        public void Full()
        {
            if (_state != State.Full)
            {
                _state = State.Full;
                _background.gameObject.SetActive(true);
                _rt.anchorMin = _initialAnchorMin;
                _rt.anchorMax = _initialAnchorMax;
                _rt.offsetMin = _initialOffsetMin;
                _rt.offsetMax = _initialOffsetMax;
                SetFullSettings();
            }

            UIMapDisplay.Activate();
        }

        public void Minimap()
        {
            if (_state != State.Minimap || _settingsChanged)
            {
                _state = State.Minimap;
                _rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, _offset, _size);
                _rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, _offset, _size);
                UpdateMiniSettings();
            }
            UIMapDisplay.Activate();
            _background.gameObject.SetActive(false);
        }

        public void Disabled()
        {
            if (_state != State.Disabled)
            {
                _background.gameObject.SetActive(false);
                _state = State.Disabled;
            }
            UIMapDisplay.Deactivate();
        }
    }
}
