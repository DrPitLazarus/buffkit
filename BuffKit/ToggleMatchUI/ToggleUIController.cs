using UnityEngine;
using System.Collections.Generic;

namespace BuffKit.ToggleMatchUI
{
    public class ToggleUIController : MonoBehaviour
    {
        public static ToggleUIController Instance;

        public static bool Initialized { get; private set; }

        private static KeyBinding _kb;

        public bool ShowUI { get; private set; }
        private List<GameObject> _objectsToHide = new List<GameObject>();

        private void Awake()
        {
            Instance = this;
            _kb = new KeyBinding(KeyCode.F6);
            // UI object tree
            _objectsToHide.Add(UIDisplayManager.instance.gameObject.transform.parent.gameObject);
            // Match chat panel
            _objectsToHide.Add(GameObject.Find("Menu UI/Standard Canvas/Menu Header Footer/Match Chat Panel"));
            // First-person equipment object
            _objectsToHide.Add(GameObject.Find("GameCameraRig/CutScene/MainCamera/FoVOffset/PlayerEquipment"));
            ShowUI = true;
            Initialized = true;
        }
        private void OnDisable()
        {
            Initialized = false;
        }
        public void LateUpdate()
        {
            if (_kb.GetDown())
            {
                ShowUI = !ShowUI;
                MuseLog.Info("Show UI change to: " + ShowUI);
                foreach (var ob in _objectsToHide)
                    ob.SetActive(ShowUI);
                if (ShowUI)
                    UIRepairComponentView.Activate();
            }
            // Some things get re-activated elsewhere (e.g. change states, close pause menu, etc.) so disable every update
            if (!ShowUI)
            {
                UIRepairComponentView.Deactivate();
                foreach (var ob in _objectsToHide)
                    ob.SetActive(false);
            }
        }
    }
}