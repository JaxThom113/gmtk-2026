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

    protected Vector3 playerDir;
    protected Vector3 stepPos;
    protected bool stepping;

    [Header("Animation")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected float stepsPerCycle = 4f;
    [SerializeField]
    protected Timer timer;

    [Header("sounds")]
    [SerializeField] protected float volume;

    protected AnimationClip lastClip;

    protected bool isDead = false;

    protected float defaultStepSize;
    public virtual void ResetObj()
    {
        CurrentHealth = MaxHealth;
        isDead = false; 
        animator.enabled = true;
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
        AudioManager.Instance.PlaySound(AudioRef.Hit, volume: volume);
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
        FacePlayer();
        Move();
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

        if (playerDistance > stopDistance)
        {
            float step = Mathf.Min(stepSize, playerDistance - stopDistance);
            stepPos = transform.position + playerDir * step;
            rb.MovePosition(stepPos);
        }

        PlaySynced(PickClip(playerDistance));
        timer.RestartTimer();
    }
    
    protected void StopStepping(object sender, EventArgs e)
    {
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

    public void TakeKnockback(Vector3 dir, float amount)
    {
        if(gameObject.activeSelf)
            rb.DOMove(dir * amount + rb.position,0.25f).SetEase(Ease.OutCubic);
    }
}
