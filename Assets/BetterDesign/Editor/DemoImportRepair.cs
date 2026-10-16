using System;
using UnityEditor;
using UnityEngine;
namespace BetterDesign.Editor
{
    [InitializeOnLoad]
    public static class DemoImportRepair
    {
        static DemoImportRepair() { EditorApplication.delayCall += EnsureBindings; }
        [MenuItem("Tools/BetterDesign/Repair Script Imports")]
        public static void EnsureBindings()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall -= EnsureBindings;
                EditorApplication.delayCall += EnsureBindings; return;
            }
            bool repaired = false;
            repaired |= Check("Runtime/DemoScene.cs",typeof(DemoScene));
            repaired |= Check("Runtime/Configuration/DemoSettings.cs",typeof(DemoSettings));
            repaired |= Check("Runtime/Gameplay/WorldSceneBindings.cs",typeof(WorldSceneBindings));
            repaired |= Check("Runtime/Infrastructure/PersistenceManager.cs",typeof(PersistenceManager));
            repaired |= Check("Runtime/Input/PlayerInputReader.cs",typeof(PlayerInputReader));
            repaired |= Check("Runtime/Presentation/ActorView.cs",typeof(ActorView));
            repaired |= Check("Runtime/Presentation/LootView.cs",typeof(LootView));
            repaired |= Check("Runtime/UI/DemoHud.cs",typeof(DemoHud));
            repaired |= Check("Runtime/UI/InventoryView.cs",typeof(InventoryView));
            if (repaired) AssetDatabase.ImportAsset("Assets/BetterDesign/Configuration/Sanctuary.asset",ImportAssetOptions.ForceUpdate|ImportAssetOptions.ForceSynchronousImport);
        }
        static bool Check(string relative,Type expected)
        {
            string path = "Assets/BetterDesign/" + relative;
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (script != null && script.GetClass() == expected) return false;
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate|ImportAssetOptions.ForceSynchronousImport);
            script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (script == null || script.GetClass() != expected) Debug.LogError("BetterDesign script binding unavailable: " + path);
            return true;
        }
    }
}
