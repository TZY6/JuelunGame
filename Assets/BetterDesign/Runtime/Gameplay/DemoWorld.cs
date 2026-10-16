using System;
using BetterDesign.Core;
using UnityEngine;

namespace BetterDesign
{
    // Composition-owned world. Systems share injected references, never global singletons.
    public sealed class DemoWorld : IDisposable
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
        readonly IPlayerInput input;
        readonly Transform root, actors, effects;
        readonly FixedPool<ActorView> enemyPool;
        readonly FixedPool<Effect> effectPool;
        readonly Sprite square;
        readonly Texture2D pixel;
        int enemyCount;
        public int LivingEnemies { get; private set; }
        public int PoolActive { get { return enemyPool.Active + effectPool.Active; } }
        public int PoolPeak { get { return effectPool.Peak; } }
        public int PoolMisses { get { return effectPool.Misses + enemyPool.Misses; } }
        public const int EffectCapacity = 48;
        public bool Paused { get; private set; }
        public Vector2 Beacon { get { return new Vector2(-7, 0); } }

        public DemoWorld(Transform root, DemoSettings settings, ISaveStore saves, IPlayerInput input)
        {
            this.root = root; Settings = settings; Saves = saves; this.input = input;
            Model = new GameModel(Events);
            actors = Group("Actors - Views + injected Controllers");
            effects = Group("Pool - 48 prewarmed effects");
            pixel = new Texture2D(1, 1); pixel.SetPixel(0, 0, Color.white); pixel.Apply();
            square = Sprite.Create(pixel, new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1);
            BuildArena();
            enemyPool = new FixedPool<ActorView>(16, () => CreateActor("Sentinel [pooled]", false));
            effectPool = new FixedPool<Effect>(EffectCapacity, () => new Effect(CreateSprite("Effect [pooled]", effects, square, Color.cyan, Vector2.zero, new Vector2(.2f,.2f), 300)));
            Hero = new ActorController(this, CreateActor("Hero - FSM / IDamageable", true), true, 0, GameModel.MaxHp, Settings);
            Hero.Reset(Model.Hp, new Vector2(-5,0));
            SpawnWave();
        }
        Transform Group(string name) { var go = new GameObject(name); go.transform.SetParent(root, false); return go.transform; }
        SpriteRenderer CreateSprite(string name, Transform parent, Sprite sprite, Color color, Vector2 at, Vector2 scale, int order)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.position = at; go.transform.localScale = scale;
            var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.color = color; renderer.sortingOrder = order;
            return renderer;
        }
        ActorView CreateActor(string name, bool hero)
        {
            var go = new GameObject(name); go.transform.SetParent(actors, false);
            var view = go.AddComponent<ActorView>();
            view.Configure(hero ? Settings.heroSprite : Settings.enemySprite, hero ? Settings.heroAnimator : Settings.enemyAnimator, hero ? Color.white : new Color(1,.65f,.55f));
            return view;
        }
        void BuildArena()
        {
            var terrain = Group("Sanctuary - RPG1 palette");
            CreateSprite("Water border", terrain, square, new Color(.12f,.32f,.40f), Vector2.zero, new Vector2(24,16), -110);
            for (int x = -10; x <= 9; x++)
                for (int y = -6; y <= 5; y++)
                    CreateSprite("Grass", terrain, Settings.groundSprite != null ? Settings.groundSprite : square,
                        Settings.groundSprite != null ? Color.white : new Color(.29f,.44f,.31f), new Vector2(x,y), Vector2.one, -100);
            CreateSprite("Sanctuary path", terrain, square, new Color(.62f,.59f,.40f), new Vector2(-6,0), new Vector2(4,1.5f), -90);
            CreateSprite("Quest beacon", terrain, square, new Color(.2f,.9f,.9f), Beacon, new Vector2(.55f,1.1f), -80);
            CreateSprite("Combat field", terrain, square, new Color(.72f,.76f,.45f,.12f), new Vector2(2,0), new Vector2(12,9), -80);
            for (int x = -10; x <= 10; x++)
            {
                CreateSprite("North boundary", terrain, square, new Color(.16f,.30f,.25f), new Vector2(x,6), new Vector2(.9f,.45f), -70);
                CreateSprite("South boundary", terrain, square, new Color(.16f,.30f,.25f), new Vector2(x,-6), new Vector2(.9f,.45f), -70);
            }
        }
        public Vector2 Clamp(Vector2 p) { return new Vector2(Mathf.Clamp(p.x,-9.5f,9.5f), Mathf.Clamp(p.y,-5.4f,5.4f)); }
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
                float angle = Mathf.PI * 2 * i / count;
                AddEnemy(new Vector2(3 + Mathf.Cos(angle)*4, Mathf.Sin(angle)*3.7f), Mathf.Min(500, Settings.baseEnemyHealth + (Model.Wave-1)*10));
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
            if (Model.UsePotion()) { Hero.SyncHealth(Model.Hp); Events.Notice.Publish("Restored 40 HP."); }
            else Events.Notice.Publish("Cannot use potion: full HP, empty bag or defeated.");
        }
        public void NextWave()
        {
            if (Model.NextWave()) { SpawnWave(); Events.Notice.Publish("New wave. Good luck!"); }
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
            var data = Model.Capture(); data.x = Hero.Position.x; data.y = Hero.Position.y;
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
                for (int i = 0; i < data.enemies.Length; i++) AddEnemy(new Vector2(data.enemies[i].x,data.enemies[i].y),data.enemies[i].hp);
                LivingEnemies = enemyCount;
            }
            Events.Notice.Publish(message);
        }
        public void NewRun()
        {
            enemyPool.ReturnAll(); effectPool.ReturnAll(); Paused = false;
            Model.Restore(new SaveData()); Hero.Reset(Model.Hp,new Vector2(-5,0)); SpawnWave();
            Events.Notice.Publish("New run. Checkpoint kept until you save.");
        }
        public void Dispose()
        {
            enemyPool.ReturnAll(); effectPool.ReturnAll();
            UnityEngine.Object.Destroy(square); UnityEngine.Object.Destroy(pixel);
        }
    }
}
