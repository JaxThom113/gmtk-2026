using Sezylrin.SimplePooling;
using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class WeaponBase : MonoBehaviour
{
    private PlayerComponentManager PCM;
    public Animator anim;
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
    public Costs weaponType;
    [SerializeField]
    protected string sound;
    [SerializeField]
    [Range(0f, 1f)]
    protected float volume;
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
        attackTimer.RestartTimer();
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

    public float GetAttackInterval()
    {
        return isRapidActive.Bool? attackInterval * 0.5f : attackInterval;
    }

    public virtual void Attack(Vector3 attackDir)
    {
        AudioManager.Instance.PlaySound(sound, volume: volume);
    }

    public virtual void StoreWeapon()
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
    [SerializeField]
    protected bool isAnimPlaying;

    protected List<Collider> hitColliders = new List<Collider>();
    public virtual void DoDamage(Collider other)
    {
        if (other.TryGetComponent(out IHealth health))
        {
            if(hitColliders.Contains(other))
                return;
            hitColliders.Add(other);
            health.TakeDamage(damage);
        }
    }
    public override void Attack(Vector3 attackDir)
    {
        base.Attack(attackDir);
        this.attackDir = attackDir;
        anim.Play("Attack");
    }
    public void AnimationPlaying()
    {
        isAnimPlaying = true;
    }

    public void AnimationStopped()
    {
        isAnimPlaying = false;
    }
    public bool GetAnimState()
    {
        return isAnimPlaying;
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


