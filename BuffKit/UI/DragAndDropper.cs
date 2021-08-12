using System.Collections.Generic;
using Muse;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Transform = UnityEngine.Transform;
using Vector3 = UnityEngine.Vector3;

namespace BuffKit.UI
{
    public class DragAndDropper : MonoBehaviour
    {
        private Dictionary<int, Transform> _order = new Dictionary<int, Transform>();

        private void Start()
        {
            if (gameObject.GetComponent<HorizontalOrVerticalLayoutGroup>() is null)
            {
                MuseLog.Debug("DnD found itself in a non-layout GO, initiating self-destruct");
                Destroy(this);
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                _order.Add(i, transform.GetChild(i));
            }

            foreach (var i in _order)
            {
                if (i.Value.GetComponent<LayoutElement>() != null)
                {
                    var d = i.Value.gameObject.AddComponent<Dragger>();
                }
            }
        }

        // private void OnTransformChildrenChanged()
        // {
        //     
        // }
    }

    public class Dragger : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerUpHandler
    {
        public HorizontalOrVerticalLayoutGroup lg;

        //An invisible clone that takes up the element's layout slot
        private GameObject _shadow;
        private int _startingIndex;
        private Vector3 _startingPosition;
        private readonly Dictionary<int, Vector3> _siblingPositions = new Dictionary<int, Vector3>();
        private bool _dragged = false;

        private readonly SortedList<int, RectTransform> _siblings =
            new SortedList<int, RectTransform>();

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
            _shadow.SetActive(true);

            //Yeet ourselves out of the layout
            transform.GetComponent<LayoutElement>().ignoreLayout = true;
            //Make sure we are getting drawn above everything else
            transform.SetAsLastSibling();
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position;
            var rt = transform.GetComponent<RectTransform>();

            int closestSiblingIndex = 0;
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

                // If shadow is the closest sibling, return
                var shadowIndex = _shadow.transform.GetSiblingIndex();
                if (shadowIndex == closestSiblingIndex) return;

                _shadow.transform.SetSiblingIndex(closestSiblingIndex);
                LayoutRebuilder.ForceRebuildLayoutImmediate(_lg.GetComponent<RectTransform>());
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            gameObject.GetComponent<LayoutElement>().ignoreLayout = false;

            var index = _shadow.transform.GetSiblingIndex();
            transform.SetSiblingIndex(index);
            Destroy(_shadow);

            LayoutRebuilder.ForceRebuildLayoutImmediate(_lg.GetComponent<RectTransform>());

            _dragged = false;
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