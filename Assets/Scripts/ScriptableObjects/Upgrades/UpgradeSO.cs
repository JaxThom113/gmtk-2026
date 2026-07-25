using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UpgradeType
{
    Weapon,
    Ability,
    Passive
}

[CreateAssetMenu(fileName = "UpgradeSO", menuName = "ScriptableObjects/Upgrades/UpgradeSO")]
public class UpgradeSO : ScriptableObject
{
    [Header("Upgrade Data")]
    public int level;
    public string title;
    public UpgradeType type;
    public Sprite artwork;
    [TextArea(3, 6)]
    public string description;
    public UpgradeSO nextLevel;
}