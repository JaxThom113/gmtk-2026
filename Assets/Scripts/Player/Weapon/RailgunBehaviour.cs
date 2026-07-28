using System;
using UnityEngine;
using UnityEngine.Rendering;

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
    }
    AudioObj obj;
    public void SetPlayerController(PlayerController playerController)
    {
        this.playerController = playerController;
    }

    public override void Attack(Vector3 attackDir)
    {
        startupTimer.RestartTimer();
        charging.Play(true);
        anim.Play("ChargeUp");
        obj = AudioManager.Instance.PlaySound(AudioRef.RailgunCharge, volume: volume);
    }

    private void fireWeapon(object sender, EventArgs e)
    {
        obj.StopSound(true, 0.2f);
        AudioManager.Instance.PlaySound(AudioRef.RailgunFire, volume: volume);
        anim.Play("Attack");
        charging.Stop();
        Vector3 dir = playerController.GetAttackDir();
        dir.y = 0;
        dir.Normalize();
        ShootBullet(dir);
    }
}
