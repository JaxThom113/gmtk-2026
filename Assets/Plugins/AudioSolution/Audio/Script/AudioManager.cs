using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KevinCastejon.MissingFeatures;
using AYellowpaper.SerializedCollections;
using System;
using UnityEngine.Audio;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif
public class AudioManager : MonoBehaviour
{
    public AudioSettingSO setting;
    public TransformSO listener;

    public float masterVolume = 100;
    public float sfxVolume = 100;
    public float bgmVolume = 100;

    [SerializeField, Tooltip("Loaded Sounds, press Load sounds to load in sounds, do not edit the dictionary")]
    [SerializedDictionary("Reference", "AudioClipSO")]
    private SerializedDictionary<string, AudioClipSO> LoadedAudioClips;
    [SerializeField] AudioMixerGroup SFXMixerGroup;
    [SerializeField] AudioMixerGroup BGMMixerGroup;
    [SerializeField] AudioMixerGroup MasterMixerGroup;
    [SerializeField]
    private Stack<AudioObj> availableSources = new Stack<AudioObj>();
    [SerializeField]
    private GameObject audioObjPF;
    [SerializeField]
    private LoadedSoundDict loadedAudio;

    
    [HideInInspector] public float maxDistance;
    [HideInInspector] public float minDistance;
    [HideInInspector] public float transitionPoint;
    [HideInInspector] public bool isStaticSpatial;
    [HideInInspector] public float staticSpatial;
    [HideInInspector] public float maxSpatialBlend;
    [HideInInspector] public float minSpatialBlend;
    [HideInInspector] public AudioRolloffMode rolloffMode;

    public static AudioManager Instance;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        if(this != Instance)
        {
            DestroyImmediate(gameObject);
        }
    }
    private void Start()
    {
        UpdateDict(loadedAudio);
    }
#if UNITY_EDITOR
    
    public void LoadSounds(AudioClipSO[] newList)
    {
        string[] guids = AssetDatabase.FindAssets("t:LoadedSoundDict", new[] { setting.SoundLibPath });
        var path = AssetDatabase.GUIDToAssetPath(guids[0]);
        LoadedSoundDict loadedSO = AssetDatabase.LoadAssetAtPath<LoadedSoundDict>(path);
        loadedSO.loadedSound.Clear();
        foreach (AudioClipSO clip in newList)
        {
            loadedSO.loadedSound.Add(clip.ReferenceName, clip);
        }
        UpdateDict(loadedSO);
        EditorUtility.SetDirty(loadedSO);
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        UpdateScripts();
    }

    private static void UpdateScripts()
    {
        string[] guids = AssetDatabase.FindAssets("t:prefab AudioManager");
        if (guids.Length == 0)
            return;
        string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        GameObject contentsRoot = PrefabUtility.LoadPrefabContents(assetPath);
        AudioManager temp = contentsRoot.GetComponentInChildren<AudioManager>();
        if (temp.setting != null)
        {
            PrefabUtility.UnloadPrefabContents(contentsRoot);
            return;
        }
        temp.setting = Resources.Load<AudioSettingSO>("AudioSetting");
        PrefabUtility.SaveAsPrefabAsset(contentsRoot, assetPath);
        PrefabUtility.UnloadPrefabContents(contentsRoot);
    }
#endif
    private void UpdateDict(LoadedSoundDict loadedSO)
    {
        LoadedAudioClips.Clear();
        foreach(KeyValuePair<string,AudioClipSO> obj in loadedSO.loadedSound)
        {
            LoadedAudioClips.Add(obj.Key, obj.Value);
        }
    }
    public AudioObj PlaySound(string reference, bool loop = false, float volume = 1, Transform parent = null)
    {
        AudioClipSO SO = loadedAudio.loadedSound[reference];
        AudioClip clipToPlay = SO.clips[UnityEngine.Random.Range(0,SO.clips.Count)];
        AudioObj temp;
        if (availableSources.Count > 0)
        {
            temp = availableSources.Pop();            
        }
        else
        {
            temp = Instantiate(audioObjPF, transform).GetComponent<AudioObj>();
            temp.Init(this);
        }
            temp.StartPlaying(clipToPlay, SO.mixGroup, loop, volume);
        if (setting.IsAudio3D)
        {
            if (!isStaticSpatial)
                temp.Set3DValues(parent, SO.maxDistance, SO.minDistance, SO.transitionPoint, SO.maxSpatialBlend, SO.minSpatialBlend, SO.rolloffMode);
            else
                temp.Set3DValues(parent, SO.maxDistance, SO.minDistance, SO.staticSpatial, SO.rolloffMode);
        }
        return temp;
    }
    /// <summary>
    /// Play clip based on AudioRef
    /// </summary>
    /// <param name="reference"></param>
    /// <param name="loop"></param>
    /// <param name="volume"></param>
    /// <returns></returns>
    public AudioObj PlaySound(AudioRef reference, bool loop = false, float volume = 1)
    {
        return PlaySound(reference.ToString(), loop, volume, null);
    }


    /// <summary>
    /// Play clip based on AudioRef
    /// </summary>
    /// <param name="reference"></param>
    /// <param name="parent"></param>
    /// <param name="loop"></param>
    /// <param name="volume"></param>
    /// <returns></returns>
    public AudioObj PlaySound(AudioRef reference, Transform parent, bool loop = false, float volume = 1)
    {
        return PlaySound(reference.ToString(), loop, volume, parent);
    }
    public void ReAddToStack(AudioObj obj)
    {
        availableSources.Push(obj);
    }

    private float LinearToDecibel(float linear)
    {
        if (linear != 0)
            return 20.0f * Mathf.Log10(linear);
        else
            return -80.0f;
    }

    public void ModifyBGMVolume(float volume)
    {
        bgmVolume = volume;
        BGMMixerGroup.audioMixer.SetFloat("BGM_Volume", LinearToDecibel(bgmVolume / 100f));
    }

    public void ModifyMasterVolume(float volume)
    {
        masterVolume = volume;
        MasterMixerGroup.audioMixer.SetFloat("Master_Volume", LinearToDecibel(masterVolume / 100f));
    }

    public void ModifySFXVolume(float volume)
    {
        sfxVolume = volume;
        SFXMixerGroup.audioMixer.SetFloat("SFX_Volume", LinearToDecibel(sfxVolume / 100f));
    }

    public LoadedSoundDict GetDict()
    {
        return loadedAudio;
    }
}
