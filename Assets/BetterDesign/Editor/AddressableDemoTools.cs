using System;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace BetterDesign.Editor
{
    public static class AddressableDemoTools
    {
        public const string Root = "Assets/BetterDesign/Addressables";
        public const string SettingsPath = Root + "/SanctuaryAddressables.asset";
        public const string DemoPath = "Assets/BetterDesign/Scenes/BetterDesignDemo.unity";
        public const string LauncherPath = "Assets/BetterDesign/Scenes/BetterDesignLauncher.unity";
        public const string SceneAddress = "better-design/sanctuary";
        [MenuItem("Tools/BetterDesign/Addressables/Configure")]
        public static void Configure()
        {
            var current = AddressableAssetSettingsDefaultObject.Settings;
            if (current != null && AssetDatabase.GetAssetPath(current) != SettingsPath) throw new InvalidOperationException("Another Addressables settings asset is active. Merge the BetterDesign scene into it explicitly.");
            var settings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(SettingsPath);
            if (settings == null) settings = AddressableAssetSettings.Create(Root,"SanctuaryAddressables",true,true);
            AddressableAssetSettingsDefaultObject defaultObject;
            if (!EditorBuildSettings.TryGetConfigObject(AddressableAssetSettingsDefaultObject.kDefaultConfigObjectName,out defaultObject))
            {
                defaultObject = ScriptableObject.CreateInstance<AddressableAssetSettingsDefaultObject>();
                AssetDatabase.CreateAsset(defaultObject,Root+"/DefaultObject.asset");
                EditorBuildSettings.AddConfigObject(AddressableAssetSettingsDefaultObject.kDefaultConfigObjectName,defaultObject,true);
            }
            AddressableAssetSettingsDefaultObject.Settings = settings;
            var group = settings.FindGroup("BetterDesign Scenes");
            if (group == null) group = settings.CreateGroup("BetterDesign Scenes",false,false,false,null,typeof(BundledAssetGroupSchema),typeof(ContentUpdateGroupSchema));
            var schema = group.GetSchema<BundledAssetGroupSchema>();
            schema.BuildPath.SetVariableByName(settings,AddressableAssetSettings.kLocalBuildPath);
            schema.LoadPath.SetVariableByName(settings,AddressableAssetSettings.kLocalLoadPath);
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            var entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(DemoPath),group);
            entry.address = SceneAddress;
            settings.BuildAddressablesWithPlayerBuild = AddressableAssetSettings.PlayerBuildOption.BuildWithPlayer;
            EditorUtility.SetDirty(settings); EditorUtility.SetDirty(group); EditorUtility.SetDirty(schema);
            AssetDatabase.SaveAssets();
            Debug.Log("BetterDesign Addressables configured: "+SceneAddress);
        }
        [MenuItem("Tools/BetterDesign/Addressables/Open Launcher")]
        public static void OpenLauncher()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(LauncherPath);
        }
        [MenuItem("Tools/BetterDesign/Addressables/Build Local Content")]
        public static void BuildContent()
        {
            Configure();
            AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
            if (!string.IsNullOrEmpty(result.Error)) throw new InvalidOperationException(result.Error);
            Debug.Log("BetterDesign Addressables content built: "+result.OutputPath);
        }
        [MenuItem("Tools/BetterDesign/Addressables/Build Windows Demo")]
        public static void BuildPlayer()
        {
            string path = EditorUtility.SaveFilePanel("Build BetterDesign Windows Demo","","BetterDesign.exe","exe");
            if (string.IsNullOrEmpty(path)) return;
            Configure();
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { LauncherPath },locationPathName = path,target = BuildTarget.StandaloneWindows64 });
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new InvalidOperationException("Player build failed. Check the Console, including scripts outside BetterDesign.");
        }
    }
}
