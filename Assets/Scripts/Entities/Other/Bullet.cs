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
    }

    public void ResetObj()
    {
        rb.linearVelocity = transform.up * speed;
        despawnTimer.RestartTimer();
    }

    private void OnCollisionEnter(Collision collision)
    {
        adjustHealthSO.Int = dmg;
        PoolObject();
        // delete the bullet instance
        //Destroy(gameObject);
    }

    private void PoolObject()
    {
        despawnTimer.StopAll();
        Pooler.PoolObject(gameObject);

    }
}