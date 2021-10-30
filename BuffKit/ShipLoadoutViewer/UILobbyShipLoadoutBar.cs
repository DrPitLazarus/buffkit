using System;
using System.Collections.Generic;
using System.Linq;
using Muse.Goi2.Entity;
using Muse.Goi2.Entity.Vo;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Resources = BuffKit.UI.Resources;

namespace BuffKit.ShipLoadoutViewer
{
    class UILobbyShipLoadoutBar : MonoBehaviour
    {
        public bool MarkForRedraw { set; get; }

        List<UIShipLoadoutSlot> slots;

        Image backgroundImg;
        Image headerImg;
        Color baseBGCol;

        public static GameObject Build(Transform parent, out UILobbyShipLoadoutBar loadoutBar)
        {
            var obPanel = new GameObject("Ship Loadout Panel");
            obPanel.transform.SetParent(parent);
            loadoutBar = obPanel.AddComponent<UILobbyShipLoadoutBar>();
            var hlg = obPanel.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 1;
            hlg.padding = new RectOffset(1, 1, 1, 1);
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            var leSample = obPanel.transform.parent.GetChild(0).GetComponent<LayoutElement>(); // LayoutElement from ship header
            var le = obPanel.AddComponent<LayoutElement>();
            le.preferredWidth = leSample.preferredWidth;
            le.preferredHeight = leSample.preferredHeight + 2;
            leSample.preferredWidth = le.preferredHeight;
            loadoutBar.backgroundImg = obPanel.AddComponent<Image>();
            loadoutBar.headerImg = obPanel.transform.parent.GetChild(0).GetComponent<Image>();

            loadoutBar.slots = new List<UIShipLoadoutSlot>();
            foreach (var i in Enumerable.Range(0, 6))
            {
                var slot = new GameObject("slot" + (i + 1));
                slot.transform.parent = obPanel.transform;
                slot.transform.localPosition = new Vector3(0, 0, 0);
                le = slot.AddComponent<LayoutElement>();
                le.preferredWidth = 26;
                le.preferredHeight = 26;
                loadoutBar.slots.Add(slot.AddComponent<UIShipLoadoutSlot>());
            }

            return obPanel;
        }

        float currentFadeOut = 0f;
        float timeToFadeOut = 3f;
        void DisplayChanged()
        {
            currentFadeOut = timeToFadeOut;
        }

        void Update()
        {
            currentFadeOut = Mathf.Clamp(currentFadeOut - Time.deltaTime, 0, Mathf.Infinity);
            backgroundImg.color = Color.Lerp(baseBGCol, Color.white, currentFadeOut / timeToFadeOut);
            if (MarkForRedraw)
            {
                DisplayShipFromData(lastShip);
                MarkForRedraw = false;
            }
        }

        class ShipLoadoutData : IEquatable<ShipLoadoutData>
        {
            int shipClass = -1;
            int[] shipGuns;
            int availableSlots = 0;
            public ShipLoadoutData(ShipViewObject svo)
            {
                if (svo != null)
                {
                    shipClass = svo.ModelId;
                    availableSlots = svo.Model.GunSlots;
                    shipGuns = Util.GetSortedGunIds(svo.Model, svo.Presets[0].Guns);
                }
            }

            public override string ToString()
            {
                string str = $"Class: {shipClass}, Guns:";
                for (int i = 0; i < availableSlots; i++) str += $" {shipGuns[i]}";
                return str;
            }

            public int GetVisibleSlots()
            {
                return availableSlots;
            }
            public int GetGunId(int slotIndex)
            {
                return shipGuns[slotIndex];
            }

            public bool Equals(ShipLoadoutData other)
            {
                // if (other == null) return false;
                if (other.shipClass != shipClass) return false;
                for (int i = 0; i < availableSlots; i++)
                    if (other.shipGuns[i] != shipGuns[i]) return false;
                return true;
            }
        }

        ShipLoadoutData lastShip = new ShipLoadoutData(null);
        public void DisplayShip(ShipViewObject svo)
        {
            baseBGCol = headerImg.color;
            baseBGCol.a = 0.5f;

            var newShip = new ShipLoadoutData(svo);

            DisplayShipFromData(newShip);

            if (!newShip.Equals(lastShip))
                DisplayChanged();

            lastShip = newShip;
        }
        void DisplayShipFromData(ShipLoadoutData data)
        {
            for (int i = 0; i < data.GetVisibleSlots(); i++)
            {
                slots[i].SetGun(data.GetGunId(i));
                slots[i].gameObject.SetActive(true);
            }
            for (int i = data.GetVisibleSlots(); i < 6; i++)
                slots[i].gameObject.SetActive(false);
        }
    }

    class UIShipLoadoutSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public enum UIShipLoadoutSlotInfoViewer
        {
            Disabled,
            Hover,
            Click
        }
        public static UIShipLoadoutSlotInfoViewer InfoDisplaySetting = UIShipLoadoutSlotInfoViewer.Click;

        private int _gunId = -1;
        private GunItemInfo _gunInfo;
        private RawImage _image;
        public void SetGun(int gunId)
        {
            _gunId = gunId;
            _image.texture = Resources.GetGunTexture(gunId);
            _gunInfo = GunItemInfo.FromGunItem(CachedRepository.Instance.Get<GunItem>(_gunId));
        }
        private void Awake()
        {
            _image = gameObject.AddComponent<RawImage>();
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (InfoDisplaySetting != UIShipLoadoutSlotInfoViewer.Hover) return;
            if (_gunId == -1) return;
            UIGunTooltip.Instance.RenderGun(_gunInfo);
            UIGunTooltip.Instance.ShowAtScreenPosition(UIShipCustomizationScreen.Instance.gunTooltipAnchor.position, new Vector2?(new Vector2(0f, 1f)), 0f);
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            UIGunTooltip.Instance.Hide();
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (InfoDisplaySetting != UIShipLoadoutSlotInfoViewer.Click) return;
            if (_gunId == -1) return;
            UIGunTooltip.Instance.RenderGun(_gunInfo);
            UIGunTooltip.Instance.ShowAtScreenPosition(UIShipCustomizationScreen.Instance.gunTooltipAnchor.position, new Vector2?(new Vector2(0f, 1f)), 0f);
        }
    }
}
