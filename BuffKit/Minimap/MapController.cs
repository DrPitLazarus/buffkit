using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        public static MapController Instance { get; private set; }

        private Transform _container;
        private Transform _background;
        private Transform _grid;
        private Transform _labels;
        private RectTransform _rt;
        private CanvasScaler _scaler;
        private List<Transform> _gridLabels = new List<Transform>();
        private List<RectTransform> _verticalGridLines = new List<RectTransform>();
        private List<RectTransform> _horizontalGridLines = new List<RectTransform>();
        private Vector2 _initialAnchorMin;
        private Vector2 _initialAnchorMax;
        private Vector2 _initialOffsetMin;
        private Vector2 _initialOffsetMax;

        private int _size = 300;
        private int _offset = 10;
        private float _labelScale = 1.0f;
        private bool _showLabels = true;
        private bool _showGrid = true;

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

            Instance._container = Instance.transform.FindChild("Map Container");
            Instance._background = Instance.transform.FindChild("Background");
            Instance._grid = Instance.transform.FindChild("Map Container/Map Border/Map Display Mask/Lines");
            Instance._labels = Instance.transform.FindChild("Map Container/Map Border/Map Display Mask/Labels");
            Instance._scaler = Instance.transform.parent.GetComponent<CanvasScaler>();
            Instance._rt = Instance._container.GetComponent<RectTransform>();

            for (int i = 0; i < Instance._grid.childCount; i++)
            {
                var c = Instance._grid.GetChild(i);
                if (c.name.Contains("Text"))
                    Instance._gridLabels.Add(c);
                else if (c.name.Contains("Vertical"))
                    Instance._verticalGridLines.Add(c.GetComponent<RectTransform>());
                else if (c.name.Contains("Horizontal"))
                    Instance._horizontalGridLines.Add(c.GetComponent<RectTransform>());
            }
            
            Instance._initialAnchorMin = Instance._rt.anchorMin;
            Instance._initialAnchorMax = Instance._rt.anchorMax;
            Instance._initialOffsetMin = Instance._rt.offsetMin;
            Instance._initialOffsetMax = Instance._rt.offsetMax;

            _initialized = true;
        }

        private void SetGridLabelScale(float scale)
        {
            foreach (var label in _gridLabels) 
                label.localScale = new Vector3(scale, scale);
        }
        
        private void SetGridLinesToOnePixel()
        {
            float scale = _scaler.referenceResolution.x / Screen.width; 

            var horizontalScale = new Vector3(1f, scale);
            var verticalScale = new Vector3(scale, 1f);
            
            foreach (var line in _verticalGridLines) 
                line.localScale = verticalScale;

            foreach (var line in _horizontalGridLines) 
                line.localScale = horizontalScale;
        }

        private void SetGridLinesToNormal()
        {
            foreach (var line in _verticalGridLines)
            {
                line.localScale = Vector3.one;
            }
            
            foreach (var line in _horizontalGridLines)
            {
                line.localScale = Vector3.one;
            }

        }

        public void Full()
        {
            if (_state != State.Full)
            {
                _state = State.Full;
                _background.gameObject.SetActive(true);
                _labels.gameObject.SetActive(true);
                _grid.gameObject.SetActive(true);
                
                _rt.anchorMin = _initialAnchorMin;
                _rt.anchorMax = _initialAnchorMax;
                _rt.offsetMin = _initialOffsetMin;
                _rt.offsetMax = _initialOffsetMax;
                SetGridLabelScale(1f);
                SetGridLinesToNormal();
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
                _labels.gameObject.SetActive(false);
                _grid.gameObject.SetActive(_showGrid);
                SetGridLabelScale(0.7f);
                _settingsChanged = false;
            }
            UIMapDisplay.Activate();
            _background.gameObject.SetActive(false);
            SetGridLinesToOnePixel();
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
