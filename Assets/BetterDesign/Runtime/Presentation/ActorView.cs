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
        [SerializeField] RuntimeAnimatorController bowController;
        RuntimeAnimatorController defaultController;
        public RuntimeAnimatorController BowController { get { return bowController; } }
        [SerializeField] SpriteRenderer healthBackground,healthFill,actionIndicator,aimIndicator;
        public bool HasCombatReferences { get { return healthBackground != null && healthFill != null && actionIndicator != null; } }
        public SpriteRenderer Renderer { get { return spriteRenderer; } }
        public Animator Animation { get { return animator; } }
        static readonly int Speed = UnityEngine.Animator.StringToHash("Speed");
        static readonly int Locomotion = UnityEngine.Animator.StringToHash("Base Layer.Locomotion");
        static readonly int Attack = UnityEngine.Animator.StringToHash("Base Layer.Attack");
        ActorState lastState;
        Vector3 originalScale;
        bool originalFlip,desiredFlip;
        int originalSorting;
        float flash;
        Color normalColor;
        public bool HasSceneReferences { get { return spriteRenderer != null && animator != null; } }
        public void Initialize()
        {
            if (!HasSceneReferences) throw new System.InvalidOperationException(name + ": actor scene references missing.");
            defaultController = animator.runtimeAnimatorController;
            normalColor = Renderer.color; originalScale = transform.localScale;
            originalFlip = desiredFlip = Renderer.flipX; originalSorting = Renderer.sortingOrder;
            lastState = ActorState.Idle;
        }
        public void Render(ActorModel model,Vector2 movement,float dt,int maximumHealth = 100)
        {
            transform.position = new Vector3(model.X,model.Y,0);
            if (Mathf.Abs(model.FacingX) > .01f) desiredFlip = model.FacingX < 0;
            Renderer.flipX = desiredFlip;
            Present(model.Machine.State,movement,dt);
            if (!HasCombatReferences) return;
            bool alive = model.Health > 0;
            healthBackground.enabled = healthFill.enabled = alive;
            float ratio = Mathf.Clamp01(model.Health/(float)Mathf.Max(1,maximumHealth));
            healthFill.transform.localScale = new Vector3(.9f*ratio,.075f,1);
            healthFill.transform.localPosition = new Vector3(-.45f+.45f*ratio,.82f,0);
            healthFill.color = ratio > .35f ? new Color(.3f,.95f,.55f) : new Color(1,.3f,.2f);
            Vector3 direction = new Vector3(model.FacingX,model.FacingY,0);
            float angle = Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg;
            actionIndicator.enabled = alive && (model.Machine.State == ActorState.Attack || model.Machine.State == ActorState.Skill);
            actionIndicator.transform.localPosition = direction*.9f;
            actionIndicator.transform.localRotation = Quaternion.Euler(0,0,angle);
            actionIndicator.color = model.Machine.State == ActorState.Skill ? new Color(.65f,.4f,1,.85f) : new Color(1,.65f,.2f,.85f);
            if (aimIndicator != null)
            {
                aimIndicator.enabled = alive;
                aimIndicator.transform.localPosition = direction*1.05f;
                aimIndicator.transform.localRotation = Quaternion.Euler(0,0,angle);
            }
        }
        void Present(ActorState state, Vector2 movement, float dt)
        {
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
            Renderer.color = flash > 0 ? new Color(1,.15f,.15f,1) : state == ActorState.Dead ? new Color(.4f, .4f, .4f, .5f) : normalColor;
            lastState = state;
        }
        void LateUpdate() { if (spriteRenderer != null) spriteRenderer.flipX = desiredFlip; }
        public void SetWeapon(bool bow)
        {
            var controller = bow && bowController != null ? bowController : defaultController;
            if (Animation.runtimeAnimatorController == controller) return;
            Animation.runtimeAnimatorController = controller; lastState = ActorState.Idle;
            Animation.Rebind(); Animation.Update(0);
        }
        public void HitFlash() { flash = .1f; }
        public void OnRent()
        {
            gameObject.SetActive(true);
            lastState = ActorState.Idle; flash = 0;
            Renderer.color = normalColor; Renderer.flipX = desiredFlip = originalFlip; Renderer.sortingOrder = originalSorting;
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
