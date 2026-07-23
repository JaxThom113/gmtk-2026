using UnityEngine;

public class ShotgunBehaviour : RangedWeapon
{
    [SerializeField]
    private int projectiles;
    [SerializeField]
    private int spread;

    public override void Attack(Vector3 attackDir)
    {
        Vector2 initial = new Vector2(attackDir.x, attackDir.z).normalized;
        Quaternion initialRot = Quaternion.Euler(0, 0, (float)projectiles * 0.5f * spread);
        initial = initialRot * initial;
        for (int i = 0; i < projectiles; i++)
        {
            ShootBullet(new Vector3(initial.x,0,initial.y));
            initial = Quaternion.Euler(0, 0, -spread) * initial;
        }
    }
}
