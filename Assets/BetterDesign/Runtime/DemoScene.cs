using UnityEngine;
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
        DemoWorld world;
        public DemoSettings Settings { get { return settings; } }
        public WorldSceneBindings WorldManager { get { return worldManager; } }
        public PersistenceManager Persistence { get { return persistenceManager; } }
        public DemoHud UiManager { get { return uiManager; } }
        public Camera GameCamera { get { return gameCamera; } }
        void Awake()
        {
            if (settings == null || worldManager == null || !worldManager.IsConfigured ||
                persistenceManager == null || uiManager == null || !uiManager.IsConfigured || gameCamera == null)
            {
                Debug.LogError("BetterDesign: assign all authored scene references on the managers.",this);
                enabled = false; return;
            }
            world = new DemoWorld(worldManager,settings,persistenceManager,new KeyboardInput());
            uiManager.Bind(world);
        }
        void Update()
        {
            if (world == null) return;
            if (Input.GetKeyDown(KeyCode.F1)) uiManager.ToggleArchitecture();
            if (Input.GetKeyDown(KeyCode.Escape)) uiManager.Back();
            if (Input.GetKeyDown(KeyCode.Tab)) uiManager.ToggleBag();
            if (Input.GetKeyDown(KeyCode.F5)) world.Save();
            if (Input.GetKeyDown(KeyCode.F9)) { world.Load(); uiManager.CloseAll(); }
            if (!uiManager.HasModal)
            {
                if (Input.GetKeyDown(KeyCode.E)) world.Interact();
                if (Input.GetKeyDown(KeyCode.B)) world.Buy();
                if (Input.GetKeyDown(KeyCode.H)) world.Heal();
                if (Input.GetKeyDown(KeyCode.N)) world.NextWave();
            }
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
        void OnDestroy()
        {
            if (uiManager != null) uiManager.Dispose();
            if (world != null) world.Dispose();
        }
    }
}
