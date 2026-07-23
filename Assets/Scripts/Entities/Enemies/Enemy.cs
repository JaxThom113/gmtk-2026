using Sezylrin.SimplePooling;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour, IHealth
{
    [Header("core")]
    protected Transform player;
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
        // all enemies must be initialized with a reference to the player
        player = playerTransform;
        rb = GetComponent<Rigidbody>();
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
            //Die();
        }
    }
}