using System;
using UnityEngine;

public class DuelPistolBehaviour : RangedWeapon
{
    [SerializeField]
    private Transform leftGun;
    [SerializeField] 
    private Transform rightGun;
    [SerializeField]
    private ParticleSystem particleL, particleR;

    private bool alternateGun = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void Attack(Vector3 attackDir)
    {
        attackDir.y = 0;
        attackDir.Normalize();
        ShootBullet(attackDir);


        bulletSP = alternateGun? leftGun : rightGun;
        anim.Play(alternateGun ? "AttackL" : "AttackR");
        if (alternateGun)
            particleL.Play();
        else particleR.Play();
        alternateGun = !alternateGun;
        transform.localEulerAngles = Vector3.zero;
    }

    protected override void RapidMode(object sender, EventArgs e)
    {
        base.RapidMode(sender, e);
        if (isRapidActive.Bool)
        {
            var main = particleL.main;
            main.simulationSpeed = 2;
            main = particleR.main;
            main.simulationSpeed = 2;
        }
        else
        {
            var main = particleL.main;
            main.simulationSpeed = 1;
            main = particleR.main;
            main.simulationSpeed = 1;
        }
    }
}
