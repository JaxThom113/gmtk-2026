using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
[CreateAssetMenu(fileName = "EventSO", menuName = "ScriptableObjects/Types/EventSO")]

public class EventSO : BaseTypeSO
{
    public EventHandler onEventTrigger { get; set; }
    public EventHandler onEventOver { get; set; }
    public void Invoke()
    {
        onEventTrigger?.Invoke(this, EventArgs.Empty);
        onEventOver?.Invoke(this, EventArgs.Empty);
    }
}
