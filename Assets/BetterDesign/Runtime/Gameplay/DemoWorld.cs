using System;
using BetterDesign.Core;
using UnityEngine;

namespace BetterDesign
{
    // Composition-owned world. Systems share injected references, never global singletons.
    public sealed class DemoWorld : IDisposable, IInventoryActions, IActorWorld
    {
        public GameEvents Events { get; } = new GameEvents(error => Debug.LogException(error));
        public GameModel Model { get; }
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
        readonly CombatSystem combat;
        readonly CheckpointCoordinator checkpoints;
        int enemyCount;
        public int LivingEnemies { get; private set; }
        public int PoolActive { get { return enemyPool.Active + combat.Active; } }
        public int PoolPeak { get { return combat.Peak; } }
        public int PoolMisses { get { return combat.Misses + enemyPool.Misses; } }
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
            checkpoints = new CheckpointCoordinator(Model,Loot,Enemies,saves);
            heroSpawn = bindings.hero.transform.position;
            enemySpawns = new Vector2[bindings.enemySlots.Length];
            for (int i = 0; i < enemySpawns.Length; i++)
            {
                enemySpawns[i] = bindings.enemySlots[i].transform.position;
                bindings.enemySlots[i].Initialize();
            }
            bindings.hero.Initialize();
            int actorIndex = 0;
            // Factories register pre-existing scene components; they create no GameObjects.
            enemyPool = new FixedPool<ActorView>(bindings.enemySlots.Length, () => bindings.enemySlots[actorIndex++]);
            combat = new CombatSystem(bindings.effectSlots,Enemies);
            Hero = new ActorController(this, bindings.hero, true, 0, GameModel.MaxHp, Settings);
            Hero.Reset(Model.Hp, heroSpawn);
            SpawnWave();
        }
        public Vector2 Clamp(Vector2 p)
        {
            return new Vector2(Mathf.Clamp(p.x,bindings.minimum.x,bindings.maximum.x),
                Mathf.Clamp(p.y,bindings.minimum.y,bindings.maximum.y));
        }
        public Vector2 EnemyMovement(ActorController source)
        {
            if (!Hero.IsAlive) return Vector2.zero;
            Vector2 delta = Hero.Position-source.Position;
            Vector2 velocity = delta.sqrMagnitude > 1.05f*1.05f ? delta.normalized : Vector2.zero;
            for (int i = 0; i < enemyCount; i++)
            {
                var other = Enemies[i];
                if (other == source || !other.IsAlive) continue;
                Vector2 away = source.Position-other.Position;
                float distance = away.magnitude;
                if (distance >= .85f) continue;
                if (distance < .001f) away = i%2 == 0 ? Vector2.up : Vector2.down;
                else away /= distance;
                velocity += away*(1-distance/.85f)*1.6f;
            }
            return Vector2.ClampMagnitude(velocity,1)*Settings.enemySpeed;
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
            combat.Tick(dt,enemyCount);
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
            enemyPool.ReturnAll(); combat.Clear(); enemyCount = 0;
            int count = Mathf.Min(6 + (Model.Wave - 1) * 2, 16);
            for (int i = 0; i < count; i++)
            {
                AddEnemy(enemySpawns[i], Mathf.Min(500, Settings.baseEnemyHealth + (Model.Wave-1)*10));
            }
            LivingEnemies = enemyCount;
        }
        public void Melee(ActorController source,int damage) { combat.SetTargets(enemyCount); combat.Melee(source,damage); }
        public bool Fire(Vector2 position,Vector2 direction,int damage,bool arrow) { return combat.Fire(position,direction,damage,arrow); }
        public void ShowHit(Vector2 position,Color color) { combat.ShowHit(position,color); }
        public void Interact()
        {
            if (Hero.IsAlive && Vector2.Distance(Hero.Position, Beacon) <= 2.3f && Hero.TryInteract())
            {
                Model.Facts.Record(FactKind.LocationVisited,"sanctuary.beacon");
                Model.Facts.Record(FactKind.DialogueCompleted,"sanctuary.contract");
                Model.InteractQuest();
            }
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
        public void Save() { string message; checkpoints.Save(enemyCount,out message); Events.Notice.Publish(message); }
        public void Load()
        {
            SaveData data; string message;
            if (checkpoints.TryLoad(out data, out message))
            {
                enemyPool.ReturnAll(); combat.Clear(); enemyCount = 0; Paused = false;
                Model.Restore(data,false); Hero.Reset(Model.Hp, new Vector2(data.x,data.y));
                Loot.Restore(data.version == 1 ? null : data.loot);
                for (int i = 0; i < data.enemies.Length; i++) AddEnemy(new Vector2(data.enemies[i].x,data.enemies[i].y),data.enemies[i].hp);
                LivingEnemies = enemyCount; Model.NotifyRestored();
            }
            Events.Notice.Publish(message);
        }
        public void NewRun()
        {
            enemyPool.ReturnAll(); combat.Clear(); Paused = false;
            Loot.Clear(); Model.Restore(new SaveData(),false); Hero.Reset(Model.Hp,heroSpawn); SpawnWave(); Model.NotifyRestored();
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
            Loot.Clear(); enemyPool.ReturnAll(); combat.Clear();
        }
    }
}
