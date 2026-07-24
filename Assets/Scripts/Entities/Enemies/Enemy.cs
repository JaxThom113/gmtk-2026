using Sezylrin.SimplePooling;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour, IHealth
{
    [Header("core")]
    protected Transform player;
    [SerializeField]
    protected BoolSO timeSlowedActive;
    [Header("Health")]
    [field: SerializeField]
    public float CurrentHealth { get; set; }
    [field: SerializeField]
    public float MaxHealth { get; set; }
    [SerializeField]
    protected int playerTimeIncreaseAmount;
    [SerializeField]
    protected IntSO playerTimeAdjustment;

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

    protected Vector3 playerDir;
    protected Vector3 stepPos;
    protected bool stepping;

    [Header("Animation")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected float stepsPerCycle = 4f;

    protected AnimationClip lastClip;

    public void ResetObj()
    {
        CurrentHealth = MaxHealth;
    }

    public virtual void Initialize(Transform playerTransform)
    {
        player = playerTransform;
        rb = GetComponent<Rigidbody>();
        timeSlowedActive.onValueChanged += slowTime;
    }

    protected virtual void slowTime(object sender, EventArgs e)
    {

    }

    public void TakeDamage(float damage)
    {
        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
        {
            playerTimeAdjustment.Int += playerTimeIncreaseAmount;
            Pooler.GetObject<ExpOrb>(expPrefab, transform.position, Quaternion.identity,
                onGet: (e) =>
                {
                    e.ResetObj();
                    e.SetExpAmount(expAmount);
                }
                );
            Pooler.PoolObject(gameObject);
        }
    }

    protected virtual void FixedUpdate()
    {
        FacePlayer();
        Move();
    }

    protected virtual void Move()
    {
        if (stepping || player == null || rb == null)
            return;

        float playerDistance = Vector3.Distance(transform.position, player.position);

        stepping = true;
        StartCoroutine(TakeStep(playerDistance));
    }

    protected virtual void FacePlayer()
    {
        Vector3 flat = playerDir;
        flat.y = 0f;
        if (flat.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(flat);
    }

    protected virtual IEnumerator TakeStep(float playerDistance)
    {
        playerDir = (player.position - transform.position).normalized;

        if (playerDistance > stopDistance)
        {
            float step = Mathf.Min(stepSize, playerDistance - stopDistance);
            stepPos = transform.position + playerDir * step;
            rb.MovePosition(stepPos);
        }

        PlaySynced(PickClip(playerDistance));
        yield return new WaitForSeconds(stepDelay);
        stepping = false;
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
}
