using System;
using BetterDesign.Core;
using UnityEngine;

namespace BetterDesign
{
    // Composition-owned world. Systems share injected references, never global singletons.
    public sealed class DemoWorld : IDisposable, IInventoryActions
    {
        sealed class Effect : IPoolItem
        {
            public bool IsRented { get; set; }
            public readonly SpriteRenderer Sprite;
            public Vector2 Velocity;
            public float Life;
            public bool Projectile;
            public Effect(SpriteRenderer sprite) { Sprite = sprite; }
            public void OnRent() { Sprite.gameObject.SetActive(true); Life = .25f; Velocity = Vector2.zero; Projectile = false; }
            public void OnReturn() { Sprite.gameObject.SetActive(false); }
        }
        public readonly GameEvents Events = new GameEvents();
        public readonly GameModel Model;
        public ActorController Hero { get; private set; }
        public readonly ActorController[] Enemies = new ActorController[16];
        public readonly DemoSettings Settings;
        public readonly ISaveStore Saves;
        public readonly LootController Loot;
        readonly IPlayerInput input;
        readonly WorldSceneBindings bindings;
        readonly Vector2[] enemySpawns;
        readonly Vector2 heroSpawn;
        readonly FixedPool<ActorView> enemyPool;
        readonly FixedPool<Effect> effectPool;
        int enemyCount;
        public int LivingEnemies { get; private set; }
        public int PoolActive { get { return enemyPool.Active + effectPool.Active; } }
        public int PoolPeak { get { return effectPool.Peak; } }
        public int PoolMisses { get { return effectPool.Misses + enemyPool.Misses; } }
        public const int EffectCapacity = 48;
        public bool Paused { get; private set; }
        public Vector2 Beacon { get { return bindings.beacon.position; } }

