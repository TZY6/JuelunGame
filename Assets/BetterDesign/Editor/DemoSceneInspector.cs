using UnityEditor;
using UnityEngine;

namespace BetterDesign.Editor
{
    [CustomEditor(typeof(DemoScene))]
    public sealed class DemoSceneInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.HelpBox("This scene contains the map, hero, enemies, effect slots, Camera, EventSystem and TMP UI. Edit those objects directly in the Hierarchy. GameManager binds their serialized references; Play does not create scene objects.",MessageType.Info);
            if (GUILayout.Button("Validate authored scene and TMP font")) DemoValidation.Validate();
        }
    }
}
