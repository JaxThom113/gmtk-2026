using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "AudioSetting", menuName = "ScriptableObjects/Audio/AudioSetting")]

[Serializable]
public class AudioSettingSO : ScriptableObject
{
    public bool IsAudio3D;
    public string AudioRefPath;
    public string SoundLibPath;
    public string AudioClipSOPath;

    [HideInInspector] public float maxDistance;
    [HideInInspector] public float minDistance;
    [HideInInspector] public float transitionPoint;
    [HideInInspector] public bool isStaticSpatial;
    [HideInInspector] public float staticSpatial;
    [HideInInspector] public float maxSpatialBlend;
    [HideInInspector] public float minSpatialBlend;
    [HideInInspector] public AudioRolloffMode rolloffMode;
}
