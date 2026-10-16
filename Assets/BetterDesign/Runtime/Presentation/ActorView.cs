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
        Vector3 originalScale;
        bool originalFlip;
        int originalSorting;
        float flash;
        Color normalColor;
        public bool HasSceneReferences { get { return spriteRenderer != null && animator != null; } }
        public void Initialize()
        {
            if (!HasSceneReferences) throw new System.InvalidOperationException(name + ": actor scene references missing.");
            normalColor = Renderer.color; originalScale = transform.localScale;
            originalFlip = Renderer.flipX; originalSorting = Renderer.sortingOrder;
            lastState = ActorState.Idle;
        }
        public void Render(ActorModel model,Vector2 movement,float dt)
        {
            transform.position = new Vector3(model.X,model.Y,0);
            Present(model.Machine.State,movement,dt);
        }
        void Present(ActorState state, Vector2 movement, float dt)
        {
            if (movement.x != 0) Renderer.flipX = movement.x < 0;
            Renderer.sortingOrder = 100 - Mathf.RoundToInt(transform.position.y * 10);
            if (Animation.runtimeAnimatorController != null)
            {
                Animation.SetFloat(Speed, movement.sqrMagnitude > 0.01f ? 1 : 0, 0.12f, dt);
                if (state != lastState)
                {
                    if ((state == ActorState.Attack || state == ActorState.Skill)) Animation.CrossFadeInFixedTime(Attack, 0.07f);
                    else if ((lastState == ActorState.Attack || lastState == ActorState.Skill)) Animation.CrossFadeInFixedTime(Locomotion, 0.08f);
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
            Renderer.color = normalColor; Renderer.flipX = originalFlip; Renderer.sortingOrder = originalSorting;
            transform.localScale = originalScale;
            if (Animation != null && Animation.runtimeAnimatorController != null) { Animation.speed = 1; Animation.Rebind(); Animation.Update(0); }
        }
        public void OnReturn()
        {
            if (this == null) return;
            StopAllCoroutines(); flash = 0; Renderer.color = normalColor;
            Animation.speed = 1; gameObject.SetActive(false);
        }
    }
}
