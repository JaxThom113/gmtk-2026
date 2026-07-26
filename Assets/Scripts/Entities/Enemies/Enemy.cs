using DG.Tweening;
using Sezylrin.SimplePooling;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour, IHealth
{
    [Header("core")]
    protected Transform player;
    [Header("time slow")]
    [SerializeField]
    protected BoolSO timeSlowedActive;
    [SerializeField]
    protected BoolSO timeFreezeUnlocked;
    [Header("Health")]
    [field: SerializeField]
    public float CurrentHealth { get; set; }
    [field: SerializeField]
    public float MaxHealth { get; set; }
    [SerializeField]
    protected int playerTimeIncreaseAmount;
    [SerializeField]
    protected IntSO playerTimeAdjustment;
    [SerializeField]
    protected Collider hitbox;
    [Header("Disintegrate")]
    [SerializeField]
    [ColorUsage(true,true)]
    private Color deathColor;
    [SerializeField]
    private float deathTimeDur;
    [SerializeField]
    private SkinnedMeshRenderer rend;
    [SerializeField]
    private GameObject outlineOBJ;
    [SerializeField]
    private LayerMask outlineLayer;

    private int layer;

    [Header("Spawning")]
    [SerializeField]
    [ColorUsage(true, true)]
    private Color SpawnColour; 
    [SerializeField]
    private float spawnDur;
    [SerializeField]
    private int spiralness;

    [Header("Enemy Stats")]
    [SerializeField] protected int damage;

    [Header("EXP")]
    [SerializeField]
    private GameObject expPrefab;
    [SerializeField]
    private int expAmount;

    protected Rigidbody rb;

    [Header("Movement Settings")]
    [SerializeField] protected float speed;
    [SerializeField] protected float stepSize;
    [SerializeField] protected float stopDistance;
    [SerializeField] protected float stepDelay = 1f;
    [SerializeField] protected LayerMask stepCollisionMask;
    [SerializeField] protected float separationPadding = 0.4f;
    [SerializeField] protected float separationStrength = 0.5f;

    protected Vector3 playerDir;
    protected Vector3 stepPos;
    protected bool stepping;
    protected CapsuleCollider stepCapsule;

    static readonly Collider[] SeparationHits = new Collider[24];

    [Header("Animation")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected float stepsPerCycle = 4f;
    [SerializeField]
    protected Timer timer;

    protected AnimationClip lastClip;

    protected bool isDead = false;

    protected float defaultStepSize;
    public virtual void ResetObj()
    {
        CurrentHealth = MaxHealth;
        isDead = false; 
        animator.enabled = true;
        lastClip = null;
        outlineOBJ.layer = layer;
        hitbox.enabled = true;
        SpawnIn();
        stepping = false;
        slowTime();
    }

    public virtual void Initialize(Transform playerTransform)
    {
        player = playerTransform;
        rb = GetComponent<Rigidbody>();
        stepCapsule = GetComponent<CapsuleCollider>();

        if (stepCollisionMask == 0)
            stepCollisionMask = LayerMask.GetMask("Default", "Ground", "Enemy");

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.None;
        }

        timeSlowedActive.onValueChanged -= slowTime;
        timeSlowedActive.onValueChanged += slowTime;
        layer = (int)Mathf.Log(outlineLayer.value, 2);
        timer.GenerateTimer();
        timer.SetTime(stepDelay, false);
        timer.SubscribeToTimerIsZero(StopStepping);
        defaultStepSize = stepSize;
    }

    private void SpawnIn()
    {
        outlineOBJ.layer = LayerMask.NameToLayer("Default");
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        block.SetColor("_OutlineColour", SpawnColour);
        block.SetFloat("_SpiralStrength", spiralness);
        DOVirtual.Float(0, 1.1f, spawnDur,
                onVirtualUpdate: (f) =>
                {
                    block.SetFloat("_DissolveAmount", f);
                    rend.SetPropertyBlock(block);
                }).OnComplete(() => outlineOBJ.layer = layer);
    }

    protected virtual void slowTime(object sender, EventArgs e)
    {
        slowTime();
    }
    protected bool isFrozen;
    protected virtual void slowTime()
    {
        if (timeSlowedActive.Bool)
        {
            if(timeFreezeUnlocked.Bool)
            {
                animator.speed = 0.01f;
                isFrozen = true;
            }
            else
            {
                animator.speed = 0.5f;
                stepSize *= 0.5f;
            }
        }
        else
        {
            isFrozen = false;
            animator.speed = 1;
            stepSize = defaultStepSize;
        }
    }
    public void TakeDamage(float damage)
    {
        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
        {
            hitbox.enabled = false;
            playerTimeAdjustment.Int += playerTimeIncreaseAmount;
            Pooler.GetObject<ExpOrb>(expPrefab, transform.position, Quaternion.identity,
                onGet: (e) =>
                {
                    e.ResetObj();
                    e.SetExpAmount(expAmount);
                }
                );

            isDead = true;

            outlineOBJ.layer = LayerMask.NameToLayer("Default");

            animator.enabled = false;
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetColor("_OutlineColour", deathColor);
            block.SetFloat("_SpiralStrength", 0);
            DOVirtual.Float(1.1f,0,deathTimeDur,
                onVirtualUpdate: (f) =>
                {
                    block.SetFloat("_DissolveAmount", f);
                    rend.SetPropertyBlock(block);
                }).OnComplete(() => Pooler.PoolObject(gameObject));

        }
    }

    protected virtual void FixedUpdate()
    {
        if (isDead)
            return;
        if (isFrozen)
            return;
        SeparateFromNearbyEnemies();
        FacePlayer();
        Move();
    }

    protected void SeparateFromNearbyEnemies()
    {
        if (rb == null)
            return;
        if (stepCapsule == null)
            stepCapsule = GetComponent<CapsuleCollider>();
        if (stepCapsule == null)
            return;

        float scale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
        float myRadius = stepCapsule.radius * scale;
        float height = Mathf.Max(0f, stepCapsule.height * transform.lossyScale.y - 2f * myRadius);

        Vector3 axis = Vector3.up;
        if (stepCapsule.direction == 0) axis = transform.right;
        else if (stepCapsule.direction == 2) axis = transform.forward;

        Vector3 worldCenter = transform.TransformPoint(stepCapsule.center);
        Vector3 p1 = worldCenter + axis * (height * 0.5f);
        Vector3 p2 = worldCenter - axis * (height * 0.5f);

        float queryRadius = myRadius + separationPadding;
        int enemyMask = LayerMask.GetMask("Enemy");
        int count = Physics.OverlapCapsuleNonAlloc(p1, p2, queryRadius, SeparationHits, enemyMask, QueryTriggerInteraction.Ignore);

        Vector3 push = Vector3.zero;
        for (int i = 0; i < count; i++)
        {
            var col = SeparationHits[i];
            if (col == null)
                continue;
            if (col.gameObject == gameObject || col.transform.IsChildOf(transform))
                continue;
            if (col.attachedRigidbody != null && col.attachedRigidbody == rb)
                continue;

            Vector3 otherPos = col.attachedRigidbody != null
                ? col.attachedRigidbody.position
                : col.transform.position;

            Vector3 away = transform.position - otherPos;
            away.y = 0f;
            float dist = away.magnitude;

            float otherRadius = myRadius;
            if (col is CapsuleCollider otherCap)
            {
                float otherScale = Mathf.Max(col.transform.lossyScale.x, col.transform.lossyScale.z);
                otherRadius = otherCap.radius * otherScale;
            }

            float desired = myRadius + otherRadius + separationPadding;
            if (dist >= desired)
                continue;

            if (dist < 0.001f)
            {
                float angle = (GetInstanceID() & 1023) * 0.01f;
                away = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                dist = 0.001f;
            }

            float overlap = desired - dist;
            push += (away / dist) * (overlap * 0.5f * separationStrength);
        }

        push.y = 0f;
        if (push.sqrMagnitude < 0.0001f)
            return;

        push = Vector3.ClampMagnitude(push, myRadius * 0.35f);

        Vector3 target = transform.position + push;
        target.y = transform.position.y;

        if (IsSeparationBlockedByEnvironment(target))
            return;

        rb.MovePosition(target);
    }

    bool IsSeparationBlockedByEnvironment(Vector3 targetPos)
    {
        if (stepCapsule == null)
            return false;

        Vector3 delta = targetPos - transform.position;
        delta.y = 0f;
        float dist = delta.magnitude;
        if (dist < 0.001f)
            return false;

        float scale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
        float radius = stepCapsule.radius * scale * 0.98f;

        Vector3 axis = Vector3.up;
        if (stepCapsule.direction == 0) axis = transform.right;
        else if (stepCapsule.direction == 2) axis = transform.forward;

        float height = Mathf.Max(0f, stepCapsule.height * transform.lossyScale.y - 2f * radius);
        Vector3 worldCenter = transform.TransformPoint(stepCapsule.center);
        Vector3 p1 = worldCenter + axis * (height * 0.5f);
        Vector3 p2 = worldCenter - axis * (height * 0.5f);

        int envMask = stepCollisionMask & ~LayerMask.GetMask("Enemy");
        if (envMask == 0)
            envMask = LayerMask.GetMask("Default", "Ground");

        Vector3 dir = delta / dist;
        if (!Physics.CapsuleCast(p1, p2, radius, dir, out RaycastHit hit, dist, envMask, QueryTriggerInteraction.Ignore))
            return false;

        return hit.normal.y <= 0.5f;
    }

    protected virtual void Move()
    {
        if (stepping || player == null || rb == null)
            return;

        float playerDistance = Vector3.Distance(transform.position, player.position);

        stepping = true;
        TakeStep(playerDistance);
    }

    protected virtual void FacePlayer()
    {
        Vector3 flat = playerDir;
        flat.y = 0f;
        if (flat.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(flat);
    }

    protected virtual void TakeStep(float playerDistance)
    {
        playerDir = (player.position - transform.position).normalized;
        playerDir.y = 0f;
        if (playerDir.sqrMagnitude > 0.001f)
            playerDir.Normalize();

        if (playerDistance > stopDistance)
        {
            float step = Mathf.Min(stepSize, playerDistance - stopDistance);
            stepPos = transform.position + playerDir * step;
            stepPos.y = transform.position.y;
            TryStepTo(stepPos);
        }

        PlaySynced(PickClip(playerDistance));
        timer.RestartTimer();
    }
    
    protected void StopStepping(object sender, EventArgs e)
    {
        stepping = false;
    }

    protected bool TryStepTo(Vector3 targetPos)
    {
        if (rb == null)
            return false;

        targetPos.y = transform.position.y;
        if (IsStepBlocked(targetPos))
            return false;

        rb.MovePosition(targetPos);
        return true;
    }

    protected bool IsStepBlocked(Vector3 targetPos)
    {
        if (stepCapsule == null)
            stepCapsule = GetComponent<CapsuleCollider>();
        if (stepCapsule == null)
            return false;

        Vector3 delta = targetPos - transform.position;
        delta.y = 0f;
        float dist = delta.magnitude;

        float scale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
        float radius = stepCapsule.radius * scale;

        Vector3 axis = Vector3.up;
        if (stepCapsule.direction == 0) axis = transform.right;
        else if (stepCapsule.direction == 2) axis = transform.forward;

        float height = Mathf.Max(0f, stepCapsule.height * transform.lossyScale.y - 2f * radius);
        Vector3 worldCenter = transform.TransformPoint(stepCapsule.center);
        Vector3 targetCenter = worldCenter + delta;
        Vector3 t1 = targetCenter + axis * (height * 0.5f);
        Vector3 t2 = targetCenter - axis * (height * 0.5f);

        int enemyMask = LayerMask.GetMask("Enemy");
        var overlaps = Physics.OverlapCapsule(t1, t2, radius * 0.98f, enemyMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < overlaps.Length; i++)
        {
            var col = overlaps[i];
            if (col == null)
                continue;
            if (col.gameObject == gameObject || col.transform.IsChildOf(transform))
                continue;
            if (col.attachedRigidbody != null && col.attachedRigidbody == rb)
                continue;
            return true;
        }

        if (dist < 0.001f)
            return false;

        Vector3 dir = delta / dist;
        Vector3 p1 = worldCenter + axis * (height * 0.5f);
        Vector3 p2 = worldCenter - axis * (height * 0.5f);

        if (!Physics.CapsuleCast(p1, p2, radius * 0.98f, dir, out RaycastHit hit, dist, stepCollisionMask, QueryTriggerInteraction.Ignore))
            return false;

        if (hit.collider == null)
            return false;
        if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            return false;
        if (hit.rigidbody != null && hit.rigidbody == rb)
            return false;
        if (hit.normal.y > 0.5f)
            return false;

        return true;
    }

    protected virtual AnimationClip PickClip(float distanceFromPlayer)
    {
        return null;
    }

    protected void PlaySynced(AnimationClip clip)
    {
        if (animator == null || clip == null)
            return;

        animator.speed = clip.length / (stepDelay * Mathf.Max(0.01f, stepsPerCycle));
        if (clip != lastClip)
        {
            lastClip = clip;
            animator.Play(clip.name, 0, 0f);
        }
    }

    protected void PlayClip(AnimationClip clip, float speed = 1f)
    {
        if (animator == null || clip == null)
            return;

        lastClip = clip;
        animator.speed = speed;
        animator.Play(clip.name, 0, 0f);
    }

    public void TakeKnockback(Vector3 dir, float amount)
    {
        if(gameObject.activeSelf)
            rb.DOMove(dir * amount + rb.position,0.25f).SetEase(Ease.OutCubic);
    }
}
