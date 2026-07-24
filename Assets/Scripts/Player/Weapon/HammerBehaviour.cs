using UnityEngine;

public class HammerBehaviour : MeleeWeapon
{
    [SerializeField]
    private SphereCollider hammerCol; 
    [SerializeField]
    private TrailRenderer trailRenderer;
    [SerializeField]
    private ParticleSystem impact;
    public void EnableHammerCollider()
    {
        hammerCol.enabled = true;
        impact.Play();
    }
    public void EnableHammerTrail()
    {
        trailRenderer.enabled = true;
    }
    public void DisableHammerCollider()
    {
        hammerCol.enabled = false;
        hitColliders.Clear();
    }

    public void DisableHammerTrail()
    {
        trailRenderer.enabled = false;
    }

    public void DoDamage(Collider other)
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
