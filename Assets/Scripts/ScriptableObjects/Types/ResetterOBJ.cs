using AYellowpaper.SerializedCollections;
using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
[CreateAssetMenu(fileName = "ResetterSO", menuName = "ScriptableObjects/Types/ResetterSO")]

public class ResetterOBJ : ScriptableObject
{
    public List<ScriptableObject> ScriptableObjectsToReset = new List<ScriptableObject>();

#if UNITY_EDITOR
    [ContextMenu("Reset Objects")]
    public void ResetObjectsContext()
    {
        ResetObjects();
    }
    public static ResetterOBJ instance;
    [ContextMenu("Get Instance")]
    public void GetInstance()
    {
        ResetterOBJInitializer.GetInstance();
    }
    [ContextMenu("Check Instance")]
    public void Check()
    {
        Debug.Log(instance);
    }
    [InitializeOnLoad]
    public static class ResetterOBJInitializer
    {
        static ResetterOBJInitializer()
        {
            GetInstance();
        }
        public static void GetInstance()
        {
            Debug.Log("Domain reload complete! This runs inside the Editor.");
            // 1. Search for any asset of this exact type
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(ResetterOBJ).Name}");

            int count = 0;
            ResetterOBJ result = null;
            string foundPath = "Unknown";

            List<string> paths = new List<string>();
            foreach (string guid in guids)
            {
                // 2. Convert GUID to Path
                string path = AssetDatabase.GUIDToAssetPath(guid);

                // 3. Load the asset
                ResetterOBJ candidate = AssetDatabase.LoadAssetAtPath<ResetterOBJ>(path);

                if (candidate != null)
                {
                    count++;

                    // If we already have one, we have duplicates!
                    if (result != null && !ReferenceEquals(result, candidate))
                    {
                        paths.Add(path);
                    }

                    result = candidate;
                    foundPath = path;
                }
            }

            switch (count)
            {
                case 0:
                    Debug.LogWarningFormat(
                        "No ScriptableObject of type '{0}' found in project.",
                        typeof(ResetterOBJ).Name
                    );
                    break;
                case 1:
                    break;
                default:
                    string duplicatePaths = string.Join("\n", paths);
                    Debug.LogWarningFormat(
                        "Multiple ScriptableObjects of type '{0}' found in project. Files located at \n" + duplicatePaths,
                        typeof(ResetterOBJ).Name
                    );
                    break;
            }

            ResetterOBJ.instance = result;
        }
    }
    [InitializeOnLoad]
    public static class PlayModeStateChanged
    {
        // register an event handler when the class is initialized
        static PlayModeStateChanged()
        {
            EditorApplication.playModeStateChanged += LogPlayModeState;
        }

        private static void LogPlayModeState(PlayModeStateChange state)
        {
            if (state.Equals(PlayModeStateChange.EnteredEditMode))
            {
                ResetterOBJ.ResetObjects();
            }
        }
    }
    public static void ResetObjects()
    {
        string assetPath = "Assets/ScriptableObject/Types/ResetterSO.asset";

        ResetterOBJ contentsRoot = AssetDatabase.LoadAssetAtPath<ResetterOBJ>(assetPath) as ResetterOBJ;
        if (contentsRoot == null)
        {
            Debug.Log("Resetter Obj not found, if location changed please change string path");
            return;
        }
        foreach (ITypeCanReset obj in contentsRoot.ScriptableObjectsToReset)
        {
            if(obj == null)
            {
                Debug.Log("Ressetter Broke, failed to reset");
                return;
            }
            obj.ResetValue();
        }
        //contentsRoot.GenerateCSV();
        Debug.Log("Resetted Object");
        AssetDatabase.SaveAssets();
    }

    private string filename = "";

    [ContextMenu("GenerateCSV")]
    public void GenerateCSV()
    {
        filename = Application.dataPath + "/SOToReset.csv";
        WriteCSV();
    }
    private void WriteCSV()
    {
        if (ScriptableObjectsToReset.Count <= 0)
            return;
        TextWriter tw = new StreamWriter(filename, false);
        tw.WriteLine("Slot,Description");
        for(int i = 0; i < ScriptableObjectsToReset.Count; i++) 
        {
            string final =  i.ToString() + ",";
            final += ScriptableObjectsToReset[i].name;
            tw.WriteLine(final);
        }
        tw.Close();
        Debug.Log("CSV Generated");
    }
#endif


}
