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
        readonly GameEvents events;
        IDisposable subscription;
        int selected;
        bool disposed;
        public InventoryController(GameModel model,GameEvents events,IInventoryView view,IInventoryActions actions)
        {
            this.model = model; this.view = view; this.actions = actions; this.events = events;
            subscription = events.InventoryChanged.Subscribe(OnChanged); Render();
        }
        void OnChanged(InventoryChanged change) { Render(); }
        void Render() { view.Render(model.Inventory,selected); }
        public void Select(int index) { if (!disposed && index >= 0 && index < InventoryModel.Capacity) { selected = index; Render(); } }
        public void UseSelected() { if (!disposed) actions.UseSlot(selected); }
        public void DropSelected() { if (!disposed) actions.DropSlot(selected); }
        public void SellMaterials() { if (!disposed) actions.SellMaterials(); }
        public void SetVisible(bool visible)
        {
            if (disposed) return;
            if (subscription != null) { subscription.Dispose(); subscription = null; }
            if (!visible) return;
            subscription = events.InventoryChanged.Subscribe(OnChanged); Render();
        }
        public void Dispose() { SetVisible(false); disposed = true; }
    }
}
