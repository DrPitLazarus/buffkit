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

        public void Build(BepInEx.Logging.ManualLogSource log)
        {
            this.log = log;
            gameObject.transform.SetSiblingIndex(1);                                              // Make it just below the ship header
            gameObject.transform.localPosition = new Vector3(0, 0, 0);
            log.LogInfo("Added loadoutPanel : " + gameObject.transform.GetHierarchyPath());
            var hlg = gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 1;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            var leSample = gameObject.transform.parent.GetChild(0).GetComponent<LayoutElement>(); // LayoutElement from ship header
            var le = gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = leSample.preferredWidth;
            le.preferredHeight = 30;

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

        public void DisplayShip(ShipViewObject svo)
        {
            if (svo == null)
            {
                log.LogInfo("Displaying ship : [no ship]");
                foreach (var i in slotImages)
                    i.gameObject.SetActive(false);
            }
            else
            {
                log.LogInfo($"Displaying ship : {svo.Name}");

                var guns = svo.Presets[0].Guns;
                for (int i = 0; i < svo.Model.GunSlots; i++)
                {
                    slotImages[i].gameObject.SetActive(true);
                    slotImages[i].texture = UIManager.IconForNullOrEmpty;
                }
                for (int i = svo.Model.GunSlots; i < 6; i++)
                    slotImages[i].gameObject.SetActive(false);

                foreach (var slot in guns)
                {
                    if (!ShipLoadoutViewer.dataLoaded) continue;
                    var slotName = slot.Name;
                    var definiteModelId = svo.Model.Id;
                    var modelId = svo.ModelId;
                    var dict = ShipLoadoutViewer.shipAndGunSlotToIndex[modelId];
                    var slotIndex = dict[slotName];
                    log.LogInfo($"  {slotName} {definiteModelId} {modelId} {slotIndex}");
                    if (!dict.ContainsKey(slotName)) log.LogError("Slot name not in dict!");
                    else
                        slotImages[slotIndex].texture = ShipLoadoutViewer.gunIcons[slot.GunId];
                }
            }
        }
    }
}
