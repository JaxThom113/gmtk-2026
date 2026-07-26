using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveSO", menuName = "ScriptableObjects/Game/WaveSO")]
public class WaveSO : ScriptableObject
{
    [Header("Wave Data")]
    public List<SpawnSO> spawns;
}