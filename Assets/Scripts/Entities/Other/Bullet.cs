using Sezylrin.SimplePooling;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed;

    private Rigidbody rb;

    [SerializeField]
    private Timer despawnTimer;
    [SerializeField]
    private float despawnTime;
    [SerializeField]
    private IntSO adjustHealthSO;
    private int dmg;

    [SerializeField]
    protected BoolSO timeSlowedActive;
    public void Initialise(int damage)
    {
        dmg = damage;
        rb = GetComponent<Rigidbody>();
        despawnTimer.GenerateTimer();
        despawnTimer.SetTime(despawnTime);
        despawnTimer.SubscribeToTimerIsZero(
            
            (object sender, EventArgs e) =>
            {
                PoolObject();
            }
        );
        timeSlowedActive.onValueChanged += SlowDown;
    }

    private void SlowDown(object sender, EventArgs e)
    {
        if (timeSlowedActive.Bool)
        {
            rb.linearVelocity = transform.up * speed * 0.5f;
        }
        else
        {
            rb.linearVelocity = transform.up * speed;
        }
    }
    public void ResetObj()
    {
        rb.linearVelocity = transform.up * speed;
        despawnTimer.RestartTimer();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // ignore collisions with other enemies
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            return;

        // deal damage to the player before destroying bullet object
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
            adjustHealthSO.Int = -dmg;

        PoolObject();
    }

    private void PoolObject()
    {
        if (!gameObject.activeSelf)
            return;
        despawnTimer.StopAll();
        Pooler.PoolObject(gameObject);
    }
}