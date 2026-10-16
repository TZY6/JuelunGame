using BetterDesign.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BetterDesign
{
    // MVC View: preconfigured 4x3 grid. UI callbacks forward commands to InventoryController.
    public sealed class InventoryView : MonoBehaviour, IInventoryView
    {
        [SerializeField] TMP_Text[] slotNames,slotCounts;
        [SerializeField] Image[] slotIcons,slotBackgrounds;
        [SerializeField] TMP_Text details;
        [SerializeField] Button useButton,dropButton,sellButton;
        [SerializeField] Sprite supplyIcon,mushroomIcon;
        [SerializeField] Image dragIcon;
        int dragSource = -1,dragRevision;
        public bool IsDragging { get { return dragSource >= 0; } }
        public Image DragIcon { get { return dragIcon; } }
        InventoryController controller;
        readonly ItemStack[] displayed = new ItemStack[InventoryModel.Capacity];
        int previousSelection = -1;
        bool rendered;
        public bool IsConfigured
        {
            get
            {
                if (slotNames == null || slotCounts == null || slotIcons == null || slotBackgrounds == null ||
                    slotNames.Length != InventoryModel.Capacity || slotCounts.Length != InventoryModel.Capacity ||
                    slotIcons.Length != InventoryModel.Capacity || slotBackgrounds.Length != InventoryModel.Capacity ||
                    dragIcon == null || dragIcon.raycastTarget || details == null || useButton == null || dropButton == null || sellButton == null || supplyIcon == null || mushroomIcon == null) return false;
                for (int i = 0; i < InventoryModel.Capacity; i++)
                    if (slotNames[i] == null || slotCounts[i] == null || slotIcons[i] == null || slotBackgrounds[i] == null) return false;
                return true;
            }
        }
        public void Bind(InventoryController controller)
        {
            CancelDrag(); if (this.controller != null) this.controller.SetVisible(false);
            this.controller = controller; rendered = false;
            if (controller != null) controller.SetVisible(isActiveAndEnabled);
        }
        public void Open() { if (isActiveAndEnabled && controller != null) controller.SetVisible(true); }
        public void Close() { CancelDrag(); if (controller != null) controller.SetVisible(false); }
        void OnEnable() { rendered = false; if (controller != null) controller.SetVisible(true); }
        void OnDisable() { Close(); }
        void OnDestroy() { CancelDrag(); controller = null; }
        void OnApplicationFocus(bool focused) { if (!focused) CancelDrag(); }
        public bool BeginDrag(int index,PointerEventData data)
        {
            CancelDrag();
            if (controller == null || !isActiveAndEnabled || index < 0 || index >= InventoryModel.Capacity || displayed[index].amount == 0 || dragIcon == null) return false;
            dragSource = index; dragRevision = controller.Revision;
            dragIcon.sprite = slotIcons[index].sprite;
            dragIcon.gameObject.SetActive(true);
            slotIcons[index].color = new Color(1,1,1,.3f);
            UpdateDrag(index,data); return true;
        }
        public void UpdateDrag(int index,PointerEventData data)
        {
            if (dragSource != index || dragIcon == null || data == null) return;
            Vector2 point;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)dragIcon.transform.parent,data.position,data.pressEventCamera,out point))
                dragIcon.rectTransform.anchoredPosition = point;
        }
        public bool DropOn(int from,int to)
        {
            if (!isActiveAndEnabled || !IsDragging || controller == null || from != dragSource || controller.Revision != dragRevision) { CancelDrag(); return false; }
            bool moved = controller.MoveSlot(from,to);
            CancelDrag(); return moved;
        }
        public void EndDrag(int index) { if (dragSource == index) CancelDrag(); }
        void CancelDrag()
        {
            if (dragSource >= 0 && slotIcons != null && dragSource < slotIcons.Length && slotIcons[dragSource] != null) slotIcons[dragSource].color = Color.white;
            dragSource = -1;
            if (dragIcon != null) dragIcon.gameObject.SetActive(false);
        }
        public void SelectSlot(int index) { if (controller != null) controller.Select(index); }
        public void UseSelected() { if (controller != null) controller.UseSelected(); }
        public void DropSelected() { if (controller != null) controller.DropSelected(); }
        public void SellMaterials() { if (controller != null) controller.SellMaterials(); }
        public void Render(InventoryModel model,int selected)
        {
            if (IsDragging && (controller == null || dragRevision != model.Revision)) CancelDrag();
            for (int i = 0; i < InventoryModel.Capacity; i++)
            {
                var slot = model[i];
                if (rendered && displayed[i].item == slot.item && displayed[i].amount == slot.amount &&
                    (i == selected) == (i == previousSelection)) continue;
                displayed[i] = slot;
                slotNames[i].text = slot.amount > 0 ? ItemCatalog.Name(slot.item) : "空格";
                slotCounts[i].text = slot.amount > 0 ? "×" + slot.amount : "";
                slotIcons[i].enabled = slot.amount > 0;
                slotIcons[i].sprite = slot.item == ItemId.Supply ? supplyIcon : mushroomIcon;
                slotBackgrounds[i].color = i == selected ? new Color(.15f,.46f,.47f,1) : new Color(.10f,.21f,.27f,1);
            }
            rendered = true; previousSelection = selected;
            var current = model[selected];
            details.text = ItemCatalog.Name(current.item) + "\n\n" + ItemCatalog.Description(current.item) +
                "\n\n已用格子：" + model.Occupied + "/" + InventoryModel.Capacity;
            useButton.interactable = current.item == ItemId.Supply && current.amount > 0;
            dropButton.interactable = current.amount > 0;
            sellButton.interactable = model.Count(ItemId.Mushroom) > 0;
        }
    }
}
