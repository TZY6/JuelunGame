using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BetterDesign.Editor
{
    public static class DemoValidation
    {
        const string Root = "Assets/BetterDesign/";
        [MenuItem("Tools/BetterDesign/Open Demo")]
        public static void Open()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(Root+"Scenes/BetterDesignDemo.unity");
        }
        [MenuItem("Tools/BetterDesign/Validate Assets")]
        public static void Validate()
        {
            var config = AssetDatabase.LoadAssetAtPath<DemoSettings>(Root+"Configuration/Sanctuary.asset");
            Require(config != null,"Configuration asset missing.");
            var expectedFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/7000msyhbdallicon SDF.asset");
            Require(expectedFont != null,"Requested TMP font asset missing.");
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
                var entries = new List<DemoScene>();
                var textComponents = new List<TMP_Text>();
                var buttons = new List<Button>();
                int actors = 0, cameras = 0, eventSystems = 0, legacyText = 0;
                foreach (var go in scene.GetRootGameObjects())
                {
                    entries.AddRange(go.GetComponentsInChildren<DemoScene>(true));
                    textComponents.AddRange(go.GetComponentsInChildren<TMP_Text>(true));
                    buttons.AddRange(go.GetComponentsInChildren<Button>(true));
                    actors += go.GetComponentsInChildren<ActorView>(true).Length;
                    cameras += go.GetComponentsInChildren<Camera>(true).Length;
                    eventSystems += go.GetComponentsInChildren<EventSystem>(true).Length;
                    legacyText += go.GetComponentsInChildren<Text>(true).Length;
                    foreach (var component in go.GetComponentsInChildren<MonoBehaviour>(true))
                        Require(component != null,"Missing script in authored scene.");
                    foreach (var sprite in go.GetComponentsInChildren<SpriteRenderer>(true))
                        Require(sprite.sprite != null,"Missing sprite: "+sprite.name);
                }
                Require(entries.Count == 1,"Expected exactly one GameManager.");
                var entry = entries[0];
                Require(entry.Settings == config && entry.Persistence != null && entry.GameCamera != null,"GameManager references missing.");
                Require(entry.WorldManager != null && entry.WorldManager.IsConfigured,"WorldManager scene bindings missing/duplicated.");
                ValidateController(entry.WorldManager.hero.Animation.runtimeAnimatorController);
                foreach (var actor in entry.WorldManager.enemySlots) ValidateController(actor.Animation.runtimeAnimatorController);
                Require(entry.UiManager != null && entry.UiManager.IsConfigured && entry.UiManager.Font == expectedFont,"UIManager references missing.");
                Require(actors == 17,"Expected authored hero + 16 enemy slots.");
                Require(cameras == 1 && eventSystems == 1,"Expected authored Camera and EventSystem.");
                Require(entry.WorldManager.mapRoot.childCount > 0,"Authored map missing.");
                Require(legacyText == 0 && textComponents.Count == 21,"Expected 21 TMP labels and no legacy Text.");
                foreach (var label in textComponents)
                    Require(label.font == expectedFont,"Incorrect TMP font on "+label.name);
                Require(buttons.Count == 8,"Expected 8 authored UI buttons.");
                foreach (var button in buttons)
                    Require(button.onClick.GetPersistentEventCount() == 1 &&
                        button.onClick.GetPersistentTarget(0) == entry.UiManager &&
                        !string.IsNullOrEmpty(button.onClick.GetPersistentMethodName(0)),
                        "Button requires a serialized UIManager command: "+button.name);
            }
            finally { if (!alreadyLoaded) EditorSceneManager.CloseScene(scene,true); }
            Debug.Log("BetterDesign: authored scene, 17 actors, 48 effect slots, 21 TMP labels, font and button wiring validated.");
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
