using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AudioClipSO))]
public class AudioClipSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AudioClipSO SO = (AudioClipSO)target;
        EditorUtility.SetDirty(SO);
        base.OnInspectorGUI();
        if(SO.setting == null)
        {
            SO.setting = Resources.Load<AudioSettingSO>("AudioSetting");
        }
        if (!SO.setting.IsAudio3D)
            return;
        GUIContent labelContent = new GUIContent("Override 3D settings?", "do you wish to override default 3D settings for this SO");
        SO.notDefault = EditorGUILayout.Toggle(labelContent, SO.notDefault);
        if (!SO.notDefault)
        {
            if (!SO.defaultSet)
                SO.SetDefaultSetting();
            return;
        }
        else
        {
            if (SO.defaultSet)
                SO.defaultSet = false;

        }
        EditorGUILayout.LabelField("3D sound settings", EditorStyles.boldLabel);
        GUILayout.Space(10f);
        labelContent = new GUIContent("Max Distance", "Distance for full sound roll off");
        SO.maxDistance = EditorGUILayout.FloatField(labelContent, Mathf.Clamp(SO.maxDistance,0,float.MaxValue));
        labelContent = new GUIContent("Min Distance", "Distance for full volume");
        SO.minDistance = EditorGUILayout.FloatField(labelContent, Mathf.Clamp(SO.minDistance, 0, float.MaxValue));

        labelContent = new GUIContent("Is static Spatial Blend", "do you wish to use static or dynamically adjusted spatial blend");
        SO.isStaticSpatial = EditorGUILayout.Toggle(labelContent, SO.isStaticSpatial);
        if (SO.isStaticSpatial == true)
        {
            labelContent = new GUIContent("Spatial Blend", "Sets maximum spatial blend. Spatial Blend sets how much this AudioSource is treated as a 3D source");
            SO.staticSpatial = EditorGUILayout.Slider(labelContent, SO.staticSpatial, 0, 1);
        }
        else
        {
            labelContent = new GUIContent("Transition Point", "Distance where sound starts transition from spatial to stereo");
            SO.transitionPoint = EditorGUILayout.FloatField(labelContent, SO.transitionPoint);
            labelContent = new GUIContent("Max Spatial Blend", "Sets maximum spatial blend. Spatial Blend sets how much this AudioSource is treated as a 3D source");
            SO.maxSpatialBlend = EditorGUILayout.Slider(labelContent, SO.maxSpatialBlend, 0, 1);
            labelContent = new GUIContent("Min Spatial Blend", "Sets minimum spatial blend. Spatial Blend sets how much this AudioSource is treated as a 3D source");
            SO.minSpatialBlend = EditorGUILayout.Slider(labelContent, SO.minSpatialBlend, 0, 1);

        }
        labelContent = new GUIContent("Roll Off Mode", "sets which audio attenuation mode to use, linear is recommended");
        SO.rolloffMode = (AudioRolloffMode)EditorGUILayout.EnumPopup(labelContent, SO.rolloffMode);

    }
}
