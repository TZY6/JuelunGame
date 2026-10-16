using System;
using BetterDesign.Core;
using UnityEngine;

namespace BetterDesign
{
    public sealed class LootController
    {
        sealed class Entry : IPoolItem
        {
            public bool IsRented { get; set; }
            public readonly LootView View;
            public LootSnapshot Data;
            public float Age;
            public Entry(LootView view) { View = view; }
            public void OnRent() { Age = 0; }
            public void OnReturn() { View.Hide(); }
        }
        readonly FixedPool<Entry> pool;
        readonly GameModel model;
        readonly GameEvents events;
        float fullNotice;
        public int Active { get { return pool.Active; } }
        public int Free { get { return pool.Capacity - pool.Active; } }
        public LootController(LootView[] views,GameModel model,GameEvents events)
        {
            this.model = model; this.events = events;
            int index = 0; pool = new FixedPool<Entry>(views.Length,() => new Entry(views[index++]));
        }
        public bool TrySpawn(LootSnapshot data)
        {
            Entry entry;
            if (!pool.TryRent(out entry)) return false;
            entry.Data = data; entry.View.Show(data.item); entry.View.Render(data.x,data.y,0);
            return true;
        }
        public void Tick(Vector2 player,bool alive,float dt)
        {
            fullNotice -= dt;
            for (int i = 0; i < pool.Capacity; i++)
            {
                var entry = pool[i];
                if (!entry.IsRented) continue;
                entry.Age += dt;
                entry.View.Render(entry.Data.x,entry.Data.y,Mathf.Sin(entry.Age*4)*.07f);
                float dx = player.x - entry.Data.x,dy = player.y - entry.Data.y;
                if (!alive || entry.Age < .5f || dx*dx + dy*dy > .64f) continue;
                if (model.CanCollect(entry.Data.item,entry.Data.amount))
                {
                    var collected = entry.Data;
                    // Remove the ground entry before publishing model events, so a listener saving
                    // immediately observes inventory credit and ground removal as one transaction.
                    pool.Return(entry);
                    if (!model.Collect(collected.item,collected.amount)) throw new InvalidOperationException("Validated pickup failed.");
                    events.Notice.Publish("拾取：" + ItemCatalog.Name(collected.item) + " ×" + collected.amount);
                }
                else if (fullNotice <= 0)
                {
                    fullNotice = 2; events.Notice.Publish("背包已满，物品保留在地面。可打开背包出售材料或丢弃物品。");
                }
            }
        }
        public LootSnapshot[] Capture()
        {
            var data = new LootSnapshot[pool.Active];
            for (int i = 0,j = 0; i < pool.Capacity; i++) if (pool[i].IsRented) data[j++] = pool[i].Data;
            return data;
        }
        public void Restore(LootSnapshot[] data)
        {
            pool.ReturnAll();
            if (data != null)
                for (int i = 0; i < data.Length; i++)
                    if (!TrySpawn(data[i])) throw new InvalidOperationException("Loot snapshot exceeds scene pool.");
        }
        public void Clear() { pool.ReturnAll(); }
    }
}
