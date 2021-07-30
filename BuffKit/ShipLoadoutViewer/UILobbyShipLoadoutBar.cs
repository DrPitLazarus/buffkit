using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Muse.Goi2.Entity;
using Muse.Goi2.Entity.Vo;
using Muse.Common;

namespace BuffKit.ShipLoadoutViewer
{
    class UILobbyShipLoadoutBar : MonoBehaviour
    {

        BepInEx.Logging.ManualLogSource log;

        private void Awake()
        {
        }

        List<RawImage> slotImages;

        Image backgroundImg;
        Image headerImg;
        Color baseBGCol;

        public void Build(BepInEx.Logging.ManualLogSource log)
        {
            this.log = log;
            gameObject.transform.SetSiblingIndex(1);                                              // Make it just below the ship header
            gameObject.transform.localPosition = new Vector3(0, 0, 0);
            log.LogInfo("Added loadoutPanel : " + gameObject.transform.GetHierarchyPath());
            var hlg = gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 1;
            hlg.padding = new RectOffset(1, 1, 1, 1);
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            var leSample = gameObject.transform.parent.GetChild(0).GetComponent<LayoutElement>(); // LayoutElement from ship header
            var le = gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = leSample.preferredWidth;
            le.preferredHeight = leSample.preferredHeight + 2;
            backgroundImg = gameObject.AddComponent<Image>();
            headerImg = gameObject.transform.parent.GetChild(0).GetComponent<Image>();

            slotImages = new List<RawImage>();
            foreach (var i in Enumerable.Range(0, 6))
            {
                var slot = new GameObject("slot" + (i + 1));
                slot.transform.parent = gameObject.transform;
                slot.transform.localPosition = new Vector3(0, 0, 0);
                slotImages.Add(slot.AddComponent<RawImage>());
                le = slot.AddComponent<LayoutElement>();
                le.preferredWidth = 26;
                le.preferredHeight = 26;
            }
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
                    shipGuns = new int[6];
                    for (int i = 0; i < 6; i++) shipGuns[i] = -1;
                    shipClass = svo.ModelId;
                    var gunSlots = svo.Presets[0].Guns;
                    foreach (var slot in gunSlots)
                    {
                        var slotIndex = ShipLoadoutViewer.shipAndGunSlotToIndex[shipClass][slot.Name];
                        shipGuns[slotIndex] = slot.GunId;
                    }
                    availableSlots = svo.Model.GunSlots;
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

            public Texture GetSlotTexture(int slot)
            {
                int gunId = shipGuns[slot];
                if (gunId == -1) return UIManager.IconForNullOrEmpty;
                return ShipLoadoutViewer.gunIcons[gunId];
            }

            public bool Equals(ShipLoadoutData other)
            {
                if (other == null) return false;
                if (other.shipClass != shipClass) return false;
                for (int i = 0; i < availableSlots; i++)
                    if (other.shipGuns[i] != shipGuns[i]) return false;
                return true;
            }
        }

        ShipLoadoutData lastShip = null;
        public void DisplayShip(ShipViewObject svo)
        {
            baseBGCol = headerImg.color;
            baseBGCol.a = 0.5f;

            var newShip = new ShipLoadoutData(svo);
            for (int i = 0; i < newShip.GetVisibleSlots(); i++)
            {
                slotImages[i].texture = newShip.GetSlotTexture(i);
                slotImages[i].gameObject.SetActive(true);
            }
            for (int i = newShip.GetVisibleSlots(); i < 6; i++)
                slotImages[i].gameObject.SetActive(false);

            if (!newShip.Equals(lastShip))
                DisplayChanged();

            lastShip = newShip;
        }
    }
}
