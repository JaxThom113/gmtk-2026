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

    protected void GoToFrame(Animator animator, AnimationClip clip, int frame)
    {
        animator.enabled = false;
        animator.Rebind();
        animator.Update(0f);
        float time = Mathf.Clamp(frame / clip.frameRate, 0f, clip.length);
        clip.SampleAnimation(animator.gameObject, time);
    }
}
