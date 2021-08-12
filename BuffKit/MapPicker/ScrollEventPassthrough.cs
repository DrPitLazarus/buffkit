using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BuffKit.MapPicker
{
    public class ScrollEventPassthrough : MonoBehaviour
    {
        public void Start()
        {
            var dropdown = UIPageFrame.Instance.modalDialog.dropdown;
            var scrollRect = dropdown.transform.FindChild("Dropdown List").GetComponent<ScrollRect>();
            var et = gameObject.GetComponent<EventTrigger>();
            
            var entry = new EventTrigger.Entry {eventID = EventTriggerType.Scroll};
            entry.callback.AddListener((BaseEventData data) =>
            {
                scrollRect.gameObject.SendMessage("OnScroll", data);
            });
            
            et.triggers.Add(entry);
            Destroy(this);
        }
    }
}