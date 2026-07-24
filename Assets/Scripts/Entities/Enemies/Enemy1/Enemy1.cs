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

    private bool isCooldown;
    private bool isAttacking;
    private float attackTime;
    private Quaternion attackLockRot;

    public override void Initialize(Transform playerTransform)
    {
        base.Initialize(playerTransform);
        attackCollider.enabled = false;

        // Physics can't spin us — only this script sets yaw
        rb.constraints = RigidbodyConstraints.FreezePositionY
            | RigidbodyConstraints.FreezeRotationX
            | RigidbodyConstraints.FreezeRotationY
            | RigidbodyConstraints.FreezeRotationZ;
    }

    protected override void FixedUpdate()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
        if (rb == null || player == null)
            return;

        rb.angularVelocity = Vector3.zero;

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

    protected override IEnumerator TakeStep(float playerDistance)
    {
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude > 0.001f)
            playerDir = toPlayer.normalized;

        if (isCooldown)
        {
            attackCollider.enabled = false;
            while (isCooldown)
                yield return null;
            stepping = false;
            yield break;
        }

        if (isAttacking)
        {
            yield return AttackRoutine();
            stepping = false;
            yield break;
        }

        if (playerDistance <= attackRange)
        {
            isAttacking = true;
            attackTime = 0f;
            attackLockRot = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            yield return AttackRoutine();
            stepping = false;
            yield break;
        }

        float step = Mathf.Min(stepSize, playerDistance - attackRange);
        if (step > 0f)
        {
            stepPos = transform.position + playerDir * step;
            stepPos.y = transform.position.y;
            if (!IsStepBlockedByEnemy1(stepPos))
                rb.MovePosition(stepPos);
        }

        attackCollider.enabled = false;
        PlaySynced(runAnim);
        yield return new WaitForSeconds(stepDelay);
        stepping = false;
    }

    private bool IsStepBlockedByEnemy1(Vector3 targetPos)
    {
        var cap = GetComponent<CapsuleCollider>();
        if (cap == null)
            return false;

        Vector3 delta = targetPos - transform.position;
        float dist = delta.magnitude;
        if (dist < 0.001f)
            return false;

        Vector3 dir = delta / dist;
        float scale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
        float radius = cap.radius * scale * 0.95f;

        Vector3 axis = Vector3.up;
        if (cap.direction == 0) axis = transform.right;
        else if (cap.direction == 2) axis = transform.forward;

        float height = Mathf.Max(0f, cap.height * transform.lossyScale.y - 2f * radius);
        Vector3 worldCenter = transform.TransformPoint(cap.center);
        Vector3 p1 = worldCenter + axis * (height * 0.5f);
        Vector3 p2 = worldCenter - axis * (height * 0.5f);

        int mask = LayerMask.GetMask("Enemy");
        if (Physics.CapsuleCast(p1, p2, radius, dir, out RaycastHit hit, dist, mask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider != null
                && hit.collider.gameObject != gameObject
                && hit.rigidbody != rb
                && hit.collider.GetComponentInParent<Enemy1>() != null)
                return true;
        }

        return false;
    }

    private IEnumerator AttackRoutine()
    {
        attackCollider.enabled = true;
        attackLockRot = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        PlayClip(attackAnimation);

        float end = Time.time + attackAnimation.length;
        while (Time.time < end)
        {
            rb.MoveRotation(attackLockRot);
            rb.angularVelocity = Vector3.zero;
            yield return null;
        }

        isAttacking = false;
        attackTime = 0f;
        attackCollider.enabled = false;
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

    private void OnTriggerEnter(Collider other)
    {
        if (!attackCollider.enabled)
            return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            Debug.Log("hit player");
    }
}
