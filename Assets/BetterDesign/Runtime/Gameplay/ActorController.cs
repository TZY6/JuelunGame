using BetterDesign.Core;
using UnityEngine;

namespace BetterDesign
{
    // Plain C# controller; scene scope supplies dependencies explicitly.
    public sealed class ActorController : IDamageable
    {
        readonly IActorWorld world;
        readonly bool hero;
        readonly int id;
        readonly DemoSettings settings;
        public readonly ActorModel Model;
        public ActorStateMachine Machine { get { return Model.Machine; } }
        public readonly ActorView View;
        public int Health { get { return Model.Health; } }
        public bool IsAlive { get { return Health > 0; } }
        public Vector2 Position { get { return new Vector2(Model.X,Model.Y); } }
        public Vector2 Facing { get { return new Vector2(Model.FacingX,Model.FacingY); } }
        float invulnerable, cooldown, spellCooldown, dashCooldown;
        bool resolved;
        float attackBuffer,spellBuffer,dashBuffer;
        Vector2 knockback;
        public int MaximumHealth { get { return hero ? GameModel.MaxHp : Mathf.Min(500,settings.baseEnemyHealth+(world.Model.Wave-1)*10); } }
        AttackDefinition attack;
        readonly AttackExecutor attacks;
        public bool TryInteract() { return IsAlive && Machine.TryEnter(ActorState.Interact); }
        public ActorController(IActorWorld world, ActorView view, bool hero, int id, int health, DemoSettings settings)
        {
            this.world = world; attacks = new AttackExecutor(world,settings); View = view; this.hero = hero; this.id = id; this.settings = settings;
            Model = hero ? world.Model.Player : new ActorModel();
        }
        public void Reset(int health, Vector2 position)
        {
            Model.Reset(health,position.x,position.y);
            invulnerable = cooldown = spellCooldown = dashCooldown = 0; resolved = false; attack = null;
            attackBuffer = spellBuffer = dashBuffer = 0; knockback = Vector2.zero;
            View.OnRent(); if (hero) View.SetWeapon(world.Model.Progression.Weapon == 1); View.Render(Model,Vector2.zero,0,MaximumHealth);
        }
        public void Tick(float dt, IPlayerInput input)
        {
            if (hero) View.SetWeapon(world.Model.Progression.Weapon == 1);
            Machine.Tick(dt); invulnerable -= dt; cooldown -= dt; spellCooldown -= dt; dashCooldown -= dt;
            Vector2 movement = Vector2.zero;
            if (!IsAlive) { View.Render(Model,movement,dt,MaximumHealth); return; }
            if (hero)
            {
                attackBuffer = input.Attack ? .18f : attackBuffer-dt;
                spellBuffer = input.Spell ? .18f : spellBuffer-dt;
                dashBuffer = input.Dash ? .18f : dashBuffer-dt;
                var aimed = input as IAimedPlayerInput;
                if (aimed != null && aimed.HasAim)
                {
                    Vector2 direction = aimed.AimPoint-Position;
                    if (direction.sqrMagnitude > .04f) Model.Face(direction.normalized.x,direction.normalized.y);
                }
                else if (input.Move.sqrMagnitude > .01f && !Machine.Locked) Model.Face(input.Move.normalized.x,input.Move.normalized.y);
                if (dashBuffer > 0 && dashCooldown <= 0 && Machine.TryEnter(ActorState.Dash))
                {
                    dashBuffer = 0; dashCooldown = .9f; invulnerable = Mathf.Max(invulnerable,.22f);
                    if (input.Move.sqrMagnitude > .01f) Model.Face(input.Move.normalized.x,input.Move.normalized.y);
                }
            }
            if (Machine.State == ActorState.Attack || Machine.State == ActorState.Skill)
            {
                if (attack == null) { Machine.Complete(); }
                else
                {
                    if (!resolved && Machine.Elapsed >= attack.windup)
                    {
                        resolved = true;
                        if (!attacks.Execute(this,attack,hero) && hero) world.Events.Notice.Publish("投射物池已满，本次攻击取消。");
                    }
                    if (hero) movement = input.Move * settings.moveSpeed * attack.movementScale;
                    if (Machine.Elapsed >= attack.duration) Machine.Complete();
                }
            }
            else if (Machine.State == ActorState.Interact)
            {
                if (Machine.Elapsed >= .15f) Machine.Complete();
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
                Machine.SetLocomotion(movement.sqrMagnitude > .01f);
                bool wantsAttack = hero ? attackBuffer > 0 : Vector2.Distance(Position, world.Hero.Position) < 1.1f && world.Hero.IsAlive;
                if (hero && spellBuffer > 0 && spellCooldown <= 0 && Machine.TryEnter(ActorState.Skill))
                {
                    attack = settings.magic; spellCooldown = attack.cooldown; resolved = false; spellBuffer = 0; movement *= attack.movementScale;
                }
                else if (wantsAttack && cooldown <= 0 && Machine.TryEnter(ActorState.Attack,hero && world.Model.Progression.Weapon == 1))
                {
                    attack = hero ? (world.Model.Progression.Weapon == 1 ? settings.bow : settings.melee) : settings.enemy;
                    cooldown = attack.cooldown; resolved = false; attackBuffer = 0; movement *= attack.movementScale;
                }

            }
            Machine.SetLocomotion(movement.sqrMagnitude > .01f);
            Vector2 next = world.Clamp(Position + (movement+knockback) * dt);
            knockback = Vector2.MoveTowards(knockback,Vector2.zero,18*dt);
            Model.MoveTo(next.x,next.y);
            View.Render(Model,movement,dt,MaximumHealth);
        }
        Vector2 EnemyMovement()
        {
            if (!world.Hero.IsAlive) return Vector2.zero;
            Vector2 delta = world.Hero.Position - Position;
            if (delta.sqrMagnitude > .01f) { Vector2 direction = delta.normalized; Model.Face(direction.x,direction.y); }
            return world.EnemyMovement(this);
        }
        public void ReceiveDamage(int amount)
        {
            if (!IsAlive || invulnerable > 0 || amount <= 0) return;
            amount = DamageCalculator.Calculate(new DamageContext(amount));
            amount = Mathf.Min(Health,amount);
            if (hero) world.Model.Damage(amount); else Model.Damage(amount);
            invulnerable = hero ? .65f : .12f;
            Machine.TryEnter(IsAlive ? ActorState.Hurt : ActorState.Dead);
            attackBuffer = spellBuffer = 0;
            if (!hero) knockback = Facing * -2.5f;
            View.HitFlash();
            world.ShowHit(Position, !IsAlive ? Color.yellow : new Color(1,.4f,.2f));
            if (!IsAlive && !hero) world.EnemyDefeated(this,id);
            world.Events.Combat.Publish(new CombatEvent(id, amount, !IsAlive));
            if (!IsAlive && hero) world.Events.Notice.Publish("You fell. Load checkpoint (F9) or start a new run.");
        }
    }
}
