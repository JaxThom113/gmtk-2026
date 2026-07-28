using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnSO", menuName = "ScriptableObjects/Game/SpawnSO")]
public class SpawnSO : ScriptableObject
{
    [Header("Spawn Data")]
    public GameObject enemy;
    public int count;
    public int delay;
}