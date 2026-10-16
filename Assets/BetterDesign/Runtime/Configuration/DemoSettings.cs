using UnityEngine;

namespace BetterDesign
{
    [CreateAssetMenu(menuName = "BetterDesign/Demo Settings")]
    public sealed class DemoSettings : ScriptableObject
    {
        [Range(1, 8)] public float moveSpeed = 4.2f;
        [Range(1, 4)] public float enemySpeed = 1.4f;
        [Range(1, 100)] public int meleeDamage = 25;
        [Range(1, 100)] public int spellDamage = 35;
        [Range(1, 40)] public int enemyDamage = 10;
        [Range(1, 60)] public int baseEnemyHealth = 50;
        [Range(1, 10)] public float spellCooldown = 1.2f;
        public RuntimeAnimatorController heroAnimator;
        public RuntimeAnimatorController enemyAnimator;
        public Sprite heroSprite, enemySprite, groundSprite;
    }
}
