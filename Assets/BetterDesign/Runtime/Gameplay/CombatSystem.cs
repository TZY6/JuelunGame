using BetterDesign.Core;
using UnityEngine;
namespace BetterDesign
{
    public sealed class CombatSystem
    {
        sealed class Effect : IPoolItem
        {
            public bool IsRented { get; set; }
            public readonly SpriteRenderer Sprite;
            public Vector2 Velocity;
            public float Life;
            public bool Projectile;
            public int Damage;
            readonly Vector3 scale;
            readonly Color color;
            public Effect(SpriteRenderer sprite) { Sprite = sprite; scale = sprite.transform.localScale; color = sprite.color; }
            public void OnRent() { Sprite.gameObject.SetActive(true); Life = .25f; Velocity = Vector2.zero; Projectile = false; Damage = 0; }
            public void OnReturn() { Life = 0; Velocity = Vector2.zero; Projectile = false; Damage = 0; Sprite.color = color; Sprite.transform.localScale = scale; Sprite.gameObject.SetActive(false); }
        }

        readonly FixedPool<Effect> effectPool;
        readonly ActorController[] enemies;
        int count;
        public int Active { get { return effectPool.Active; } }
        public int Peak { get { return effectPool.Peak; } }
        public int Misses { get { return effectPool.Misses; } }
        public CombatSystem(SpriteRenderer[] slots,ActorController[] enemies)
        {
            this.enemies = enemies; int index = 0;
            effectPool = new FixedPool<Effect>(slots.Length,() => new Effect(slots[index++]),12);
        }
        public void Tick(float dt,int count)
        {
            this.count = count;
            for (int i = 0; i < effectPool.Capacity; i++)
            {
                var effect = effectPool[i];
                if (!effect.IsRented) continue;
                effect.Life -= dt;
                effect.Sprite.transform.position += (Vector3)(effect.Velocity * dt);
                if (effect.Projectile)
                    for (int j = 0; j < count; j++)
                        if (enemies[j].IsAlive && (enemies[j].Position - (Vector2)effect.Sprite.transform.position).sqrMagnitude < .45f)
                        { enemies[j].ReceiveDamage(effect.Damage); effect.Life = 0; break; }
                if (effect.Life <= 0) effectPool.Return(effect);
            }

        }
        public void SetTargets(int count) { this.count = count; }
        public void Melee(ActorController source, int damage)
        {
            ShowHit(source.Position + source.Facing * .8f, new Color(.4f,1,1));
            for (int i = 0; i < count; i++)
            {
                var enemy = enemies[i];
                Vector2 delta = enemy.Position - source.Position;
                if (enemy.IsAlive && delta.sqrMagnitude <= 2.7f && Vector2.Dot(delta.normalized, source.Facing) > -.15f)
                    enemy.ReceiveDamage(damage);
            }
        }
        public bool Fire(Vector2 position,Vector2 direction,int damage,bool arrow)
        {
            Effect effect;
            if (!effectPool.TryRent(out effect)) return false;
            effect.Projectile = true; effect.Damage = damage; effect.Life = 1.5f; effect.Velocity = direction * 9;
            effect.Sprite.transform.position = position + direction * .6f;
            effect.Sprite.transform.localScale = new Vector3(.3f,.3f,1);
            effect.Sprite.color = arrow ? new Color(1,.85f,.4f) : new Color(.4f,.95f,1);
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

        public void Clear() { effectPool.ReturnAll(); }
    }
}
