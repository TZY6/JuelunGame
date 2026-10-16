using UnityEngine;
using UnityEngine.EventSystems;

namespace BetterDesign
{
    // Scene component: bindings visible in Inspector. Sample exactly once in GameManager.Update.
    [DisallowMultipleComponent]
    public sealed class PlayerInputReader : MonoBehaviour, IPlayerInput, IAimedPlayerInput
    {
        [SerializeField] Camera aimCamera;
        [SerializeField] EventSystem uiEventSystem;
        public EventSystem UiEventSystem { get { return uiEventSystem; } }
        public Camera AimCamera { get { return aimCamera; } }
        public void SetAimCamera(Camera value) { aimCamera = value; }
        Vector2 aimPoint;
        bool hasAim;
        public bool HasAim { get { return !blocked && hasAim; } }
        public Vector2 AimPoint { get { return aimPoint; } }
        [Header("Movement")]
        public KeyCode up = KeyCode.W, down = KeyCode.S, left = KeyCode.A, right = KeyCode.D;
        public bool allowArrowKeys = true;
        [Header("Combat")]
        public KeyCode attack = KeyCode.Space, spell = KeyCode.Q, dash = KeyCode.LeftShift;
        public bool allowMouseButtons = true;
        [Header("Equipment and progression")]
        public KeyCode switchWeapon = KeyCode.R, upgradeSkill = KeyCode.K;
        [Header("World commands")]
        public KeyCode interact = KeyCode.E, buy = KeyCode.B, heal = KeyCode.H, nextWave = KeyCode.N;
        [Header("UI and persistence")]
        public KeyCode inventory = KeyCode.Tab, pause = KeyCode.Escape, observer = KeyCode.F1, save = KeyCode.F5, load = KeyCode.F9;
        Vector2 movement;
        bool attackPressed,spellPressed,dashPressed,blocked;
        public Vector2 Move { get { return blocked ? Vector2.zero : movement; } }
        public bool Attack { get { return !blocked && attackPressed; } }
        public bool Dash { get { return !blocked && dashPressed; } }
        public bool Spell { get { return !blocked && spellPressed; } }
        public bool InventoryPressed { get; private set; }
        public bool PausePressed { get; private set; }
        public bool ObserverPressed { get; private set; }
        public bool SavePressed { get; private set; }
        public bool LoadPressed { get; private set; }
        public bool InteractPressed { get; private set; }
        public bool BuyPressed { get; private set; }
        public bool HealPressed { get; private set; }
        public bool UpgradeSkillPressed { get; private set; }
        public bool SwitchWeaponPressed { get; private set; }
        public bool NextWavePressed { get; private set; }
        public void Capture()
        {
            bool north = Input.GetKey(up) || (allowArrowKeys && Input.GetKey(KeyCode.UpArrow));
            bool south = Input.GetKey(down) || (allowArrowKeys && Input.GetKey(KeyCode.DownArrow));
            bool west = Input.GetKey(left) || (allowArrowKeys && Input.GetKey(KeyCode.LeftArrow));
            bool east = Input.GetKey(right) || (allowArrowKeys && Input.GetKey(KeyCode.RightArrow));
            movement = new Vector2((east ? 1 : 0) - (west ? 1 : 0),(north ? 1 : 0) - (south ? 1 : 0)).normalized;
            bool overUI = uiEventSystem != null && uiEventSystem.IsPointerOverGameObject();
            hasAim = allowMouseButtons && !overUI && aimCamera != null && aimCamera.pixelRect.Contains(Input.mousePosition);
            if (hasAim) aimPoint = aimCamera.ScreenToWorldPoint(Input.mousePosition);
            attackPressed = Input.GetKey(attack) || (allowMouseButtons && !overUI && Input.GetMouseButton(0));
            spellPressed = Input.GetKey(spell) || (allowMouseButtons && !overUI && Input.GetMouseButton(1));
            dashPressed = Input.GetKeyDown(dash);
            SwitchWeaponPressed = Input.GetKeyDown(switchWeapon);
            UpgradeSkillPressed = Input.GetKeyDown(upgradeSkill);
            InventoryPressed = Input.GetKeyDown(inventory); PausePressed = Input.GetKeyDown(pause);
            ObserverPressed = Input.GetKeyDown(observer); SavePressed = Input.GetKeyDown(save); LoadPressed = Input.GetKeyDown(load);
            InteractPressed = Input.GetKeyDown(interact); BuyPressed = Input.GetKeyDown(buy);
            HealPressed = Input.GetKeyDown(heal); NextWavePressed = Input.GetKeyDown(nextWave);
        }
        public void SetGameplayBlocked(bool value) { blocked = value; }
    }
}
