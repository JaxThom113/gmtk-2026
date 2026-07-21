using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioStaticInstance))]
public class AudioStaticInstanceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AudioStaticInstance audioInstance = (AudioStaticInstance)target;
        base.OnInspectorGUI();
        if (audioInstance.setting == null)
        {
            audioInstance.setting = Resources.Load<AudioSettingSO>("AudioSetting");
        }
        if (audioInstance.setting.IsAudio3D)
        {
            audioInstance.listener = Resources.Load<TransformSO>("ListenerPos");
        }
        if(GUILayout.Button("Load Settings"))
        {
            audioInstance.SetupAudioSource();
        }
    }
}
