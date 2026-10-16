using UnityEngine;
using UnityEngine.EventSystems;
namespace BetterDesign
{
    [DisallowMultipleComponent]
    public sealed class InventorySlotDrag : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler,IDropHandler
    {
        [SerializeField] InventoryView view;
        [SerializeField] int index;
        public InventoryView View { get { return view; } }
        public int Index { get { return index; } }
        public void OnBeginDrag(PointerEventData data)
        {
            if (data.button == PointerEventData.InputButton.Left && view != null) view.BeginDrag(index,data);
        }
        public void OnDrag(PointerEventData data) { if (view != null) view.UpdateDrag(index,data); }
        public void OnEndDrag(PointerEventData data) { if (view != null) view.EndDrag(index); }
        public void OnDrop(PointerEventData data)
        {
            var source = data.pointerDrag == null ? null : data.pointerDrag.GetComponent<InventorySlotDrag>();
            if (source != null && source.view == view && view != null) view.DropOn(source.index,index);
        }
        void OnDisable() { if (view != null) view.EndDrag(index); }
    }
}
