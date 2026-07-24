using System;
using UnityEngine;

public class RailgunBehaviour : RangedWeapon
{
    [SerializeField]
    private Timer startupTimer;
    [SerializeField]
    private float chargeUpDur;
    private PlayerController playerController;
    [SerializeField]
    private ParticleSystem charging;
    protected override void Start()
    {
        base.Start();
        startupTimer.GenerateTimer();
        startupTimer.SubscribeToTimerIsZero(fireWeapon);
        startupTimer.SetTime(chargeUpDur, false);
        playerController = gameObject.GetComponentInParent<PlayerController>();
    }

    public override void Attack(Vector3 attackDir)
    {
        startupTimer.RestartTimer();
        charging.Play(true);
        anim.Play("ChargeUp");
    }

    private void fireWeapon(object sender, EventArgs e)
    {
        anim.Play("Attack");
        charging.Stop();
        Debug.Log(playerController.GetAttackDir());
        Vector3 dir = playerController.GetAttackDir();
        dir.y = 0;
        dir.Normalize();
        ShootBullet(dir);
    }
}
