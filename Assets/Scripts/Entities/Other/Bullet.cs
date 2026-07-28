using DG.Tweening;
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

    [SerializeField]
    private List<ParticleSystem> particles = new List<ParticleSystem>();
    [SerializeField]
    private List<TrailRenderer> trailRenderers = new List<TrailRenderer>();
    private List<float> renderTimes = new List<float>();
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
        foreach (TrailRenderer tr in trailRenderers)
        {
            renderTimes.Add(tr.time);
        }
    }

    private void SlowDown(object sender, EventArgs e)
    {
        SlowDown();
    }

    private void SlowDown()
    {
        if (timeSlowedActive.Bool)
        {
            if (isTimeFreezeUnlocked.Bool)
            {
                rb.linearVelocity = Vector3.zero;
                foreach(TrailRenderer trailRenderer in trailRenderers)
                {
                    trailRenderer.time = 1000f;

                }
                foreach (ParticleSystem particle in particles)
                {
                    particle.Pause();
                }
                rb.linearVelocity = Vector3.zero;
            }
            else
                rb.linearVelocity = transform.forward * speed * 0.5f;
        }
        else
        {
            rb.linearVelocity = transform.forward * speed; 
            for (int i = 0; i < renderTimes.Count; i++)
            {
                trailRenderers[i].time = renderTimes[i];
            }
            foreach (ParticleSystem particle in particles)
            {
                particle.Play();
            }
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