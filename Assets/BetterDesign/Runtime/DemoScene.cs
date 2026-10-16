using UnityEngine;
using BetterDesign.Core;
using UnityEngine.Profiling;

namespace BetterDesign
{
    // GameManager: only binds existing scene references and owns system lifetimes.
    [DisallowMultipleComponent]
    public sealed class DemoScene : MonoBehaviour
    {
        [SerializeField] DemoSettings settings;
        [SerializeField] WorldSceneBindings worldManager;
        [SerializeField] PersistenceManager persistenceManager;
        [SerializeField] DemoHud uiManager;
        [SerializeField] Camera gameCamera;
        [SerializeField] PlayerInputReader inputReader;
        [SerializeField] InventoryView inventoryView;
        InventoryController inventoryController;
        DemoWorld world;
        public DemoSettings Settings { get { return settings; } }
        public WorldSceneBindings WorldManager { get { return worldManager; } }
        public PersistenceManager Persistence { get { return persistenceManager; } }
        public DemoHud UiManager { get { return uiManager; } }
        public Camera GameCamera { get { return gameCamera; } }
        public PlayerInputReader InputReader { get { return inputReader; } }
        public InventoryView InventoryView { get { return inventoryView; } }
        void Awake()
        {
            if (settings == null || !settings.IsValid || worldManager == null || !worldManager.IsConfigured ||
                persistenceManager == null || uiManager == null || !uiManager.IsConfigured || gameCamera == null ||
                inputReader == null || inventoryView == null || !inventoryView.IsConfigured)
            {
                Debug.LogError("BetterDesign: assign all authored scene references on the managers.",this);
                enabled = false; return;
            }
            world = new DemoWorld(worldManager,settings,persistenceManager,inputReader);
            uiManager.Bind(world);
            inventoryController = new InventoryController(world.Model,world.Events,inventoryView,world);
            inventoryView.Bind(inventoryController);
        }
        void Update()
        {
            if (world == null) return;
            inputReader.Capture();
            if (inputReader.ObserverPressed) uiManager.ToggleArchitecture();
            if (inputReader.PausePressed) uiManager.Back();
            if (inputReader.InventoryPressed) uiManager.ToggleBag();
            if (inputReader.SavePressed) world.Save();
            if (inputReader.LoadPressed) { world.Load(); uiManager.CloseAll(); }
            if (!uiManager.HasModal)
            {
                if (inputReader.UpgradeSkillPressed) world.Events.Notice.Publish(world.Model.UpgradeMagic() ? "法术升级，伤害 +5。" : "需要 2 级和技能点，最高 3 阶。");
                if (inputReader.SwitchWeaponPressed) world.Model.EquipWeapon(1-world.Model.Progression.Weapon);
                if (inputReader.InteractPressed) world.Interact();
                if (inputReader.BuyPressed) world.Buy();
                if (inputReader.HealPressed) world.Heal();
                if (inputReader.NextWavePressed) world.NextWave();
            }
            inputReader.SetGameplayBlocked(uiManager.HasModal || !Application.isFocused);
            Profiler.BeginSample("BetterDesign.World");
            try { world.Tick(Mathf.Min(Time.deltaTime,.05f)); }
            finally { Profiler.EndSample(); }
            uiManager.Tick(Time.unscaledDeltaTime);
            gameCamera.orthographicSize = Mathf.Max(8.4f,12f / Mathf.Max(.1f,gameCamera.aspect));
        }
        void OnApplicationFocus(bool focused)
        {
            if (!focused && world != null && !uiManager.HasModal) uiManager.Back();
        }
        void OnDisable() { if (inputReader != null) inputReader.SetGameplayBlocked(true); }
        void OnDestroy()
        {
            if (inventoryView != null) inventoryView.Bind(null);
            if (inventoryController != null) inventoryController.Dispose();
            if (uiManager != null) uiManager.Dispose();
            if (world != null) world.Dispose();
        }
    }
}
