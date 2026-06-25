using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(AudioManager))]
public class AudioManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AudioManager manager = (AudioManager)target;
        base.OnInspectorGUI();
        GUILayout.Space(10f);
        Hide3D(manager);
        LoadSound(manager);
        Save3DSetting(manager);
    }

    private void Hide3D(AudioManager manager)
    {
        
        if (manager.setting.IsAudio3D == false)
            return;
        EditorGUILayout.LabelField("3D default sound settings", EditorStyles.boldLabel);
        GUILayout.Space(10f);
        GUIContent labelContent = new GUIContent("Max Distance", "Distance for full sound roll off");
        manager.maxDistance = EditorGUILayout.FloatField(labelContent, Mathf.Clamp(manager.maxDistance, 0, float.MaxValue));
        labelContent = new GUIContent("Min Distance", "Distance for full volume");
        manager.minDistance = EditorGUILayout.FloatField (labelContent, Mathf.Clamp(manager.minDistance, 0, float.MaxValue));
        labelContent = new GUIContent("Is static Spatial Blend", "do you wish to use static or dynamically adjusted spatial blend");
        manager.isStaticSpatial = EditorGUILayout.Toggle(labelContent, manager.isStaticSpatial);
        if(manager.isStaticSpatial == true)
        {
            labelContent = new GUIContent("Spatial Blend", "Sets maximum spatial blend. Spatial Blend sets how much this AudioSource is treated as a 3D source");
            manager.staticSpatial = EditorGUILayout.Slider(labelContent, manager.staticSpatial, 0, 1);
            }
        else
        {
            labelContent = new GUIContent("Transition Point", "Distance where sound starts transition from spatial to stereo");
            manager.transitionPoint = EditorGUILayout.FloatField(labelContent, manager.transitionPoint);
            labelContent = new GUIContent("Max Spatial Blend", "Sets maximum spatial blend. Spatial Blend sets how much this AudioSource is treated as a 3D source");
            manager.maxSpatialBlend = EditorGUILayout.Slider(labelContent, manager.maxSpatialBlend, 0, 1);
            labelContent = new GUIContent("Min Spatial Blend", "Sets minimum spatial blend. Spatial Blend sets how much this AudioSource is treated as a 3D source");
            manager.minSpatialBlend = EditorGUILayout.Slider(labelContent, manager.minSpatialBlend, 0, 1);
            }
        labelContent = new GUIContent("Roll Off Mode", "sets which audio attenuation mode to use, linear is recommended");
        manager.rolloffMode = (AudioRolloffMode)EditorGUILayout.EnumPopup(labelContent, manager.rolloffMode);
        }
    private void Save3DSetting(AudioManager manager)
    {
        if (GUILayout.Button("Save Settings"))
        {
            string[] guids = AssetDatabase.FindAssets("t:AudioSettingSO AudioSetting");
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            AudioSettingSO setting = AssetDatabase.LoadAssetAtPath<AudioSettingSO>(path);
            setting.maxDistance = manager.maxDistance;
            setting.minDistance = manager.minDistance;
            setting.isStaticSpatial = manager.isStaticSpatial;
            setting.staticSpatial = manager.staticSpatial;
            setting.transitionPoint = manager.transitionPoint;
            setting.maxSpatialBlend = manager.maxSpatialBlend;
            setting.minSpatialBlend = manager.minSpatialBlend;
            setting.rolloffMode = manager.rolloffMode;
            LoadedSoundDict dict = manager.GetDict();
            foreach (KeyValuePair<string,AudioClipSO> keyValuePair in dict.loadedSound)
            {
                keyValuePair.Value.SetDefaultSetting();
            }
            EditorUtility.SetDirty(setting);
            EditorUtility.SetDirty(manager);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
            EditorApplication.ExecuteMenuItem("File/Save Project");
        }

    }
    private void LoadSound(AudioManager manager)
    {
        if (GUILayout.Button("Load Sounds"))
        {
            string[] guids = AssetDatabase.FindAssets("t:AudioClipSO", new[] { manager.setting.AudioClipSOPath });
            int count = guids.Length;
            AudioClipSO[] clips = new AudioClipSO[count];
            for (int n = 0; n < count; n++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[n]);
                clips[n] = AssetDatabase.LoadAssetAtPath<AudioClipSO>(path);
            }

            string enumName = "AudioRef";
            string filePathAndName = manager.setting.AudioRefPath + "/" + enumName + ".cs";
            List<string> names = new List<string>();
            for (int i = 0; i < clips.Length; i++)
            {
                if (names.Contains(clips[i].ReferenceName))
                {
                    Debug.LogWarning("There is a duplicate AudioClipSO reference name " + clips[i].ReferenceName + " in the file located at " + AssetDatabase.GUIDToAssetPath(guids[i]));
                    Selection.activeObject = clips[i];
                    //OverrideAudioRefWithDefault();
                    return;
                }
                names.Add(clips[i].ReferenceName);
            }
            using (StreamWriter streamWriter = new StreamWriter(filePathAndName))
            {
                streamWriter.WriteLine("using System;");
                streamWriter.WriteLine("");
                streamWriter.WriteLine("[Serializable]");
                streamWriter.WriteLine("public enum " + enumName);
                streamWriter.WriteLine("{");

                for (int i = 0; i < names.Count; i++)
                {
                    streamWriter.WriteLine("	" + clips[i].ReferenceName + ",");
                }
                streamWriter.WriteLine("}");
                manager.LoadSounds(clips);
            }

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
            EditorApplication.ExecuteMenuItem("File/Save Project");
        }
    }
}
