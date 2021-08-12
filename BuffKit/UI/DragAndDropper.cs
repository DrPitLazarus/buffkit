using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
                    d.lg = gameObject.GetComponent<HorizontalOrVerticalLayoutGroup>();
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

        private void UpdateSiblingPositions()
        {
            _siblingPositions.Clear();
            for (int i = 0; i < transform.parent.childCount; i++)
            {
                if (i == transform.GetSiblingIndex()) continue;
                var c = transform.parent.GetChild(i);
                _siblingPositions.Add(i, c.position);
            }
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            _dragged = true;
            _startingIndex = transform.GetSiblingIndex();
            _startingPosition = transform.position;

            UpdateSiblingPositions();
            
            // Clone self to take up space in the layout
             _shadow = Instantiate(gameObject, transform.parent);
             _shadow.SetActive(true);
            
            //Make shadow invisible
            _shadow.transform.localScale = new Vector3(0, 0, 0);
            //Yeet ourselves out of the layout
            transform.GetComponent<LayoutElement>().ignoreLayout = true;
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position;

            int previousSiblingIndex = -1;
            foreach (var position in _siblingPositions)
            {
                if (transform.position.x > position.Value.x && position.Key >= previousSiblingIndex)
                {
                    previousSiblingIndex = position.Key;
                }
            }

            var index = _shadow.transform.GetSiblingIndex();
            if (index == previousSiblingIndex + 1) return;
            
            _shadow.transform.SetSiblingIndex(previousSiblingIndex + 1);
            LayoutRebuilder.ForceRebuildLayoutImmediate(lg.GetComponent<RectTransform>());
            UpdateSiblingPositions();
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            gameObject.GetComponent<LayoutElement>().ignoreLayout = false;
            
            var index = _shadow.transform.GetSiblingIndex();
            transform.SetSiblingIndex(index);
            Destroy(_shadow);
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(lg.GetComponent<RectTransform>());

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