using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace BetterDesign
{
    [DisallowMultipleComponent]
    public sealed class SceneLauncher : MonoBehaviour
    {
        [SerializeField] AssetReference scene;
        [SerializeField] Camera launcherCamera;
        [SerializeField] EventSystem launcherEvents;
        [SerializeField] GameObject menu,returnBar;
        [SerializeField] Button enterButton,returnButton;
        [SerializeField] TMP_Text status;
        [SerializeField] Image progress;
        AddressableSceneFlow flow;
        int displayedPercent = -1;
        public AddressableSceneFlow Flow { get { return flow; } }
        public bool IsConfigured { get { return scene != null && scene.RuntimeKeyIsValid() && launcherCamera != null && launcherEvents != null && menu != null && returnBar != null && enterButton != null && returnButton != null && status != null && progress != null; } }
        void Awake()
        {
            if (!IsConfigured) { Debug.LogError("BetterDesign: launcher references are incomplete.",this); enabled = false; return; }
            flow = new AddressableSceneFlow(scene.RuntimeKey,gameObject.scene);
            flow.Changed += Refresh; Refresh();
        }
        public async void EnterDemo()
        {
            if (flow == null || flow.Busy || flow.HasScene) return;
            await flow.EnterAsync();
        }
        public async void ReturnToLauncher()
        {
            if (flow == null || flow.Busy || !flow.HasScene) return;
            await flow.LeaveAsync();
        }
        void Update()
        {
            if (flow == null) return;
            if (flow.HasScene && !flow.Busy && Input.GetKeyDown(KeyCode.F10)) ReturnToLauncher();
            if (flow.Busy)
            {
                int percent = Mathf.FloorToInt(flow.Progress*100);
                progress.fillAmount = flow.Progress;
                if (percent != displayedPercent) { displayedPercent = percent; status.text = (flow.State == SceneFlowState.Loading ? "正在加载场景 " : "正在卸载场景 ") + percent + "%"; }
            }
        }
        void Refresh()
        {
            if (this == null || flow == null) return;
            bool loaded = flow.HasScene, busy = flow.Busy;
            launcherEvents.gameObject.SetActive(!busy && !loaded);
            launcherCamera.gameObject.SetActive(!busy && !loaded);
            menu.SetActive(!loaded || busy); returnBar.SetActive(loaded && !busy);
            enterButton.interactable = !busy && !loaded; returnButton.interactable = !busy;
            displayedPercent = -1; progress.fillAmount = loaded ? 1 : 0;
            status.text = flow.State == SceneFlowState.Failed ? "加载或卸载失败，可重试。\n" + flow.Error :
                loaded ? "场景已加载" : busy ? "正在处理场景..." : "准备就绪：点击进入游戏";
            if (!busy && launcherEvents.gameObject.activeInHierarchy) launcherEvents.SetSelectedGameObject(null);
        }
        void OnDestroy()
        {
            if (flow == null) return;
            flow.Changed -= Refresh; flow.Dispose(); flow = null;
        }
    }
}
