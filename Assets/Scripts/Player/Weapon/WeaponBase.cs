using Sezylrin.SimplePooling;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    public Animator anim;
    public AnimatorSO animSO;
    [SerializeField]
    protected float damage = 10f;
    [SerializeField]
    protected float attackInterval;
    protected Vector3 attackDir;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        //animSO.Animator = anim;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchWeapon()
    {
        animSO.Animator = anim;
    }

    public float GetAttackInterval()
    {
        return attackInterval;
    }

    public virtual void Attack(Vector3 attackDir)
    {
        this.attackDir = attackDir;
    }

    public void StoreWeapon()
    {
        anim.SetBool("isStored", true);
    }

    public void ActiveWeapon()
    {
        anim.SetBool("isStored", false);
    }
}

public class MeleeWeapon : WeaponBase
{
    
    protected List<Collider> hitColliders = new List<Collider>();
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IHealth health))
        {
            if(hitColliders.Contains(other))
                return;
            hitColliders.Add(other);
            health.TakeDamage(damage);
        }
    }
}

public class RangedWeapon : WeaponBase
{
    [SerializeField]
    protected GameObject bulletPF;
    [SerializeField]
    protected float projectileLifeTime;
    [SerializeField]
    protected float projectileSpeed;
    [SerializeField]
    protected Transform bulletSP;
    [SerializeField]
    protected int pierce;

    protected void ShootBullet(Vector3 dir)
    {
        Pooler.GetObject<Projectile>(bulletPF, bulletSP.position, quaternion.identity,
            onNewInstance: (e) => e.Initialize(),
            onGet: (e) =>
            {
                e.ResetObj(dir, pierce, projectileLifeTime, projectileSpeed, damage);
            }
            );
    }
}


