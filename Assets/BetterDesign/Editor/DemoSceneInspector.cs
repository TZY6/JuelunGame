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
            EditorGUILayout.HelpBox("Press Play to compose the sanctuary, actors, object pools and UGUI. Everything is owned by this scene root. Inspect the live hierarchy and press F1 for architecture telemetry.",MessageType.Info);
            if (GUILayout.Button("Validate scene and resource references")) DemoValidation.Validate();
        }
        void OnSceneGUI()
        {
            Handles.color = new Color(.3f,.9f,.8f,1);
            Handles.DrawWireCube(Vector3.zero,new Vector3(20,12,0));
            Handles.Label(new Vector3(-7,0,0),"Sanctuary / Quest beacon");
            Handles.Label(new Vector3(2,1,0),"Combat arena (created on Play)");
        }
    }
}
