using System;
using UnityEngine;

public class RailgunBehaviour : RangedWeapon
{
    [SerializeField]
    private Timer startupTimer;
    [SerializeField]
    private float chargeUpDur;
    private PlayerController playerController;
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
    }

    private void fireWeapon(object sender, EventArgs e)
    {
        ShootBullet(playerController.GetAttackDir());
    }
}
