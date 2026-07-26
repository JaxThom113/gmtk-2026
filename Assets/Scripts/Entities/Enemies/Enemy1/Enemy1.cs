using System.Collections;
using UnityEngine;

public class Enemy1 : Enemy
{
    [Header("Enemy1 Attack")]
    [SerializeField] private float attackRange;
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private float attackStepDelay = 0.05f;
    [SerializeField] private SphereCollider attackCollider;

    [Header("Enemy1 Animations")]
    public AnimationClip runAnim;
    public AnimationClip attackAnimation;
    [Header("vfx")]
    [SerializeField]
    ParticleSystem impact;

    private bool isCooldown;
    private bool isAttacking;
    private float attackTime;
    private Quaternion attackLockRot;

    public override void Initialize(Transform playerTransform)
    {
        base.Initialize(playerTransform);
        attackCollider.enabled = false;
    }

    public override void ResetObj()
    {
        base.ResetObj();
        isCooldown = false;
        isAttacking = false;
        attackTime = 0f;
        attackCollider.enabled = false;
    }

    protected override void FixedUpdate()
    {
        if (isDead)
            return;
        if(isFrozen) return;
        if (rb == null || player == null)
            return;

        SeparateFromNearbyEnemies();

        if (isAttacking)
        {
            rb.MoveRotation(attackLockRot);
            Move();
            return;
        }

        Vector3 e = transform.eulerAngles;
        rb.MoveRotation(Quaternion.Euler(0f, e.y, 0f));

        FacePlayer();
        Move();
        AttemptAttack();
    }
    private void AttemptAttack()
    {
        if (isAttacking || isCooldown)
            return;
        float playerDistance = Vector3.Distance(transform.position, player.position);
        if (playerDistance <= attackRange)
        {
            isAttacking = true;
            attackTime = 0f;
            attackLockRot = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            StartCoroutine(AttackRoutine());
        }
    }
    protected override void FacePlayer()
    {
        if (isCooldown || isAttacking)
            return;

        Vector3 flat = player.position - transform.position;
        flat.y = 0f;
        if (flat.sqrMagnitude < 0.001f)
            return;

        playerDir = flat.normalized;
        rb.MoveRotation(Quaternion.LookRotation(playerDir));
    }

    protected override void TakeStep(float playerDistance)
    {
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude > 0.001f)
            playerDir = toPlayer.normalized;

        timer.RestartTimer();
        if (isCooldown || isAttacking)
        {
            return;
        }

        /*if (isAttacking)
        {
            StartCoroutine(AttackRoutine());
            return;
        }*/

        

        float step = Mathf.Min(stepSize, playerDistance - attackRange);
        if (step > 0f)
        {
            stepPos = transform.position + playerDir * step;
            TryStepTo(stepPos);
        }

        attackCollider.enabled = false;
        if (runAnim != null)
        {
            lastClip = null;
            PlaySynced(runAnim);
        }
    }

    private IEnumerator AttackRoutine()
    {
        attackLockRot = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        PlayClip(attackAnimation);


        float end = Time.time + attackAnimation.length;
        while (Time.time < end)
        {
            rb.MoveRotation(attackLockRot);
            yield return null;
        }

        isAttacking = false;
        attackTime = 0f;
        StartCoroutine(AttackCooldown());
    }

    protected override AnimationClip PickClip(float distanceFromPlayer)
    {
        if (isAttacking || distanceFromPlayer <= attackRange)
            return attackAnimation;
        return runAnim;
    }

    private IEnumerator AttackCooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(cooldown);
        isCooldown = false;
    }

    public void TriggerAttack()
    {
        attackCollider.enabled = true;
        impact.Play();

    }

    public void StopAttack()
    {
        attackCollider.enabled = false;
    }
    public void DoDamage()
    {
        playerTimeAdjustment.Int = -damage;
    }
}
