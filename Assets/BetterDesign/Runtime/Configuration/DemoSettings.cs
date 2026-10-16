using UnityEngine;

namespace BetterDesign
{
    [CreateAssetMenu(menuName = "BetterDesign/Demo Settings")]
    public sealed class DemoSettings : ScriptableObject
    {
        public AttackDefinition melee = new AttackDefinition { kind = AttackKind.Melee };
        public AttackDefinition bow = new AttackDefinition { kind = AttackKind.Arrow,movementScale = .65f };
        public AttackDefinition magic = new AttackDefinition { kind = AttackKind.Spell,cooldown = 1.2f,movementScale = .65f };
        public AttackDefinition enemy = new AttackDefinition { kind = AttackKind.Melee,windup = .3f,duration = .6f,cooldown = 1.1f };
        public bool IsValid { get { return melee != null && bow != null && magic != null && enemy != null &&
            melee.IsValid && bow.IsValid && magic.IsValid && enemy.IsValid &&
            melee.kind == AttackKind.Melee && bow.kind == AttackKind.Arrow && magic.kind == AttackKind.Spell &&
            moveSpeed > 0 && moveSpeed <= 8 && enemySpeed > 0 && enemySpeed <= 4 && meleeDamage > 0 && spellDamage > 0 && enemyDamage > 0 && baseEnemyHealth > 0 && baseEnemyHealth <= 500; } }

        [Range(1, 8)] public float moveSpeed = 4.2f;
        [Range(1, 4)] public float enemySpeed = 1.4f;
        [Range(1, 100)] public int meleeDamage = 25;
        [Range(1, 100)] public int spellDamage = 35;
        [Range(1, 40)] public int enemyDamage = 10;
        [Range(1, 60)] public int baseEnemyHealth = 50;
    }
}
