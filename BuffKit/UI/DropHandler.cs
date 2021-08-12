using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Math = Muse.Math;
using Transform = UnityEngine.Transform;
using Vector3 = UnityEngine.Vector3;

namespace BuffKit.UI
{
    public class DropHandler : MonoBehaviour
    {
        public event Action<SortedList<int, Transform>, SortedList<int, Transform>> OnDropped;

        private SortedList<int, Transform> _startingOrder = new SortedList<int, Transform>();

        private void Start()
        {
            if (gameObject.GetComponent<HorizontalOrVerticalLayoutGroup>() is null)
            {
                MuseLog.Debug("DnD found itself in a non-layout GO, initiating self-destruct");
                Destroy(this);
                return;
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                var c = transform.GetChild(i);
                
                if (c.GetComponent<LayoutElement>() != null)
                {
                    c.gameObject.AddComponent<Dragger>();
                    _startingOrder.Add(i, c);
                }
            }
        }

        private void Dropped()
        {
            var order = new SortedList<int, Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                var c = transform.GetChild(i);
                if (!(c.GetComponent<Dragger>() is null) && c.gameObject.activeInHierarchy)
                {
                    order.Add(i, transform.GetChild(i));
                }
            }
            
            OnDropped?.Invoke(_startingOrder, order);

            _startingOrder = order;
        }
    }

    public class Dragger : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerUpHandler
    {
        private HorizontalOrVerticalLayoutGroup _lg;
        private bool _isHorizontal;

        //An invisible clone that takes up the element's layout slot
        private GameObject _shadow;

        //Used to check whether we need to null-out the PointerUp event
        private bool _dragged = false;
        
        private void Start()
        {
            _lg = transform.parent.GetComponent<HorizontalOrVerticalLayoutGroup>();
            if (_lg is null)
            {
                MuseLog.Debug("DnD found itself in a child of non-layout GO, initiating self-destruct");
                Destroy(this);
                return;
            }

            _isHorizontal = _lg is HorizontalLayoutGroup;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _dragged = true;
            
            //Clone self to take up space in the layout
            _shadow = Instantiate(gameObject, transform.parent);
            //Make shadow invisible while still keeping its original layout box dimensions
            _shadow.transform.localScale = new Vector3(0, 0, 0);
            
            //The rebuilds are needed to prevent jitter when the component is first picked up
            LayoutRebuilder.ForceRebuildLayoutImmediate(_lg.GetComponent<RectTransform>());

            //Yeet ourselves out of the layout
            transform.GetComponent<LayoutElement>().ignoreLayout = true;
            //Make sure we are getting drawn above everything else
            transform.SetAsLastSibling();
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(_lg.GetComponent<RectTransform>());
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position;
            var rt = transform.GetComponent<RectTransform>();

            int closestSiblingIndex = int.MinValue;
            float closestSiblingDistance = float.PositiveInfinity;
            
            // Find the closest sibling 
            for (int i = 0; i < transform.parent.childCount; i++)
            {
                // Ignore self, as it is getting dragged around and doing all sorts of strange things
                if (i == transform.GetSiblingIndex()) continue; 

                // Others should have rect transforms too
                if (!(transform.parent.GetChild(i).GetComponent<RectTransform>() is RectTransform t)) continue;

                //Distance between the centers
                float distance;
                if (_isHorizontal)
                {
                    distance = Math.Abs(
                        (rt.position.x + rt.sizeDelta.x / 2) - (t.position.x + t.sizeDelta.x / 2)
                    );
                }
                else
                {
                    distance = Math.Abs(
                        (rt.position.y + rt.sizeDelta.y / 2) - (t.position.y + t.sizeDelta.y / 2)
                    );
                }
                
                if (distance < closestSiblingDistance)
                {
                    closestSiblingIndex = i;
                    closestSiblingDistance = distance;
                }
            }
            
            
            // If shadow is the closest sibling, return
            var shadowIndex = _shadow.transform.GetSiblingIndex();
            if (shadowIndex == closestSiblingIndex)
            {
                return;
            }

            _shadow.transform.SetSiblingIndex(closestSiblingIndex);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_lg.GetComponent<RectTransform>());
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            gameObject.GetComponent<LayoutElement>().ignoreLayout = false;

            var index = _shadow.transform.GetSiblingIndex();
            transform.SetSiblingIndex(index);
            Destroy(_shadow);

            LayoutRebuilder.ForceRebuildLayoutImmediate(_lg.GetComponent<RectTransform>());

            _dragged = false;
            _lg.gameObject.SendMessage("Dropped");
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_dragged)
            {
                eventData.pointerPress = null;
            }
        }
    }
}