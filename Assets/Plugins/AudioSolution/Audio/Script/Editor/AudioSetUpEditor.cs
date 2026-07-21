using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioSetUp))]
public class AudioSetUpEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AudioSetUp setUp = (AudioSetUp)target;
        base.OnInspectorGUI();
        if (setUp.IsString)
        {
            EditorGUILayout.LabelField("File Path to where AudioClipSO are Stored. Blank for default", EditorStyles.boldLabel);
            setUp.AudioClipSOLoc = EditorGUILayout.TextField(setUp.AudioClipSOLoc);
        }
        else
        {
            EditorGUILayout.LabelField("Reference a AudioClipSO for destination folder, Null for default", EditorStyles.boldLabel);
            setUp.clipLoc = (AudioClipSO)EditorGUILayout.ObjectField(setUp.clipLoc, typeof(AudioClipSO),false);

        }

        setUp.Is3D();
        if (GUILayout.Button("Run StartUp"))
        {
            setUp.RunSetup();
        }
        
    }
}
