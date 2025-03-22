using UnityEngine;
using UnityEditor;
using Models;

[CustomEditor(typeof(MockRequestHandler))]
public class MockRequestHandlerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Get the target
        MockRequestHandler mockHandler = (MockRequestHandler)target;

        // Add some space
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("AWS Component State Controls", EditorStyles.boldLabel);
        
        // Add buttons for changing states with current state display
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("EC2 State:", GUILayout.Width(80));
        EditorGUILayout.LabelField(mockHandler.EC2State.ToString(), EditorStyles.boldLabel, GUILayout.Width(80));
        if (GUILayout.Button("Change EC2 State", GUILayout.Height(30)))
        {
            mockHandler.ChangeEC2State();
            // Force the inspector to update
            EditorUtility.SetDirty(target);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("RDB State:", GUILayout.Width(80));
        EditorGUILayout.LabelField(mockHandler.RDBState.ToString(), EditorStyles.boldLabel, GUILayout.Width(80));
        if (GUILayout.Button("Change RDB State", GUILayout.Height(30)))
        {
            mockHandler.ChangeRDBState();
            // Force the inspector to update
            EditorUtility.SetDirty(target);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("ALB State:", GUILayout.Width(80));
        EditorGUILayout.LabelField(mockHandler.ALBState.ToString(), EditorStyles.boldLabel, GUILayout.Width(80));
        if (GUILayout.Button("Change ALB State", GUILayout.Height(30)))
        {
            mockHandler.ChangeALBState();
            // Force the inspector to update
            EditorUtility.SetDirty(target);
        }
        EditorGUILayout.EndHorizontal();
    }
}