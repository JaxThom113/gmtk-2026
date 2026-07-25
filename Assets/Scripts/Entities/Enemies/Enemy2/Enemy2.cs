using System.Collections;
using Sezylrin.SimplePooling;
using UnityEngine;

public class Enemy2 : Enemy
{
    [Header("Enemy2 Ranges")]
    [SerializeField] private float startShootingRange = 15f;
    [SerializeField] private float fireCooldown = 0.5f;

    [Header("Gun References")]
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform shootPosL;
    [SerializeField] private Transform shootPosR;

    [Header("Enemy2 Animations")]
    public AnimationClip walkAnim;
    public AnimationClip fireAnim;

    [Header("MuzzleFlash")]
    [SerializeField]
    private ParticleSystem muzzleL;
    [SerializeField]
    private ParticleSystem muzzleR;

    private float nextFireTime;
    private bool isFiring;

    protected override void FixedUpdate()
    {        

        base.FixedUpdate();
        TryFire();
    }

    protected override IEnumerator TakeStep(float playerDistance)
    {
        playerDir = (player.position - transform.position).normalized;

        if (playerDistance > stopDistance)
        {
            float step = Mathf.Min(stepSize, playerDistance - stopDistance);
            stepPos = transform.position + playerDir * step;
            rb.MovePosition(stepPos);
        }

        if (!isFiring && walkAnim != null)
        {
            lastClip = null;
            PlaySynced(walkAnim);
        }

        yield return new WaitForSeconds(stepDelay);
        stepping = false;
    }

    protected override AnimationClip PickClip(float distanceFromPlayer)
    {
        if (isFiring || distanceFromPlayer <= startShootingRange)
            return fireAnim;
        return walkAnim;
    }

    private void TryFire()
    {
        if (player == null || bullet == null || Time.time < nextFireTime)
            return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > startShootingRange)
            return;

        nextFireTime = Time.time + fireCooldown;
        StartCoroutine(FireRoutine());
    }

    private IEnumerator FireRoutine()
    {
        isFiring = true;
        if (fireAnim != null)
            PlayClip(fireAnim);

        Fire(shootPosL);
        Fire(shootPosR);

        float hold = fireAnim != null ? fireAnim.length : 0f;
        if (hold > 0f)
            yield return new WaitForSeconds(hold);
        else
            yield return null;

        isFiring = false;
    }

    private void Fire(Transform muzzle)
    {
        if (muzzle == null)
            return;

        muzzleL.Play(true);
        muzzleR.Play(true);
        Pooler.GetObject<Bullet>(bullet, muzzle.position, muzzle.rotation,
            onNewInstance: (b) => b.Initialise(damage),
            onGet: (b) => b.ResetObj());
    }
}
