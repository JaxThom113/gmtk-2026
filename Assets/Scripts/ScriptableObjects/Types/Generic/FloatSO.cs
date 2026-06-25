using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "FloatSO", menuName = "ScriptableObjects/Types/FloatSO")]
public class FloatSO : ResetableTypeSO<float>
{
    [CollapsibleGroup("FloatSO")]
    [SerializeField]
    private float _float;
    public float Float
    {
        get { return _float; }
        set
        {
            if (_float == value)
            {
                return;
            }
            _float = value;
            onValueChanged?.Invoke(this, EventArgs.Empty);
            DelayReset();
        }
    }

    public override void ResetValue()
    {
        _float = defaultValue;
    }
}
