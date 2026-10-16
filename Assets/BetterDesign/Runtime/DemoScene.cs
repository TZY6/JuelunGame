using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;

namespace BetterDesign
{
    // Only scene entry point; owns service creation, ticking and teardown.
    [DisallowMultipleComponent]
    public sealed class DemoScene : MonoBehaviour
    {
        [SerializeField] DemoSettings settings;
        DemoWorld world;
        DemoHud hud;
        Camera gameCamera;
        public DemoSettings Settings { get { return settings; } }
        void Awake()
        {
            if (settings == null || settings.heroSprite == null || settings.enemySprite == null ||
                settings.heroAnimator == null || settings.enemyAnimator == null)
            {
                Debug.LogError("BetterDesign: missing demo configuration / art references.", this);
                enabled = false; return;
            }
            var cameraGo = new GameObject("Camera - adaptive orthographic",typeof(Camera),typeof(AudioListener));
            cameraGo.transform.SetParent(transform,false); cameraGo.transform.position = new Vector3(0,0,-10);
            gameCamera = cameraGo.GetComponent<Camera>(); gameCamera.orthographic = true;
            gameCamera.clearFlags = CameraClearFlags.SolidColor;
            gameCamera.backgroundColor = new Color(.06f,.11f,.16f);
            var eventGo = new GameObject("EventSystem - UGUI input",typeof(EventSystem),typeof(StandaloneInputModule));
            eventGo.transform.SetParent(transform,false);
            world = new DemoWorld(transform, settings, new JsonSaveStore(Application.persistentDataPath),new KeyboardInput());
            hud = new DemoHud(transform,world,world.NewRun);
        }
        void Update()
        {
            if (world == null) return;
            if (Input.GetKeyDown(KeyCode.F1)) hud.ToggleArchitecture();
            if (Input.GetKeyDown(KeyCode.Escape)) hud.Back();
            if (Input.GetKeyDown(KeyCode.Tab)) hud.ToggleBag();
            if (Input.GetKeyDown(KeyCode.F5)) world.Save();
            if (Input.GetKeyDown(KeyCode.F9)) { world.Load(); hud.CloseAll(); }
            if (!hud.HasModal)
            {
                if (Input.GetKeyDown(KeyCode.E)) world.Interact();
                if (Input.GetKeyDown(KeyCode.B)) world.Buy();
                if (Input.GetKeyDown(KeyCode.H)) world.Heal();
                if (Input.GetKeyDown(KeyCode.N)) world.NextWave();
            }
            Profiler.BeginSample("BetterDesign.World");
            try { world.Tick(Mathf.Min(Time.deltaTime,.05f)); }
            finally { Profiler.EndSample(); }
            hud.Tick(Time.unscaledDeltaTime);
            gameCamera.orthographicSize = Mathf.Max(8.4f,12f / Mathf.Max(.1f,gameCamera.aspect));
        }
        void OnApplicationFocus(bool focused)
        {
            if (!focused && hud != null && !hud.HasModal) hud.Back();
        }
        void OnDestroy()
        {
            if (hud != null) hud.Dispose();
            if (world != null) world.Dispose();
        }
    }
}
