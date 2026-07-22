using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    public Animator anim;
    public AnimatorSO animSO;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        animSO.Animator = anim;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchWeapon()
    {
        animSO.Animator = anim;
    }
}

public class MeleeWeapon : WeaponBase
{
    [SerializeField]
    protected float damage = 10f;
    
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

}