        public DemoWorld(WorldSceneBindings bindings, DemoSettings settings, ISaveStore saves, IPlayerInput input)
        {
            if (bindings == null || !bindings.IsConfigured)
                throw new InvalidOperationException("WorldManager scene references are incomplete.");
            this.bindings = bindings; Settings = settings; Saves = saves; this.input = input;
            Model = new GameModel(Events);
            Loot = new LootController(bindings.lootSlots,Model,Events);
            heroSpawn = bindings.hero.transform.position;
            enemySpawns = new Vector2[bindings.enemySlots.Length];
            for (int i = 0; i < enemySpawns.Length; i++)
            {
                enemySpawns[i] = bindings.enemySlots[i].transform.position;
                bindings.enemySlots[i].Initialize();
            }
            bindings.hero.Initialize();
            int actorIndex = 0, effectIndex = 0;
            // Factories register pre-existing scene components; they create no GameObjects.
            enemyPool = new FixedPool<ActorView>(bindings.enemySlots.Length, () => bindings.enemySlots[actorIndex++]);
            effectPool = new FixedPool<Effect>(bindings.effectSlots.Length, () => new Effect(bindings.effectSlots[effectIndex++]));
            Hero = new ActorController(this, bindings.hero, true, 0, GameModel.MaxHp, Settings);
            Hero.Reset(Model.Hp, heroSpawn);
            SpawnWave();
        }
        public Vector2 Clamp(Vector2 p)
        {
            return new Vector2(Mathf.Clamp(p.x,bindings.minimum.x,bindings.maximum.x),
                Mathf.Clamp(p.y,bindings.minimum.y,bindings.maximum.y));
        }
        public void Tick(float dt)
        {
            if (Paused) return;
            Hero.Tick(dt, input);
            LivingEnemies = 0;
            for (int i = 0; i < enemyCount; i++)
            {
                var enemy = Enemies[i];
                if (!enemy.View.IsRented) continue;
                enemy.Tick(dt, null);
                if (enemy.IsAlive) LivingEnemies++;
                else if (enemy.Machine.Elapsed > .7f) enemyPool.Return(enemy.View);
            }
            for (int i = 0; i < effectPool.Capacity; i++)
            {
                var effect = effectPool[i];
                if (!effect.IsRented) continue;
                effect.Life -= dt;
                effect.Sprite.transform.position += (Vector3)(effect.Velocity * dt);
                if (effect.Projectile)
                    for (int j = 0; j < enemyCount; j++)
                        if (Enemies[j].IsAlive && (Enemies[j].Position - (Vector2)effect.Sprite.transform.position).sqrMagnitude < .45f)
                        { Enemies[j].ReceiveDamage(Settings.spellDamage); effect.Life = 0; break; }
                if (effect.Life <= 0) effectPool.Return(effect);
            }
            Loot.Tick(Hero.Position,Hero.IsAlive,dt);
            if (LivingEnemies == 0 && !Model.WaveCleared && Hero.IsAlive)
            {
                Model.ClearWave(); Events.Notice.Publish("Wave cleared! Return to beacon (E), or N for next wave.");
                Save();
            }
        }
        void AddEnemy(Vector2 position, int hp)
        {
            ActorView view;
            if (!enemyPool.TryRent(out view)) return;
            var controller = Enemies[enemyCount];
            if (controller == null || controller.View != view)
                controller = new ActorController(this, view, false, enemyCount + 1, hp, Settings);
            controller.Reset(hp, position);
            Enemies[enemyCount++] = controller;
        }
        public void SpawnWave()
        {
            enemyPool.ReturnAll(); effectPool.ReturnAll(); enemyCount = 0;
            int count = Mathf.Min(6 + (Model.Wave - 1) * 2, 16);
            for (int i = 0; i < count; i++)
            {
                AddEnemy(enemySpawns[i], Mathf.Min(500, Settings.baseEnemyHealth + (Model.Wave-1)*10));
            }
            LivingEnemies = enemyCount;
        }
        public void Melee(ActorController source, int damage)
        {
            ShowHit(source.Position + source.Facing * .8f, new Color(.4f,1,1));
            for (int i = 0; i < enemyCount; i++)
            {
                var enemy = Enemies[i];
                Vector2 delta = enemy.Position - source.Position;
                if (enemy.IsAlive && delta.sqrMagnitude <= 2.7f && Vector2.Dot(delta.normalized, source.Facing) > -.15f)
                    enemy.ReceiveDamage(damage);
            }
        }
        public bool Fire(Vector2 position, Vector2 direction)
        {
            Effect effect;
            if (!effectPool.TryRent(out effect)) return false;
            effect.Projectile = true; effect.Life = 1.5f; effect.Velocity = direction * 9;
            effect.Sprite.transform.position = position + direction * .6f;
            effect.Sprite.transform.localScale = new Vector3(.3f,.3f,1);
            effect.Sprite.color = new Color(.4f,.95f,1);
            return true;
        }
        public void ShowHit(Vector2 position, Color color)
        {
            Effect effect;
            if (!effectPool.TryRent(out effect)) return;
            effect.Sprite.transform.position = position;
            effect.Sprite.transform.localScale = new Vector3(.65f,.65f,1);
            effect.Sprite.color = color;
        }
        public void Interact()
        {
            if (Hero.IsAlive && Vector2.Distance(Hero.Position, Beacon) <= 2.3f) Model.InteractQuest();
            else Events.Notice.Publish("Move near the cyan beacon on the west side, then E.");
        }
        public void Buy()
        {
            if (Hero.IsAlive && Vector2.Distance(Hero.Position, Beacon) <= 2.3f)
                Events.Notice.Publish(Model.BuyPotion() ? "Potion purchased (-10 gold)." : "Need 10 gold / inventory full.");
            else Events.Notice.Publish("Buy potions near the cyan beacon.");
        }
        public void Heal()
        {
            if (Model.UsePotion()) { Events.Notice.Publish("恢复了 40 点生命。"); }
            else Events.Notice.Publish("Cannot use potion: full HP, empty bag or defeated.");
        }
        public void NextWave()
        {
            int nextCount = Mathf.Min(6 + Model.Wave*2,16);
            if (Loot.Free < nextCount*2) { Events.Notice.Publish("请先拾取地面物品，为下一波预留掉落容量。"); return; }
            if (Model.NextWave()) { SpawnWave(); Events.Notice.Publish("下一波开始。"); }
            else Events.Notice.Publish("Clear the current wave first.");
        }
        public void TogglePause()
        {
            Paused = !Paused;
            Hero.View.Animation.speed = Paused ? 0 : 1;
            for (int i = 0; i < enemyCount; i++)
                Enemies[i].View.Animation.speed = Paused ? 0 : 1;
        }
        public void Save()
        {
            if (!Hero.IsAlive) { Events.Notice.Publish("Defeated runs cannot overwrite checkpoints."); return; }
            var data = Model.Capture(); data.loot = Loot.Capture();
            int count = 0;
            for (int i = 0; i < enemyCount; i++) if (Enemies[i].IsAlive) count++;
            data.enemies = new EnemySnapshot[count];
            for (int i = 0, j = 0; i < enemyCount; i++)
                if (Enemies[i].IsAlive)
                    data.enemies[j++] = new EnemySnapshot { x = Enemies[i].Position.x, y = Enemies[i].Position.y, hp = Enemies[i].Health };
            string message; Saves.TrySave(data, out message); Events.Notice.Publish(message);
        }
        public void Load()
        {
            SaveData data; string message;
            if (Saves.TryLoad(out data, out message))
            {
                enemyPool.ReturnAll(); effectPool.ReturnAll(); enemyCount = 0; Paused = false;
                Model.Restore(data); Hero.Reset(Model.Hp, new Vector2(data.x,data.y));
                Loot.Restore(data.version == 1 ? null : data.loot);
                for (int i = 0; i < data.enemies.Length; i++) AddEnemy(new Vector2(data.enemies[i].x,data.enemies[i].y),data.enemies[i].hp);
                LivingEnemies = enemyCount;
            }
            Events.Notice.Publish(message);
        }
        public void NewRun()
        {
            enemyPool.ReturnAll(); effectPool.ReturnAll(); Paused = false;
            Loot.Clear(); Model.Restore(new SaveData()); Hero.Reset(Model.Hp,heroSpawn); SpawnWave();
            Events.Notice.Publish("New run. Checkpoint kept until you save.");
        }
        public void EnemyDefeated(ActorController actor,int id)
        {
            Vector2 p = Clamp(actor.Position + new Vector2(-.30f,-.20f));
            Vector2 q = Clamp(actor.Position + new Vector2(.30f,-.20f));
            // Capacity is reserved before waves and player drops: no unbounded allocation or lost rewards.
            bool gold = Loot.TrySpawn(new LootSnapshot { item = ItemId.Gold,amount = 5,x = p.x,y = p.y });
            bool item = Loot.TrySpawn(new LootSnapshot { item = id % 3 == 0 ? ItemId.Supply : ItemId.Mushroom,amount = 1,x = q.x,y = q.y });
            if (!gold || !item) throw new InvalidOperationException("Reserved loot capacity was violated.");
            Model.EnemyKilled();
        }
        public bool UseSlot(int index)
        {
            bool used = Model.UseItem(index);
            Events.Notice.Publish(used ? "使用治疗补给，恢复 40 点生命。" : "无法使用：请确认物品类型、生命值与存活状态。");
            return used;
        }
        public bool DropSlot(int index)
        {
            if (index < 0 || index >= InventoryModel.Capacity || !Hero.IsAlive) return false;
            var item = Model.Inventory[index];
            if (item.amount == 0) return false;
            // Preserve enough free slots for every currently living enemy's two drops.
            int alive = 0; for (int i = 0; i < enemyCount; i++) if (Enemies[i].IsAlive) alive++;
            if (Loot.Free <= alive*2) { Events.Notice.Publish("地面掉落槽位不足，请先拾取其他物品。"); return false; }
            Vector2 position = Clamp(Hero.Position + Hero.Facing*1.25f);
            if (!Loot.TrySpawn(new LootSnapshot { item = item.item,amount = 1,x = position.x,y = position.y })) return false;
            Model.RemoveItem(index,1); Events.Notice.Publish("丢弃 1 个物品；可在地面重新拾取。"); return true;
        }
        public void SellMaterials() { if (Hero.IsAlive) Model.SellMaterials(); }
        public void Dispose()
        {
            Loot.Clear(); enemyPool.ReturnAll(); effectPool.ReturnAll();
        }
    }
}
