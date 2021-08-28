using System.Collections.Generic;
using MuseBase;
using MuseBase.Steam.Wrapper;
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
        
        private struct ShipLabel
        {
            public string Name;
            public string Number;
            public Transform Transform;
        }

        public bool MinimapEnabled = true;
        public static MapController Instance;

        private Transform _container;
        private Transform _background;
        private RectTransform _rt;
        private List<ShipLabel> _labels = new List<ShipLabel>();
        private Vector2 _initialAnchorMin;
        private Vector2 _initialAnchorMax;
        private Vector2 _initialOffsetMin; 
        private Vector2 _initialOffsetMax; 
        

        public static void Initialize()
        {
            if (!(Instance is null))
                DestroyImmediate(Instance);
            
            Instance = UIMapSpawnDisplay.instance.gameObject.AddComponent<MapController>();

            Instance._container = Instance.transform.FindChild("Map Container");
            Instance._background = Instance.transform.FindChild("Background");
            Instance._labels.Clear();
            var labelsContainer = Instance.transform.FindChild("Map Container/Map Border/Map Display Mask/Labels");
            for (int i = 0; i < labelsContainer.childCount; i++)
            {
                var l = labelsContainer.GetChild(i);
                Instance._labels.Add(new ShipLabel
                {
                    Name = l.GetComponent<Text>().text,
                    Number = i.ToString(),
                    Transform = l
                });
            }
            Instance._rt = Instance._container.GetComponent<RectTransform>();
            Instance._initialAnchorMin = Instance._rt.anchorMin;
            Instance._initialAnchorMax = Instance._rt.anchorMax;
            Instance._initialOffsetMin = Instance._rt.offsetMin;
            Instance._initialOffsetMax = Instance._rt.offsetMax;
        }

        public void Full()
        {
            _state = State.Full;
            Instance._rt.anchorMin = _initialAnchorMin;
            Instance._rt.anchorMax = _initialAnchorMax;
            Instance._rt.offsetMin = _initialOffsetMin;
            Instance._rt.offsetMax = _initialOffsetMax;
            UIMapDisplay.Activate();
        }

        public void Minimap()
        {
            
            _state = State.Minimap;
            Instance._background.gameObject.SetActive(false);
            Instance._rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 10, 300);
            Instance._rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 10, 300);
        }

        public void Disabled()
        {
            _state = State.Disabled;
            UIMapDisplay.Deactivate();
        }
    }
}
