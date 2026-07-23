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
    public static GameObject Instance { get; private set; }

    private void Awake()
    {
        
        //enabled = false;
        if (Instance != null && Instance != this)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = gameObject;
        }
    }
}
