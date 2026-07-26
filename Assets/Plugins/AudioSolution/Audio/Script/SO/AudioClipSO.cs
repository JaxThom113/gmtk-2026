using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
[CreateAssetMenu(fileName = "AudioClipObj", menuName = "ScriptableObjects/Audio/AudioClipSO")]

public class AudioClipSO : ScriptableObject
{
    // Start is called before the first frame update
    public string ReferenceName;
    public List<AudioClip> clips;
    public AudioMixerGroup mixGroup;

    [HideInInspector]public AudioSettingSO setting;
    [HideInInspector]
    public bool notDefault;
    [HideInInspector] public float maxDistance;
    [HideInInspector] public float minDistance;
    [HideInInspector] public float transitionPoint;
    [HideInInspector] public bool isStaticSpatial;
    [HideInInspector] public float staticSpatial;
    [HideInInspector] public float maxSpatialBlend;
    [HideInInspector] public float minSpatialBlend;
    [HideInInspector] public AudioRolloffMode rolloffMode;

    [HideInInspector] public bool defaultSet; 
    #if UNITY_EDITOR 
    public void SetDefaultSetting()
    {
        EditorUtility.SetDirty(this);
        if (notDefault)
            return;
        defaultSet = true;
        maxDistance = setting.maxDistance;
        minDistance = setting.minDistance;
        transitionPoint = setting.transitionPoint;
        isStaticSpatial = setting.isStaticSpatial;
        staticSpatial = setting.staticSpatial;
        maxSpatialBlend = setting.maxSpatialBlend;
        minSpatialBlend = setting.minSpatialBlend;
        rolloffMode = setting.rolloffMode;
    }
    #endif
}
