using System;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponBaseSO", menuName = "ScriptableObjects/Types/WeaponBaseSO")]
public class WeaponBaseSO : ResetableTypeSO<WeaponBase>
{
    [CollapsibleGroup("WeaponBaseSO")]
    [SerializeField]
    private WeaponBase _weaponBase;
    public WeaponBase WeaponBase
    {
        get { return _weaponBase; }
        set
        {
            if (_weaponBase == value)
            {
                return;
            }
            _weaponBase = value;
            onValueChanged?.Invoke(this, EventArgs.Empty);
            DelayReset();
        }
    }

    public override void ResetValue()
    {
        _weaponBase = null;
    }
}
