using UnityEngine;

public class HammerBehaviour : MeleeWeapon
{
    [SerializeField]
    private SphereCollider hammerCol; 
    [SerializeField]
    private TrailRenderer trailRenderer;
    public void EnableHammerCollider()
    {
        hammerCol.enabled = true;
        trailRenderer.enabled = true;
    }

    public void DisableHammerCollider()
    {
        hammerCol.enabled = false;
        hitColliders.Clear();
        trailRenderer.enabled = false;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IHealth health))
        {
            if (hitColliders.Contains(other))
                return;
            hitColliders.Add(other);
            health.TakeDamage(damage);
        }


    }
}
