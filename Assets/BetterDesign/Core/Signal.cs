using System;
using System.Collections.Generic;
namespace BetterDesign.Core
{
    public sealed class Signal<T>
    {
        sealed class Slot : IDisposable
        {
            public Signal<T> Owner;
            public Action<T> Handler;
            public void Dispose()
            {
                if (Owner == null) return;
                var owner = Owner; Owner = null; Handler = null;
                if (owner.depth == 0) owner.slots.Remove(this);
            }
        }
        readonly List<Slot> slots = new List<Slot>(16);
        readonly Action<Exception> report;
        int depth;
        public int Published { get; private set; }
        public int ListenerCount { get { int n = 0; for (int i = 0; i < slots.Count; i++) if (slots[i].Handler != null) n++; return n; } }
        public Signal(Action<Exception> report = null) { this.report = report; }
        public IDisposable Subscribe(Action<T> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            var slot = new Slot { Owner = this, Handler = handler }; slots.Add(slot); return slot;
        }
        public void Publish(T value)
        {
            Published++; depth++; int count = slots.Count;
            Exception first = null;
            try
            {
                for (int i = 0; i < count; i++)
                {
                    var handler = slots[i].Handler;
                    if (handler == null) continue;
                    try { handler(value); }
                    catch (Exception error)
                    {
                        if (first == null) first = error;
                        if (report != null) report(error);
                    }
                }
            }
            finally
            {
                if (--depth == 0)
                    for (int i = slots.Count - 1; i >= 0; i--)
                        if (slots[i].Handler == null) slots.RemoveAt(i);
            }
            if (first != null && report == null) throw first;
        }
    }
    public enum ChangeKind { Stats, Inventory, Quest, Wave, Restored }
    public struct ModelChanged { public ChangeKind Kind; public ModelChanged(ChangeKind kind) { Kind = kind; } }
    public struct CombatEvent
    {
        public int ActorId, Damage; public bool Killed;
        public CombatEvent(int id,int damage,bool killed) { ActorId = id; Damage = damage; Killed = killed; }
    }
    public struct EnemyKilled { public int Total; public EnemyKilled(int total) { Total = total; } }
    public struct ItemCollected { public ItemId Item; public int Amount; public ItemCollected(ItemId item,int amount) { Item = item; Amount = amount; } }
    public struct GoldChanged { public int Value; public GoldChanged(int value) { Value = value; } }
    public struct InventoryChanged { public int Revision; public InventoryChanged(int revision) { Revision = revision; } }
    public struct PlayerLevelUp { public int Level; public PlayerLevelUp(int level) { Level = level; } }
    public struct QuestProgressChanged { public int Current; public QuestProgressChanged(int current) { Current = current; } }
    public sealed class GameEvents
    {
        public readonly Signal<ModelChanged> Model;
        public readonly Signal<CombatEvent> Combat;
        public readonly Signal<string> Notice;
        public readonly Signal<EnemyKilled> EnemyKilled;
        public readonly Signal<ItemCollected> ItemCollected;
        public readonly Signal<GoldChanged> GoldChanged;
        public readonly Signal<InventoryChanged> InventoryChanged;
        public readonly Signal<PlayerLevelUp> PlayerLevelUp;
        public readonly Signal<QuestProgressChanged> QuestProgressChanged;
        public GameEvents(Action<Exception> report = null)
        {
            Model = new Signal<ModelChanged>(report); Combat = new Signal<CombatEvent>(report);
            Notice = new Signal<string>(report); EnemyKilled = new Signal<EnemyKilled>(report);
            ItemCollected = new Signal<ItemCollected>(report); GoldChanged = new Signal<GoldChanged>(report);
            InventoryChanged = new Signal<InventoryChanged>(report); PlayerLevelUp = new Signal<PlayerLevelUp>(report);
            QuestProgressChanged = new Signal<QuestProgressChanged>(report);
        }
    }
}
