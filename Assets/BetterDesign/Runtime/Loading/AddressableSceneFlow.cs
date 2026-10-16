using System;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
namespace BetterDesign
{
    public enum SceneFlowState { Idle,Loading,Loaded,Unloading,Failed,Disposed }
    public sealed class AddressableSceneFlow : IDisposable
    {
        readonly object key;
        readonly Scene owner;
        AsyncOperationHandle<SceneInstance> sceneHandle,unloadHandle;
        bool disposed;
        Task<bool> pending;
        public SceneFlowState State { get; private set; }
        public string Error { get; private set; } = "";
        public bool Busy { get { return State == SceneFlowState.Loading || State == SceneFlowState.Unloading; } }
        public bool HasScene { get { return sceneHandle.IsValid() && sceneHandle.Status == AsyncOperationStatus.Succeeded && sceneHandle.Result.Scene.isLoaded; } }
        public float Progress { get { return unloadHandle.IsValid() ? unloadHandle.PercentComplete : sceneHandle.IsValid() ? sceneHandle.PercentComplete : 0; } }
        public event Action Changed;
        public AddressableSceneFlow(object key,Scene owner) { this.key = key; this.owner = owner; }
        void SetState(SceneFlowState state) { State = state; Changed?.Invoke(); }
        public Task<bool> EnterAsync()
        {
            if (disposed || Busy || HasScene) return Task.FromResult(false);
            pending = Load(); return pending;
        }
        async Task<bool> Load()
        {
            Error = ""; SetState(SceneFlowState.Loading);
            try
            {
                if (key == null || key is string text && string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Scene key is empty.");
                sceneHandle = Addressables.LoadSceneAsync(key,LoadSceneMode.Additive,true);
                await Completion(sceneHandle);
                if (sceneHandle.Status != AsyncOperationStatus.Succeeded) throw sceneHandle.OperationException ?? new InvalidOperationException("Scene load failed.");
                if (disposed) { await Unload(); return false; }
                if (!SceneManager.SetActiveScene(sceneHandle.Result.Scene)) throw new InvalidOperationException("Unable to activate the loaded scene.");
                SetState(SceneFlowState.Loaded); return true;
            }
            catch (Exception error)
            {
                Error = error.Message;
                if (HasScene) await Unload();
                else if (sceneHandle.IsValid()) { Addressables.Release(sceneHandle); sceneHandle = default; }
                SetState(disposed ? SceneFlowState.Disposed : SceneFlowState.Failed); return false;
            }
        }
        public Task<bool> LeaveAsync()
        {
            if (disposed || Busy || !HasScene) return Task.FromResult(false);
            pending = Unload(); return pending;
        }
        async Task<bool> Unload()
        {
            SetState(SceneFlowState.Unloading);
            try
            {
                if (owner.IsValid() && owner.isLoaded) SceneManager.SetActiveScene(owner);
                unloadHandle = Addressables.UnloadSceneAsync(sceneHandle,false);
                await Completion(unloadHandle);
                if (unloadHandle.Status != AsyncOperationStatus.Succeeded) throw unloadHandle.OperationException ?? new InvalidOperationException("Scene unload failed.");
                sceneHandle = default;
                SetState(disposed ? SceneFlowState.Disposed : SceneFlowState.Idle); return true;
            }
            catch (Exception error) { Error = error.Message; SetState(disposed ? SceneFlowState.Disposed : SceneFlowState.Failed); return false; }
            finally { if (unloadHandle.IsValid()) Addressables.Release(unloadHandle); unloadHandle = default; }
        }
        static Task Completion<T>(AsyncOperationHandle<T> operation)
        {
            if (operation.IsDone) return Task.CompletedTask;
            var source = new TaskCompletionSource<bool>();
            operation.Completed += _ => source.TrySetResult(true);
            return source.Task;
        }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true; Changed = null;
            if (Busy) return;
            if (HasScene) pending = Unload();
            else SetState(SceneFlowState.Disposed);
        }
        public async Task DisposeAsync()
        {
            Dispose();
            if (pending != null) await pending;
        }
    }
}
