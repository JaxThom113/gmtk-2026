using DG.Tweening;
using Sezylrin.SimplePooling;
using System;
using UnityEngine;

public class Enemy3 : Enemy
{
    [Header("Enemy3 Shooting")]
    [SerializeField] private float startShootingRange = 20f;
    [SerializeField] private float shootRate;
    [SerializeField] private float chargeShotTime;
    [SerializeField] private GameObject bulletPF;
    [SerializeField] private Transform muzzle;
    [SerializeField] private Transform aimPoint;
    [SerializeField] private Transform calculatePoint;


    [Header("Enemy3 Animations")]
    public AnimationClip moveAnim;
    public AnimationClip shootMoveAnim;
    public AnimationClip standShootAnim;

    [Header("timer")]
    [SerializeField]
    private Timer timer;
    [Header("vfx")]
    [SerializeField]
    private ParticleSystem chargeVFX;
    [SerializeField]
    private ParticleSystem muzzleVFX;

    private float nextFireTime;
    protected override void FixedUpdate()
    {
        if (isDead)
            return;
        if (timer.IsTimeZero())
        {
            FacePlayer();
            Move();
        }
            
        TryFire();
    }

    public override void ResetObj()
    {
        base.ResetObj();
        AimArm();
    }
    protected virtual void AimArm()
    {
        Vector3 playerDir = player.position - calculatePoint.position;
        playerDir.y = 0;
        playerDir.Normalize();
        aimPoint.rotation = Quaternion.LookRotation(playerDir);
    }
    public override void Initialize(Transform playerTransform)
    {
        base.Initialize(playerTransform);
        timer.GenerateTimer();
        timer.SetTime(chargeShotTime, false);
        timer.SubscribeToTimerIsZero(Fire);
    }

    private void TryFire()
    {
        if (player == null || bulletPF == null || Time.time < nextFireTime)
            return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > startShootingRange)
            return;

        nextFireTime = Time.time + shootRate + chargeShotTime;
        timer.RestartTimer();
        chargeVFX.Play(true);
        AimArm();
    }

    private void Fire(object sender, EventArgs e)
    {
        chargeVFX.Stop();
        muzzleVFX.Play(true);
        Pooler.GetObject<Bullet>(bulletPF, muzzle.position, muzzle.rotation,
            onNewInstance: (b) => b.Initialise(damage),
            onGet: (b) => b.ResetObj());

    }
    protected override AnimationClip PickClip(float distanceFromPlayer)
    {
        if (distanceFromPlayer <= stopDistance)
            return standShootAnim;
        if (distanceFromPlayer <= startShootingRange)
            return shootMoveAnim;
        return moveAnim;
    }
}
