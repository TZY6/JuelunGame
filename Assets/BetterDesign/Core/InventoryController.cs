using System;

namespace BetterDesign.Core
{
    public interface IInventoryView { void Render(InventoryModel model,int selected); }
    public interface IInventoryActions
    {
        bool UseSlot(int index);
        bool DropSlot(int index);
        void SellMaterials();
    }
    // MVC controller depends on model/view interfaces, not Unity, TMP or GameObjects.
    public sealed class InventoryController : IDisposable
    {
        readonly GameModel model;
        readonly IInventoryView view;
        readonly IInventoryActions actions;
        readonly IDisposable subscription;
        int selected;
        public InventoryController(GameModel model,GameEvents events,IInventoryView view,IInventoryActions actions)
        {
            this.model = model; this.view = view; this.actions = actions;
            subscription = events.Model.Subscribe(OnChanged); Render();
        }
        void OnChanged(ModelChanged _) { Render(); }
        void Render() { view.Render(model.Inventory,selected); }
        public void Select(int index) { if (index >= 0 && index < InventoryModel.Capacity) { selected = index; Render(); } }
        public void UseSelected() { actions.UseSlot(selected); }
        public void DropSelected() { actions.DropSlot(selected); }
        public void SellMaterials() { actions.SellMaterials(); }
        public void Dispose() { subscription.Dispose(); }
    }
}
