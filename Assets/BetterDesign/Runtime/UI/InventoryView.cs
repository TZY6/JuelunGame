using BetterDesign.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
                    details == null || useButton == null || dropButton == null || sellButton == null || supplyIcon == null || mushroomIcon == null) return false;
                for (int i = 0; i < InventoryModel.Capacity; i++)
                    if (slotNames[i] == null || slotCounts[i] == null || slotIcons[i] == null || slotBackgrounds[i] == null) return false;
                return true;
            }
        }
        public void Bind(InventoryController controller) { this.controller = controller; rendered = false; if (controller != null) controller.SetVisible(isActiveAndEnabled); }
        void OnEnable() { rendered = false; if (controller != null) controller.SetVisible(true); }
        void OnDisable() { if (controller != null) controller.SetVisible(false); }
        void OnDestroy() { controller = null; }
        public void SelectSlot(int index) { if (controller != null) controller.Select(index); }
        public void UseSelected() { if (controller != null) controller.UseSelected(); }
        public void DropSelected() { if (controller != null) controller.DropSelected(); }
        public void SellMaterials() { if (controller != null) controller.SellMaterials(); }
        public void Render(InventoryModel model,int selected)
        {
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
