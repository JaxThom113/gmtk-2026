using UnityEngine;

public class DuelPistolBehaviour : RangedWeapon
{
    [SerializeField]
    private Transform leftGun;
    [SerializeField] private Transform rightGun;

    private bool alternateGun = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Attack(Vector3 attackDir)
    {
        ShootBullet(attackDir);

        bulletSP = alternateGun? leftGun : rightGun;
        alternateGun = !alternateGun;

    }
}
