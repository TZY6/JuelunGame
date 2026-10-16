using BetterDesign.Core;
using UnityEngine;

namespace BetterDesign
{
    public interface IPlayerInput
    {
        Vector2 Move { get; }
        bool Attack { get; }
        bool Dash { get; }
        bool Spell { get; }
    }
    public sealed class KeyboardInput : IPlayerInput
    {
        public Vector2 Move { get { return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized; } }
        public bool Attack { get { return Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0); } }
        public bool Dash { get { return Input.GetKeyDown(KeyCode.LeftShift); } }
        public bool Spell { get { return Input.GetKeyDown(KeyCode.Q) || Input.GetMouseButtonDown(1); } }
    }
    // Plain C# controller; scene scope supplies dependencies explicitly.
    public sealed class ActorController : IDamageable
    {
        readonly DemoWorld world;
        readonly bool hero;
        readonly int id;
        readonly DemoSettings settings;
        public readonly ActorStateMachine Machine = new ActorStateMachine();
        public readonly ActorView View;
        public int Health { get; private set; }
        public bool IsAlive { get { return Health > 0; } }
        public Vector2 Position { get { return View.transform.position; } }
        public Vector2 Facing { get; private set; } = Vector2.right;
        float invulnerable, cooldown, spellCooldown, dashCooldown;
        bool resolved;
        public ActorController(DemoWorld world, ActorView view, bool hero, int id, int health, DemoSettings settings)
        {
            this.world = world; View = view; this.hero = hero; this.id = id; Health = health; this.settings = settings;
        }
        public void Reset(int health, Vector2 position)
        {
            Health = health; View.transform.position = position; Machine.Reset();
            invulnerable = cooldown = spellCooldown = dashCooldown = 0; resolved = false;
            Facing = Vector2.right; View.OnRent();
        }
        public void Tick(float dt, IPlayerInput input)
        {
            Machine.Tick(dt); invulnerable -= dt; cooldown -= dt; spellCooldown -= dt; dashCooldown -= dt;
            Vector2 movement = Vector2.zero;
            if (!IsAlive) { View.Present(ActorState.Dead, movement, dt); return; }
            if (Machine.State == ActorState.Attack)
            {
                if (!resolved && Machine.Elapsed >= (hero ? .18f : .30f))
                {
                    resolved = true;
                    if (hero) world.Melee(this, settings.meleeDamage);
                    else if (world.Hero.IsAlive && Vector2.Distance(Position, world.Hero.Position) < 1.35f)
                        world.Hero.ReceiveDamage(settings.enemyDamage);
                }
                if (Machine.Elapsed >= (hero ? .48f : .60f)) Machine.Complete();
            }
            else if (Machine.State == ActorState.Hurt)
            {
                if (Machine.Elapsed > .18f) Machine.Complete();
            }
            else if (Machine.State == ActorState.Dash)
            {
                movement = Facing * settings.moveSpeed * 2.8f;
                if (Machine.Elapsed >= .16f) Machine.Complete();
            }
            else
            {
                movement = hero ? input.Move * settings.moveSpeed : EnemyMovement();
                if (movement.sqrMagnitude > .01f) Facing = movement.normalized;
                Machine.TryEnter(movement.sqrMagnitude > .01f ? ActorState.Move : ActorState.Idle);
                bool attack = hero ? input.Attack : Vector2.Distance(Position, world.Hero.Position) < 1.1f && world.Hero.IsAlive;
                if (attack && cooldown <= 0 && Machine.TryEnter(ActorState.Attack))
                {
                    cooldown = hero ? .55f : 1.1f; resolved = false; movement = Vector2.zero;
                }
                else if (hero && input.Dash && dashCooldown <= 0 && Machine.TryEnter(ActorState.Dash))
                {
                    dashCooldown = .9f; invulnerable = .22f;
                }
                if (hero && input.Spell && spellCooldown <= 0)
                {
                    if (world.Fire(Position, Facing)) spellCooldown = settings.spellCooldown;
                }
            }
            Vector2 next = world.Clamp(Position + movement * dt);
            View.transform.position = next;
            View.Present(Machine.State, movement, dt);
        }
        Vector2 EnemyMovement()
        {
            if (!world.Hero.IsAlive) return Vector2.zero;
            Vector2 delta = world.Hero.Position - Position;
            return delta.sqrMagnitude > 1 ? delta.normalized * settings.enemySpeed : Vector2.zero;
        }
        public void ReceiveDamage(int amount)
        {
            if (!IsAlive || invulnerable > 0 || amount <= 0) return;
            Health = Mathf.Max(0, Health - amount);
            if (hero) world.Model.Damage(amount);
            invulnerable = hero ? .65f : .12f;
            Machine.TryEnter(IsAlive ? ActorState.Hurt : ActorState.Dead);
            View.HitFlash();
            world.Events.Combat.Publish(new CombatEvent(id, amount, !IsAlive));
            world.ShowHit(Position, !IsAlive ? Color.yellow : new Color(1,.4f,.2f));
            if (!IsAlive && !hero) world.Model.EnemyKilled();
            if (!IsAlive && hero) world.Events.Notice.Publish("You fell. Load checkpoint (F9) or start a new run.");
        }
        public void SyncHealth(int health) { Health = health; }
    }
}
