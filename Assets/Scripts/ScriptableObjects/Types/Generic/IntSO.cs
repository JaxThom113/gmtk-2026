using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.EventSystems;
[CreateAssetMenu(fileName = "IntSO", menuName = "ScriptableObjects/Types/IntSO")]
public class IntSO : ResetableTypeSO<int>
{
    [CollapsibleGroup("IntSO")]
    [SerializeField]
    private int _int;
    public int Int { 
        get { return _int; } 
        set 
        {
            if (_int == value)
            {
                return;
            }
            _int = value;
            onValueChanged?.Invoke(this, EventArgs.Empty);
            DelayReset();
        }
    }

    public override void ResetValue()
    {
        _int = defaultValue;
    }
}
