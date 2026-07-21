using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface ITypeSO<T>
{
    public void ResetValue();
    public T defaultValue {  get; set; }
}

public interface ITypeCanReset 
{
    public bool ShouldReset { get; set; }
    public void ResetValue();
}
