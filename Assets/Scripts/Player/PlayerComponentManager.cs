using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerComponentManager : MonoBehaviour
{
    [field:SerializeField]
    public InputController input { get; private set; }
    [field: SerializeField]
    public PlayerController control { get; private set; }
    [field: SerializeField]
    public PlayerAnim anim { get; private set; }
    [field: SerializeField]
    public PlayerTimers timer { get; private set; }
    [field:SerializeField]
    public PlayerUnlocks unlocks { get; private set; }
    [field: SerializeField]
    public PlayerAbilities abilities { get; private set; }
    
}
