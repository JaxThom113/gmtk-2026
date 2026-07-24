using Sezylrin.SimplePooling;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    private PlayerComponentManager PCM;
    public Animator anim;
    public AnimatorSO animSO;
    [SerializeField]
    protected float damage = 10f;
    [SerializeField]
    protected float attackInterval;
    protected Vector3 attackDir;
    [SerializeField]
    protected BoolSO isRapidActive;
    [SerializeField]
    protected BoolSO isArsenalUnlocked;
    [SerializeField]
    protected Timer attackTimer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        isRapidActive.onValueChanged += RapidMode;
        attackTimer.GenerateTimer();
        attackTimer.SetTime(attackInterval,false);
        attackTimer.SetIsLooping(true);
        attackTimer.SetAdditionalLoops(-1);
        attackTimer.SubscribeToTimerIsZero(AutoAttack);
        //animSO.Animator = anim;
    }
    private void AutoAttack(object sender, EventArgs e)
    {
        Attack(transform.forward);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    private void StartAttacking()
    {
        attackTimer.ResumeTimer();
    }
    protected virtual void RapidMode(object sender, EventArgs e)
    {
        if (isRapidActive.Bool)
        {
            if(isArsenalUnlocked.Bool)
            {
                ActiveWeapon();
                Invoke("StartAttacking", 0.25f);
            }
            else
            {
                anim.speed = 2;
            }
        }
        else
        {
            if (isArsenalUnlocked.Bool)
            {
                StoreWeapon();
                attackTimer.PauseTimer();
                attackTimer.StopSpecific();
            }
            else
            {
                anim.speed = 1;
            }
        }
    }
    public void SwitchWeapon()
    {
        animSO.Animator = anim;
    }

    public float GetAttackInterval()
    {
        return isRapidActive.Bool? attackInterval * 0.5f : attackInterval;
    }

    public virtual void Attack(Vector3 attackDir)
    {
        this.attackDir = attackDir;
        anim.Play("Attack");
    }

    public void StoreWeapon()
    {
        anim.SetBool("isStored", true);
    }

    public void ActiveWeapon()
    {
        anim.SetBool("isStored", false);
    }

    public void AssignPCM(PlayerComponentManager PCM)
    {
        this.PCM = PCM;
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


