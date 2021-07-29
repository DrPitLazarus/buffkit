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
            le.preferredHeight = leSample.preferredHeight;

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
                foreach(var i in slotImages)
                    i.gameObject.SetActive(false);
            }
            else
            {
                log.LogInfo($"Displaying ship : {svo.Name}");
                // Logic from UINewShipState.MainMode
                /*
                var guns = new List<ShipSlotViewObject>();
                foreach (string text in
                    from p in svo.Model.Slots
                    where p.Value.SlotType == Muse.Goi2.Entity.ShipPartSlotType.GUN
                    select p.Key)
                {
                    var gi = svo.Model.GetDefaultGuns(NetworkedPlayer.Local.GameType, svo.CurrentPresetIndex).NullSafeGet(text, null);
                    ShipSlotViewObject item = new ShipSlotViewObject
                    {
                        Name = text,
                        Size = svo.Model.Slots[text].SlotSize,
                        GunId = (gi == null) ? 0 : gi.Id
                    };
                    guns.Add(item);
                }
                var sortedSlots = svo.Model.GetSortedSlots(svo.Presets[0].Guns);
                */
                var sortedSlots = svo.Model.GetSortedSlots(svo.Presets[0].Guns);
                var sortedGunsIds = (from slot in sortedSlots select slot.Gun.Id).ToList();
                for(int i = 0; i < sortedGunsIds.Count; i++)
                {
                    slotImages[i].texture = ShipLoadoutViewer.gunIcons[sortedGunsIds[i]];
                    slotImages[i].gameObject.SetActive(true);
                }
                for(int i = sortedGunsIds.Count; i < 6; i++)
                {
                    slotImages[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
