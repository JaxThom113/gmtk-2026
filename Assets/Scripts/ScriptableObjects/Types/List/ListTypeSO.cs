using KevinCastejon.MissingFeatures.MissingAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ListTypeSO<T> : TypeSO<T>, ITypeCanReset, IEnumerable<T>
{
    [CollapsibleGroup("List")]
    [SerializeField]
    protected List<T> list = new List<T>();
    public int Count { get { return list.Count; } }
    /// <summary>
    /// Does not invoke
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public T this[int key]
    {
        get { return list[key]; }
        set { list[key] = value; }
    }
    public void Add(T item)
    {
        list.Add(item);
        ValueChanged();
    }
    public void AddInvokeless(T item)
    {
        list.Add(item);
    }
    public void Remove(T item)
    {
        list.Remove(item);
        ValueChanged();
    }
    public void Clear()
    {
        list.Clear();
        ValueChanged();
    }
    public bool Contains(T item)
    {
        return list.Contains(item);
    }
    public void RemoveAt(int index)
    {
        list.RemoveAt(index);
        ValueChanged();
    }
    public void ValueChanged()
    {
        onValueChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Copy(List<T> list)
    {
        this.list = list;
    }
    public void Copy(T[] list)
    {
        this.list = list.ToList();
    }

    public void CopyInvoke(T[] list)
    {
        Copy(list);
        ValueChanged();
    }

    public List<T> GetList()
    {
        return list;
    }

    public bool ShouldReset { get => _shouldReset; 
        set { 
            _shouldReset = value;
            if (!value)
            {
                _clearOnReset = true;
            }
        } }
    [CollapsibleGroup("Reset Value", 99), SerializeField]
    [Tooltip("If true, the list will be reset when play mode ends")]
    protected bool _shouldReset;

    [SerializeField, ShowPropIf("_shouldReset")]
    [Tooltip("If true, the list will be cleared when play mode ends. If false, the list will be reset to the default value.")]
    public bool _clearOnReset;
    [SerializeField]
    protected List<T> _defaultValue;
    public void ResetValue()
    {
        if (_clearOnReset)
            list.Clear();
        else
        {
            list = new List<T>(_defaultValue);
        }


    }
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        string assetPath = "Assets/ScriptableObject/Types/ResetterSO.asset";
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
#endif
    public IEnumerator<T> GetEnumerator()
    {
        return ((IEnumerable<T>)list).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)list).GetEnumerator();
    }

    public void ResetListeners()
    {
        onValueChanged = null;
        onValueUpdated = null;
    }
}
