using System;

namespace BetterDesign.Core
{
    public interface IPoolItem
    {
        bool IsRented { get; set; }
        void OnRent();
        void OnReturn();
    }
    // Bounded, prewarmed pool; exhaustion is observable, never silently allocates.
    public sealed class FixedPool<T> where T : class, IPoolItem
    {
        readonly T[] items;
        int available;
        public int AvailableCapacity { get { return available; } }
        public int Active { get; private set; }
        public int Peak { get; private set; }
        public int Misses { get; private set; }
        public int Capacity { get { return items.Length; } }
        public T this[int index] { get { return items[index]; } }
        public FixedPool(int capacity, Func<T> factory,int prewarm = 0)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (prewarm < 0 || prewarm > capacity) throw new ArgumentOutOfRangeException(nameof(prewarm));
            available = prewarm == 0 ? capacity : prewarm;
            items = new T[capacity];
            for (int i = 0; i < capacity; i++) { items[i] = factory();
                if (items[i] == null) throw new ArgumentException("Pool factory returned null.");
                for (int j = 0; j < i; j++) if (ReferenceEquals(items[i],items[j])) throw new ArgumentException("Duplicate pool registration.");
                items[i].IsRented = false; items[i].OnReturn(); }
        }
        public bool TryRent(out T item)
        {
            if (Active == available && available < Capacity) available = Math.Min(Capacity,Math.Max(available+1,available*2));
            for (int i = 0; i < available; i++)
                if (!items[i].IsRented)
                {
                    item = items[i]; item.IsRented = true;
                    Active++;
                    try { item.OnRent(); }
                    catch { item.IsRented = false; Active--; item.OnReturn(); throw; }
                    Peak = Math.Max(Peak, Active); return true;
                }
            Misses++; item = null; return false;
        }
        public bool Return(T item)
        {
            if (item == null || !item.IsRented) return false;
            for (int i = 0; i < items.Length; i++)
                if (ReferenceEquals(items[i], item))
                {
                    item.IsRented = false; Active--; item.OnReturn(); return true;
                }
            return false;
        }
        public void ReturnAll() { for (int i = 0; i < items.Length; i++) Return(items[i]); }
    }
}
