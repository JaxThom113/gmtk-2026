using UnityEngine;

[CreateAssetMenu(fileName = "PassiveSO", menuName = "ScriptableObjects/Upgrades/PassiveSO")]
public class PassiveSO : UpgradeSO
{
    public int increase;
    public IntSO statToIncrease;
}
