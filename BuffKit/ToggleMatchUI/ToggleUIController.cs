using System.Collections.Generic;
using UnityEngine;

namespace BuffKit.ToggleMatchUI
{
    public class ToggleUIController : MonoBehaviour
    {
        public static ToggleUIController Instance;
        public static bool Initialized { get; private set; }
        public static bool ShowUI { get; set; }

        private static readonly List<GameObject> _objectsToHide = [];
        private static readonly Vector3 _vector3Zero = Vector3.zero;
        private static readonly KeyBinding _kb = new(KeyCode.F6);
        private static RectTransform _shipHealthFillImageRt;

        private void Awake()
        {
            Instance = this;

            // Cache the objects to hide once and not every initialization.
            if (_objectsToHide.Count == 0)
            {
                // UI object tree.
                _objectsToHide.Add(UIDisplayManager.instance.gameObject.transform.parent.gameObject);
                // Match chat panel.
                _objectsToHide.Add(GameObject.Find("Menu UI/Standard Canvas/Menu Header Footer/Match Chat Panel"));
                // First-person equipment object.
                _objectsToHide.Add(GameObject.Find("GameCameraRig/CutScene/MainCamera/FoVOffset/PlayerEquipment"));
                // Practice mode aim guide.
                _objectsToHide.Add(AimGuide.Instance.gameObject);

                _shipHealthFillImageRt = GameObject.Find("/Game UI/Match UI/UI HUD Canvas/UI HUD/UI Ship Health Display/Health Bar/Ship Health Slider/Fill Area")
                    .GetComponent<RectTransform>();
            }

            ShowUI = true;
            Initialized = true;
            // If UI was hidden when leaving the match, it doesn't show up until the key bind is toggled.
            // This simulates that and ensures the UI is visible on initialization.
            ApplyUIState();
        }

        private void OnDisable()
        {
            ShowUI = true;
            Initialized = false;
        }

        private void LateUpdate()
        {
            if (_kb.GetDown())
            {
                ShowUI = !ShowUI;
                MuseLog.Info($"Setting match UI display to {ShowUI}...");
                ApplyUIState();
            }
            // Some things get re-activated elsewhere (e.g. change states, close pause menu, etc.) so disable every update
            if (!ShowUI)
            {
                UINameTagDisplay.Deactivate();
                UIRightPanel.Deactivate(); // Practice mode command panel.
                foreach (var gameObject in _objectsToHide)
                {
                    if (gameObject.activeSelf != ShowUI)
                    {
                        gameObject.SetActive(ShowUI);
                    }
                }
            }
            else
            {
                // If the ship dies with the UI off, sometimes the position of healthFillImage is set to NaN.
                _shipHealthFillImageRt.anchoredPosition3D = _vector3Zero;
            }
        }

        /// <summary>
        /// Updates the visibility of UI elements. Intended to be used on key bind toggle and initialization, not every frame.
        /// </summary>
        private static void ApplyUIState()
        {
            foreach (var gameObject in _objectsToHide)
            {
                if (gameObject.activeSelf != ShowUI)
                {
                    gameObject.SetActive(ShowUI);
                }
            }
            if (ShowUI)
            {
                UIRepairComponentView.Activate();
                UINameTagDisplay.Activate(); // Doesn't get reactivated until match menu is closed. This fixes that.
                UIRightPanel.Activate(); // Practice mode command panel.
            }
            else
            {
                UIShipDetailsView.HideCrewToolInspectors(0);
                UIRightPanel.Deactivate(); // Practice mode command panel.
            }
        }
    }
}