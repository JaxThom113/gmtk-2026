using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
[CreateAssetMenu(fileName = "StringSO", menuName = "ScriptableObjects/Types/StringSO")]

public class StringSO : ResetableTypeSO<string>
{
    [CollapsibleGroup("StringSO")]
    [SerializeField, TextArea(3,10)]
    private string _string;
    public string String
    {
        get { return _string; }
        set
        {
            if (_string == value)
            {
                return;
            }
            _string = value;
            onValueChanged?.Invoke(this, EventArgs.Empty);
            DelayReset();
        }
    }

    public override void ResetValue()
    {
        _string = defaultValue;
    }
}
