using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSO", menuName = "ScriptableObjects/Game/GameSO")]
public class GameSO : ScriptableObject
{
    [Header("Game Data")]
    public List<WaveSO> waves;
    public int waveDelay;
}