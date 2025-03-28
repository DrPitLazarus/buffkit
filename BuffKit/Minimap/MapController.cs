﻿using System.Collections.Generic;
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
        public static bool Initialized = false;
        public static MapController Instance;

        private Transform _container;
        private Transform _background;
        private Transform _grid;
        private Transform _labels;
        private RectTransform _rt;
        private CanvasScaler _scaler;
        private List<Transform> _gridLabels = new List<Transform>();
        private List<RectTransform> _verticalGridLines = new List<RectTransform>();
        private List<RectTransform> _horizontalGridLines = new List<RectTransform>();
        private List<RectTransform> _markers = new List<RectTransform>();
        private Vector2 _initialAnchorMin;
        private Vector2 _initialAnchorMax;
        private Vector2 _initialOffsetMin;
        private Vector2 _initialOffsetMax;

        private int _size = 300;
        private int _offset = 10;
        private bool _showGrid = true;

        private static KeyBinding _kb;
        
        public void Update()
        {
            if (_kb.GetDown())
            {
                if (MinimapEnabled)
                {
                    UIMapDisplay.Deactivate();
                    Full();
                    _background.gameObject.SetActive(false);
                }

                MinimapEnabled = !MinimapEnabled;
            }
        }
        
        public void Awake()
        {
            MuseLog.Info("Creating the minimap controller");
            Instance = this;
            _kb = new KeyBinding(KeyCode.F5);

            _container = UIMapSpawnDisplay.instance.transform.FindChild("Map Container");
            _background = UIMapSpawnDisplay.instance.transform.FindChild("Background");
            _grid = UIMapSpawnDisplay.instance.transform.FindChild("Map Container/Map Border/Map Display Mask/Lines");
            _labels = UIMapSpawnDisplay.instance.transform.FindChild("Map Container/Map Border/Map Display Mask/Labels");
            _scaler = UIMapSpawnDisplay.instance.transform.parent.GetComponent<CanvasScaler>();
            _rt = _container.GetComponent<RectTransform>();
            
            for (int i = 0; i < _grid.childCount; i++)
            {
                var c = _grid.GetChild(i);
                if (c.name.Contains("Text"))
                    _gridLabels.Add(c);
                else if (c.name.Contains("Vertical"))
                    _verticalGridLines.Add(c.GetComponent<RectTransform>());
                else if (c.name.Contains("Horizontal"))
                    _horizontalGridLines.Add(c.GetComponent<RectTransform>());
            }
            
            var markersContainer = UIMapSpawnDisplay.instance.transform.FindChild("Map Container/Map Border/Map Display Mask/Markers");
            for (int i = 0; i < markersContainer.childCount; i++)
                _markers.Add(markersContainer.GetChild(i).GetComponent<RectTransform>());
            
            _initialAnchorMin = _rt.anchorMin;
            _initialAnchorMax = _rt.anchorMax;
            _initialOffsetMin = _rt.offsetMin;
            _initialOffsetMax = _rt.offsetMax;

            Initialized = true;
        }

        public void OnDisable()
        {
            MuseLog.Info("Destroying the minimap controller");
            _background.gameObject.SetActive(false);
            _labels.gameObject.SetActive(true);
            _grid.gameObject.SetActive(true);
                
            _rt.anchorMin = _initialAnchorMin;
            _rt.anchorMax = _initialAnchorMax;
            _rt.offsetMin = _initialOffsetMin;
            _rt.offsetMax = _initialOffsetMax;
            SetGridLabelScale(1f);
            SetGridLinesToNormal();
            SetMarkerSize(30);
            SetSpectatorMarkerStatus(true);
            
            Initialized = false;
            Instance = null;
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

        private void SetMarkerSize(int size)
        {
            var newSize = new Vector2(size, size);
            foreach (var marker in _markers)
                marker.sizeDelta = newSize;
        }

        private void SetSpectatorMarkerStatus(bool status)
        {
            _markers[_markers.Count - 1].gameObject.SetActive(status);
        }

        public void Full()
        {
            if (_state != State.Full)
            {
                MuseLog.Info("Minimap set to full");
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
                SetMarkerSize(30);
                SetSpectatorMarkerStatus(true);
            }
        }

        public void Minimap()
        {
            if (_state != State.Minimap)
            {
                MuseLog.Info("Minimap set to minimap");
                _state = State.Minimap;
                _rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, _offset, _size);
                _rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, _offset, _size);
                _labels.gameObject.SetActive(false);
                _grid.gameObject.SetActive(_showGrid);
                SetGridLabelScale(0.7f);
                SetMarkerSize(24);
                SetSpectatorMarkerStatus(false);
                SetGridLinesToOnePixel();
            }
            UIMapDisplay.Activate();
            _background.gameObject.SetActive(false);
        }

        public void Disabled()
        {
            if (_state != State.Disabled)
            {
                MuseLog.Info("Minimap set to disabled");
                _background.gameObject.SetActive(false);
                _state = State.Disabled;
            }
            UIMapDisplay.Deactivate();
        }
    }
}
