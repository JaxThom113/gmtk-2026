using Sezylrin.SimplePooling;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour, IHealth
{
    protected Transform player;
    [field: SerializeField]
    public float CurrentHealth { get; set; }
    [field: SerializeField]
    public float MaxHealth { get; set; }

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
            Pooler.PoolObject(gameObject);
            //Die();
        }
    }
}