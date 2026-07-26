using Sezylrin.SimplePooling;
using UnityEngine;

public class SwordBehaviour : MeleeWeapon
{
    [SerializeField]
    private BoxCollider swordCol;
    [SerializeField]
    private TrailRenderer trailRenderer;
    [SerializeField]
    private ParticleSystem slash;
    [SerializeField]
    private GameObject impactPF;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EnableSwordCollider()
    {
        swordCol.enabled = true;
        trailRenderer.enabled = true;
    }

    public void DisableSwordCollider()
    {
        swordCol.enabled = false;
        hitColliders.Clear();
        trailRenderer.enabled = false;
    }

    public void PlaySlash()
    {
        slash.Play(true);
    }

    public override void DoDamage(Collider other)
    {
        base.DoDamage(other);
        Vector3 sp = other.ClosestPoint(transform.position);
        Pooler.GetObject<Impact>(impactPF, sp, Quaternion.identity,
            onGet: (e) => e.OnSpawn());
    }
}
