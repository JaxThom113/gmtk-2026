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
    [SerializeField]
    protected BoolSO isTimeFreezeUnlocked;

    [SerializeField]
    protected bool despawnOnHit;
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
        SlowDown();
    }

    private void SlowDown()
    {
        if (timeSlowedActive.Bool)
        {
            Debug.Log(rb.linearVelocity.magnitude);
            rb.linearVelocity = transform.forward * speed * 0.5f;
            Debug.Log(rb.linearVelocity.magnitude);
        }
        else
        {
            rb.linearVelocity = transform.forward * speed;
        }
    }
    public void ResetObj()
    {
        SlowDown();
        despawnTimer.RestartTimer();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // ignore collisions with other enemies
        adjustHealthSO.Int = -dmg;
        if(despawnOnHit)
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