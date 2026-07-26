using KevinCastejon.MissingFeatures.MissingAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TypeSO<T> : BaseTypeSO
{

    public virtual EventHandler onValueChanged { get; set; }
    public virtual EventHandler onValueUpdated { get; set; }
    protected bool delayReset; 
    #if UNITY_EDITOR
    protected override void OnValidate()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.Log("Manual value changed and invoked");
            onValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnDisable()
    {
        if (EditorApplication.isPlaying)
            Debug.Log(name + " OnDisabled Triggered");
        onValueChanged = null;
    }

    #endif
}

public class BaseTypeSO : ScriptableObject
{
    [CollapsibleGroup("Description",100), TextArea(3,10)]
    public string Descirption;

    protected virtual void OnValidate()
    {

    }
#if UNITY_EDITOR
    public virtual void OnFileDelete()
    {
        OnValidate();
    }
#endif
}

public class ResetableTypeSO<T> : TypeSO<T>, ITypeSO<T>, ITypeCanReset
{
    public bool ShouldReset { get => _shouldReset ; set => _shouldReset = value ; }
    [CollapsibleGroup("Reset Value", 99), SerializeField]
    [Tooltip("If true, the value will be reset to default value when play mode ends")]
    private bool _shouldReset;
    public T defaultValue { get => _defaultValue; set => _defaultValue = value; }
    [SerializeField, ShowPropIf("_shouldReset")]
    private T _defaultValue;
    /// <summary>
    /// Reset value without invoking event
    /// </summary>
    public virtual void ResetValue()
    {


    }

    public void ResetListeners()
    {
        onValueChanged = null;
        onValueUpdated = null;
    }
    /// <summary>
    /// Reset value after invoke is called, only works if called during the same invoke
    /// </summary>
    public void ResetValueDelay()
    {
        delayReset = true;
    }

    protected void DelayReset()
    {
        if (delayReset)
        {
            ResetValue();
            delayReset = false;
        }
    }
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        string assetPath = AssetDatabase.GetAssetPath(ResetterOBJ.instance);
        ResetterOBJ contentsRoot = AssetDatabase.LoadAssetAtPath<ResetterOBJ>(assetPath) as ResetterOBJ;
        if (contentsRoot == null)
            return;
        if (ShouldReset)
        {
            if (!contentsRoot.ScriptableObjectsToReset.Contains(this))
            {
                contentsRoot.ScriptableObjectsToReset.Add(this);
            }
        }
        else
        {
            contentsRoot.ScriptableObjectsToReset.Remove(this);
        }

        EditorUtility.SetDirty(contentsRoot);
    }

    public override void OnFileDelete()
    {
        ShouldReset = false;
        base.OnFileDelete();
    }
#endif
}
