using System;
namespace BetterDesign
{
    public enum AttackKind { Melee, Arrow, Spell }
    [Serializable]
    public sealed class AttackDefinition
    {
        public AttackKind kind;
        public float windup = .18f, duration = .48f, cooldown = .55f, movementScale;
        public bool IsValid { get { return windup >= 0 && duration > windup && duration <= 5 && cooldown > 0 && cooldown <= 10 && movementScale >= 0 && movementScale <= 1; } }
    }
    public sealed class AttackExecutor
    {
        readonly IActorWorld world;
        readonly DemoSettings settings;
        public AttackExecutor(IActorWorld world,DemoSettings settings) { this.world = world; this.settings = settings; }
        public bool Execute(ActorController actor,AttackDefinition definition,bool player)
        {
            int damage = player ? (definition.kind == AttackKind.Spell ? settings.spellDamage : settings.meleeDamage) : settings.enemyDamage;
            if (player && definition.kind == AttackKind.Spell) damage += world.Model.Progression.MagicRank*5;
            if (!player)
            {
                if (world.Hero.IsAlive && (world.Hero.Position-actor.Position).sqrMagnitude < 1.8225f) world.Hero.ReceiveDamage(damage);
                return true;
            }
            if (definition.kind == AttackKind.Melee) { world.Melee(actor,damage); return true; }
            return world.Fire(actor.Position,actor.Facing,damage,definition.kind == AttackKind.Arrow);
        }
    }
}
