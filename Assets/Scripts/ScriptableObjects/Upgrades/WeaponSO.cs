using UnityEngine;

[CreateAssetMenu(fileName = "WeaponSO", menuName = "ScriptableObjects/Upgrades/WeaponSO")]
public class WeaponSO : UpgradeSO
{
    public GameObject weaponPF;
    public WeaponBaseSO weaponSO;
    public Sprite icon; // only wepaons have sprites to display in hotbar
}
