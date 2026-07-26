using UnityEngine;

[CreateAssetMenu(fileName = "WeaponSO", menuName = "Scriptable Objects/WeaponSO")]
public class WeaponSO : UpgradeSO
{
    public GameObject weaponPF;
    public WeaponBaseSO weaponSO;
    public Sprite icon; // only wepaons have sprites to display in hotbar
}
