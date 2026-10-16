using BetterDesign.Core;
using UnityEngine;

namespace BetterDesign
{
    public interface IDamageable { bool IsAlive { get; } void ReceiveDamage(int amount); }
    // Rendering adapter only. No inventory, persistence or input logic.
    public sealed class ActorView : MonoBehaviour, IPoolItem
    {
        public bool IsRented { get; set; }
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Animator animator;
        public SpriteRenderer Renderer { get { return spriteRenderer; } }
        public Animator Animation { get { return animator; } }
        static readonly int Speed = UnityEngine.Animator.StringToHash("Speed");
        static readonly int Locomotion = UnityEngine.Animator.StringToHash("Base Layer.Locomotion");
        static readonly int Attack = UnityEngine.Animator.StringToHash("Base Layer.Attack");
        ActorState lastState;
        float flash;
        Color normalColor;
        public bool HasSceneReferences { get { return spriteRenderer != null && animator != null; } }
        public void Initialize()
        {
            if (!HasSceneReferences) throw new System.InvalidOperationException(name + ": actor scene references missing.");
            normalColor = Renderer.color;
            lastState = ActorState.Idle;
        }
        public void Present(ActorState state, Vector2 movement, float dt)
        {
            if (movement.x != 0) Renderer.flipX = movement.x < 0;
            Renderer.sortingOrder = 100 - Mathf.RoundToInt(transform.position.y * 10);
            if (Animation.runtimeAnimatorController != null)
            {
                Animation.SetFloat(Speed, movement.sqrMagnitude > 0.01f ? 1 : 0, 0.12f, dt);
                if (state != lastState)
                {
                    if (state == ActorState.Attack) Animation.CrossFadeInFixedTime(Attack, 0.07f);
                    else if (lastState == ActorState.Attack) Animation.CrossFadeInFixedTime(Locomotion, 0.08f);
                }
            }
            flash = Mathf.Max(0, flash - dt);
            Renderer.color = flash > 0 ? Color.white : state == ActorState.Dead ? new Color(.4f, .4f, .4f, .5f) : normalColor;
            lastState = state;
        }
        public void HitFlash() { flash = .1f; }
        public void OnRent()
        {
            gameObject.SetActive(true);
            lastState = ActorState.Idle; flash = 0;
            if (Animation != null && Animation.runtimeAnimatorController != null) { Animation.speed = 1; Animation.Rebind(); Animation.Update(0); }
        }
        public void OnReturn() { gameObject.SetActive(false); }
    }
}
