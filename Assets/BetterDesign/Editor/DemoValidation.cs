using System;
using BetterDesign.Core;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BetterDesign.Editor
{
    public static class DemoValidation
    {
        const string Root = "Assets/BetterDesign/";
        [MenuItem("Tools/BetterDesign/Open Demo")]
        public static void Open()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(Root + "Scenes/BetterDesignDemo.unity");
        }
        [MenuItem("Tools/BetterDesign/Validate Assets")]
        public static void Validate()
        {
            var config = AssetDatabase.LoadAssetAtPath<DemoSettings>(Root+"Configuration/Sanctuary.asset");
            Require(config != null,"Configuration asset missing.");
            Require(config.heroSprite != null && config.enemySprite != null && config.groundSprite != null,"Sprite GUID/fileID reference missing.");
            ValidateController(config.heroAnimator); ValidateController(config.enemyAnimator);
            foreach (string path in new[] { "Animations/HeroAttack.anim", "Animations/EnemyAttack.anim" })
            {
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(Root+path);
                Require(clip != null && AnimationUtility.GetAnimationEvents(clip).Length == 0,"Attack clip contains legacy callbacks.");
            }
            var scene = SceneManager.GetSceneByPath(Root+"Scenes/BetterDesignDemo.unity");
            bool alreadyLoaded = scene.IsValid() && scene.isLoaded;
            if (!alreadyLoaded) scene = EditorSceneManager.OpenScene(Root+"Scenes/BetterDesignDemo.unity",OpenSceneMode.Additive);
            try
            {
                int roots = 0;
                foreach (var go in scene.GetRootGameObjects())
                {
                    var entry = go.GetComponent<DemoScene>();
                    if (entry != null) { roots++; Require(entry.Settings == config,"Entry point configuration missing."); }
                    foreach (var component in go.GetComponentsInChildren<MonoBehaviour>(true))
                        Require(component != null,"Missing script in demo scene.");
                }
                Require(roots == 1,"Expected exactly one composition root.");
            }
            finally { if (!alreadyLoaded) EditorSceneManager.CloseScene(scene,true); }
            Debug.Log("BetterDesign asset validation passed. Next: Test Runner > EditMode > BetterDesign.Tests; then open demo and Play.");
        }
        static void ValidateController(RuntimeAnimatorController controller)
        {
            var animator = controller as AnimatorController;
            Require(animator != null && animator.layers.Length == 1,"Animator controller missing or invalid.");
            bool foundTree = false, foundAttack = false;
            foreach (var state in animator.layers[0].stateMachine.states)
            {
                if (state.state.name == "Attack") foundAttack = state.state.motion != null;
                var tree = state.state.motion as BlendTree;
                if (tree != null)
                {
                    foundTree = tree.blendParameter == "Speed" && tree.children.Length == 2;
                    foreach (var child in tree.children) Require(child.motion != null,"Blend Tree child motion missing.");
                }
            }
            Require(foundTree && foundAttack,"Expected Speed Blend Tree and Attack state.");
        }
        static void Require(bool condition,string message) { if (!condition) throw new InvalidOperationException("BetterDesign: "+message); }
    }
}
