using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AudioSetUp : MonoBehaviour
{
    public AudioSettingSO setting;
    [Header("Whether the project should be 2D or 3D audio")]
    public bool Use3D;
    [Header("AudioClipSO storage locator")]
    [Tooltip("Whether to set the storage destination via string path or automatically locate path based on where a given AudioClipSO is stored at")]
    public bool IsString;
    [HideInInspector] public string AudioClipSOLoc;
    [HideInInspector] public AudioClipSO clipLoc;

    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Is3D()
    {
        if(setting.IsAudio3D != Use3D) 
            setting.IsAudio3D = Use3D;
    }
#if UNITY_EDITOR
    public void RunSetup()
    {
        string[] guid = AssetDatabase.FindAssets("t:script AudioRef");
        string audioRef = AssetDatabase.GUIDToAssetPath(guid[0]);
        guid = AssetDatabase.FindAssets("t:LoadedSoundDict");
        string soundLib = AssetDatabase.GUIDToAssetPath(guid[0]);
        string[] guids = AssetDatabase.FindAssets("t:AudioSettingSO AudioSetting");
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        AudioSettingSO setting = AssetDatabase.LoadAssetAtPath<AudioSettingSO>(path);
        setting.AudioRefPath = audioRef.Replace("/AudioRef.cs", "");
        setting.SoundLibPath = soundLib.Replace("/LoadedSoundDictionary.asset", "");
        if(IsString)
        {
            if (AudioClipSOLoc.Equals(""))
                DefaultLoc(setting);
            else
                setting.AudioClipSOPath = AudioClipSOLoc;
        }
        else
        {
            if (clipLoc == null)
                DefaultLoc(setting);
            else
            {
                path = AssetDatabase.GetAssetPath(clipLoc);
                path = path.Replace(clipLoc.name + ".asset", "");
                setting.AudioClipSOPath = path;
            }
        }
        EditorUtility.SetDirty(setting);
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        EditorApplication.ExecuteMenuItem("File/Save Project");

    }
    private void DefaultLoc(AudioSettingSO setting)
    {
        string[] guid = AssetDatabase.FindAssets("t:AudioClipSO Example");
        string ClipPath = AssetDatabase.GUIDToAssetPath(guid[0]);
        setting.AudioClipSOPath = ClipPath.Replace("/Example.asset", "");
    }
    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        UpdateScripts();
    }

    private static void UpdateScripts()
    {
        string[] guids = AssetDatabase.FindAssets("t:prefab AudioSetup");
        if (guids.Length == 0)
            return;
        string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        GameObject contentsRoot = PrefabUtility.LoadPrefabContents(assetPath);
        AudioSetUp temp = contentsRoot.GetComponentInChildren<AudioSetUp>();
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
}
