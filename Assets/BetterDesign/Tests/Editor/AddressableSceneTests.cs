using System.Collections;
using System.Threading.Tasks;
using BetterDesign.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace BetterDesign.Tests
{
    public sealed class AddressableSceneTests
    {
        [Test] public void AuthoredLauncherHasValidBindingsAndPrimaryFont()
        {
            var scene = EditorSceneManager.OpenScene(AddressableDemoTools.LauncherPath);
            var launcher = Object.FindObjectOfType<SceneLauncher>();
            Assert.IsTrue(launcher.IsConfigured);
            Assert.AreEqual(2,CountCanvasButtons(scene));
            foreach (var text in Object.FindObjectsOfType<TMPro.TMP_Text>(true))
                Assert.AreEqual("563ebed8c882c5e458174c07361ae986",AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(text.font)));
        }
        static int CountCanvasButtons(Scene scene)
        {
            int count = 0;
            foreach (var root in scene.GetRootGameObjects()) count += root.GetComponentsInChildren<UnityEngine.UI.Button>(true).Length;
            return count;
        }
        [Test] public void AddressableSceneUsesLocalBundleGroupAndStableAddress()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            Assert.AreEqual(AddressableDemoTools.SettingsPath,AssetDatabase.GetAssetPath(settings));
            var entry = settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(AddressableDemoTools.DemoPath));
            Assert.AreEqual(AddressableDemoTools.SceneAddress,entry.address);
            Assert.NotNull(entry.parentGroup.GetSchema<BundledAssetGroupSchema>());
            Assert.IsTrue(entry.parentGroup.GetSchema<BundledAssetGroupSchema>().IncludeInBuild);
        }
        static IEnumerator Wait(Task task)
        {
            float deadline = Time.realtimeSinceStartup+60;
            while (!task.IsCompleted && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.IsTrue(task.IsCompleted,"Addressables operation timed out.");
            Assert.IsFalse(task.IsFaulted,task.Exception?.ToString());
        }
        [UnityTest] public IEnumerator SceneLifecycleSupportsReentryFailureAndDisposalDuringLoad()
        {
            EditorSceneManager.OpenScene(AddressableDemoTools.LauncherPath);
            yield return new EnterPlayMode();
            var launcher = Object.FindObjectOfType<SceneLauncher>();
            Assert.IsNotNull(launcher); var flow = launcher.Flow;
            for (int i=0;i<2;i++)
            {
                var enter = flow.EnterAsync();
                Assert.IsFalse(flow.EnterAsync().Result,"Duplicate load must be rejected.");
                yield return Wait(enter); Assert.IsTrue(enter.Result,flow.Error);
                Assert.AreEqual(SceneFlowState.Loaded,flow.State);
                Assert.AreEqual("BetterDesignDemo",SceneManager.GetActiveScene().name);
                Assert.IsNotNull(Object.FindObjectOfType<DemoScene>());
                Assert.AreEqual(1,Object.FindObjectsOfType<EventSystem>().Length);
                var leave = flow.LeaveAsync(); yield return Wait(leave);
                Assert.IsTrue(leave.Result,flow.Error); Assert.IsFalse(flow.HasScene);
                Assert.IsNull(Object.FindObjectOfType<DemoScene>());
                Assert.AreEqual("BetterDesignLauncher",SceneManager.GetActiveScene().name);
                Assert.AreEqual(1,Object.FindObjectsOfType<EventSystem>().Length);
            }
            var previousHandler = UnityEngine.ResourceManagement.ResourceManager.ExceptionHandler;
            UnityEngine.ResourceManagement.ResourceManager.ExceptionHandler = (handle,error) => {};
            var invalid = new AddressableSceneFlow("better-design/missing-scene",launcher.gameObject.scene);
            try
            {
            var failed = invalid.EnterAsync(); yield return Wait(failed);
            Assert.IsFalse(failed.Result); Assert.AreEqual(SceneFlowState.Failed,invalid.State);
            Assert.IsFalse(invalid.HasScene);
            var retry = invalid.EnterAsync(); yield return Wait(retry); Assert.IsFalse(retry.Result);
            yield return Wait(invalid.DisposeAsync()); Assert.AreEqual(SceneFlowState.Disposed,invalid.State);
            }
            finally { UnityEngine.ResourceManagement.ResourceManager.ExceptionHandler = previousHandler; }
            var loading = flow.EnterAsync(); var disposal = flow.DisposeAsync();
            yield return Wait(loading); yield return Wait(disposal);
            Assert.IsFalse(loading.Result); Assert.IsFalse(flow.HasScene);
            Assert.AreEqual(SceneFlowState.Disposed,flow.State);
            Assert.IsNull(Object.FindObjectOfType<DemoScene>());
            Assert.IsFalse(flow.EnterAsync().Result);
            Debug.Log("ADDRESSABLE FLOW VERIFIED mode="+AddressableAssetSettingsDefaultObject.Settings.ActivePlayModeDataBuilder.Name);
            yield return new ExitPlayMode();
        }
    }
}
