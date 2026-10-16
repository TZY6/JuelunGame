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
        public int Active { get; private set; }
        public int Peak { get; private set; }
        public int Misses { get; private set; }
        public int Capacity { get { return items.Length; } }
        public T this[int index] { get { return items[index]; } }
        public FixedPool(int capacity, Func<T> factory)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            items = new T[capacity];
            for (int i = 0; i < capacity; i++) { items[i] = factory(); items[i].IsRented = false; items[i].OnReturn(); }
        }
        public bool TryRent(out T item)
        {
            for (int i = 0; i < items.Length; i++)
                if (!items[i].IsRented)
                {
                    item = items[i]; item.IsRented = true;
                    Active++; Peak = Math.Max(Peak, Active); item.OnRent(); return true;
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
