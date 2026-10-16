using System;
using System.Collections.Generic;

namespace BetterDesign.Core
{
    // Scene-owned typed channels: subscriptions are explicit lifetime resources.
    public sealed class Signal<T>
    {
        sealed class Slot : IDisposable
        {
            public Action<T> Handler;
            public void Dispose() { Handler = null; }
        }
        readonly List<Slot> slots = new List<Slot>(16);
        int depth;
        public int Published { get; private set; }
        public int ListenerCount
        {
            get { int n = 0; for (int i = 0; i < slots.Count; i++) if (slots[i].Handler != null) n++; return n; }
        }
        public IDisposable Subscribe(Action<T> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            var slot = new Slot { Handler = handler };
            slots.Add(slot);
            return slot;
        }
        public void Publish(T value)
        {
            Published++;
            depth++;
            int count = slots.Count; // New listeners start with the next publication.
            try
            {
                for (int i = 0; i < count; i++)
                {
                    var handler = slots[i].Handler;
                    if (handler != null) handler(value);
                }
            }
            finally
            {
                if (--depth == 0)
                    for (int i = slots.Count - 1; i >= 0; i--)
                        if (slots[i].Handler == null) slots.RemoveAt(i);
            }
        }
    }

    public enum ChangeKind { Stats, Inventory, Quest, Wave, Restored }
    public struct ModelChanged
    {
        public ChangeKind Kind;
        public ModelChanged(ChangeKind kind) { Kind = kind; }
    }
    public struct CombatEvent
    {
        public int ActorId, Damage;
        public bool Killed;
        public CombatEvent(int id, int damage, bool killed) { ActorId = id; Damage = damage; Killed = killed; }
    }
    public sealed class GameEvents
    {
        public readonly Signal<ModelChanged> Model = new Signal<ModelChanged>();
        public readonly Signal<CombatEvent> Combat = new Signal<CombatEvent>();
        public readonly Signal<string> Notice = new Signal<string>();
    }
}
