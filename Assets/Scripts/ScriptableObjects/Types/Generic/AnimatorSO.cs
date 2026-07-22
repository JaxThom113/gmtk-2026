using System;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimatorSO", menuName = "ScriptableObjects/Types/AnimatorSO")]
public class AnimatorSO : ResetableTypeSO<Animator>
{
    [CollapsibleGroup("AnimatorSO")]
    private Animator _animator;
    public Animator Animator
    {
        get { return _animator; }
        set
        {
            if (_animator == value)
            {
                return;
            }
            _animator = value;
            onValueChanged?.Invoke(this, EventArgs.Empty);
            DelayReset();
        }
    }

    public override void ResetValue()
    {
        _animator = null;
    }
}
