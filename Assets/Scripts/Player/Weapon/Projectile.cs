using Sezylrin.SimplePooling;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private Timer despawnTimer;
    [SerializeField]
    private List<TrailRenderer> trail = new List<TrailRenderer>();

    private int pierce;
    private float damage;
    public void Initialize()
    {
        despawnTimer.GenerateTimer();
        despawnTimer.SubscribeToTimerIsZero((object sender, EventArgs e) => PoolSelf());
    }
    public void ResetObj(Vector3 dir, int pierce, float projectileLifeTime, float projectileSpeed, float damage)
    {
        if(trail != null)
        {
            foreach (TrailRenderer trail in trail)
                trail.Clear();
        }
        this.damage = damage;
        this.pierce = pierce;
        rb.linearVelocity = dir * projectileSpeed;
        despawnTimer.SetTime(projectileLifeTime);
    }
    private List<Collider> hits = new List<Collider>();
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.TryGetComponent(out IHealth health))
        {
            if (!hits.Contains(other))
            {
                hits.Add(other);
                health.TakeDamage(damage);
                if(hits.Count >= pierce)
                    PoolSelf();
            }
        }
    }
    private void PoolSelf()
    {
        hits.Clear();
        despawnTimer.StopAll();
        Pooler.PoolObject(gameObject);
    }
}
