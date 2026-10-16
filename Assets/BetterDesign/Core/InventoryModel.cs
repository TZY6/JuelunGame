using System;

namespace BetterDesign.Core
{
    public enum ItemId { None, Gold, Supply, Mushroom }
    public static class ItemCatalog
    {
        public static int StackLimit(ItemId item) { return item == ItemId.Supply ? 10 : item == ItemId.Mushroom ? 20 : 0; }
        public static string Name(ItemId item)
        {
            switch (item) { case ItemId.Gold: return "金币"; case ItemId.Supply: return "治疗补给"; case ItemId.Mushroom: return "蘑菇材料"; default: return "空格"; }
        }
        public static string Description(ItemId item)
        {
            switch (item)
            {
                case ItemId.Supply: return "使用：恢复 40 点生命。\n满血或死亡时不会消耗。";
                case ItemId.Mushroom: return "制作材料，可在背包出售：每个 2 金币。";
                default: return "点击格子选择物品；可使用、丢弃或出售材料。";
            }
        }
    }
    [Serializable] public struct ItemStack { public ItemId item; public int amount; }
    [Serializable] public struct LootSnapshot { public ItemId item; public int amount; public float x,y; }
    public sealed class InventoryModel
    {
        public const int Capacity = 12;
        public int Revision { get; private set; }
        readonly ItemStack[] slots = new ItemStack[Capacity];
        public ItemStack this[int index] { get { return slots[index]; } }
        public int Occupied { get { int n = 0; for (int i = 0; i < Capacity; i++) if (slots[i].amount > 0) n++; return n; } }
        public int Count(ItemId item) { int n = 0; for (int i = 0; i < Capacity; i++) if (slots[i].item == item) n += slots[i].amount; return n; }
        public int Find(ItemId item) { for (int i = 0; i < Capacity; i++) if (slots[i].item == item && slots[i].amount > 0) return i; return -1; }
        public bool CanAdd(ItemId item,int amount)
        {
            int limit = ItemCatalog.StackLimit(item);
            if (limit == 0 || amount <= 0) return false;
            int room = 0;
            for (int i = 0; i < Capacity; i++)
            {
                if (slots[i].item == item) room += limit - slots[i].amount;
                else if (slots[i].amount == 0) room += limit;
            }
            return room >= amount;
        }
        public bool TryAdd(ItemId item,int amount)
        {
            if (!CanAdd(item,amount)) return false; // Atomic: no partial pickup if bag is full.
            int limit = ItemCatalog.StackLimit(item);
            for (int pass = 0; pass < 2 && amount > 0; pass++)
                for (int i = 0; i < Capacity && amount > 0; i++)
                    if ((pass == 0 && slots[i].item == item) || (pass == 1 && slots[i].amount == 0))
                    {
                        int take = Math.Min(amount,limit - slots[i].amount);
                        slots[i].item = item; slots[i].amount += take; amount -= take;
                    }
            Revision++; return true;
        }
        public bool TryRemove(int index,int amount)
        {
            if (index < 0 || index >= Capacity || amount <= 0 || slots[index].amount < amount) return false;
            slots[index].amount -= amount;
            if (slots[index].amount == 0) slots[index].item = ItemId.None;
            Revision++; return true;
        }
        public ItemStack[] Capture() { return (ItemStack[])slots.Clone(); }
        public static bool IsValid(ItemStack[] data)
        {
            if (data == null || data.Length != Capacity) return false;
            for (int i = 0; i < data.Length; i++)
                if (data[i].amount == 0 ? data[i].item != ItemId.None :
                    data[i].amount < 0 || data[i].amount > ItemCatalog.StackLimit(data[i].item)) return false;
            return true;
        }
        public void Restore(ItemStack[] data)
        {
            if (!IsValid(data)) throw new ArgumentException("Invalid inventory snapshot.");
            Array.Copy(data,slots,Capacity); Revision++;
        }
    }
}
