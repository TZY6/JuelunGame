using UnityEngine;

namespace BetterDesign
{
    // The WorldManager holds references to authored scene objects, never prefab recipes.
    [DisallowMultipleComponent]
    public sealed class WorldSceneBindings : MonoBehaviour
    {
        public ActorView hero;
        public ActorView[] enemySlots = new ActorView[16];
        public SpriteRenderer[] effectSlots = new SpriteRenderer[48];
        public Transform beacon;
        public Transform mapRoot;
        public Vector2 minimum = new Vector2(-9.5f,-5.4f);
        public Vector2 maximum = new Vector2(9.5f,5.4f);
        public bool IsConfigured
        {
            get
            {
                if (hero == null || !hero.HasSceneReferences || beacon == null || mapRoot == null ||
                    enemySlots == null || enemySlots.Length != 16 || effectSlots == null || effectSlots.Length != 48) return false;
                for (int i = 0; i < enemySlots.Length; i++)
                {
                    if (enemySlots[i] == null || enemySlots[i] == hero || !enemySlots[i].HasSceneReferences) return false;
                    for (int j = 0; j < i; j++) if (enemySlots[i] == enemySlots[j]) return false;
                }
                for (int i = 0; i < effectSlots.Length; i++)
                {
                    if (effectSlots[i] == null || effectSlots[i].sprite == null) return false;
                    for (int j = 0; j < i; j++) if (effectSlots[i] == effectSlots[j]) return false;
                }
                return minimum.x < maximum.x && minimum.y < maximum.y;
            }
        }
    }
}
