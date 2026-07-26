using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class LaserBehaviour : WeaponBase
{
    [SerializeField]
    private List<LineRenderer> lineRenderer = new List<LineRenderer>();
    [SerializeField]
    private Transform spawnPoint;
    private bool laserStarted = false;
    [SerializeField]
    private LayerMask hitableLayers;
    [SerializeField]
    private ParticleSystem charge;
    [SerializeField]
    private ParticleSystem impact;

    private AudioObj audioObj;
    public override void Attack(Vector3 attackDir)
    {
        if (!laserStarted)
        {
            audioObj = AudioManager.Instance.PlaySound(AudioRef.Laser, true, volume);
            audioObj.FadeIn(0, volume, 0.2f);
            impact.Play(true);
            laserStarted = true;
            charge.Play(true);
            anim.Play("Attack");
            foreach (LineRenderer lineRenderer in lineRenderer)
            {
                lineRenderer.enabled = true;
            }
        }
        if (Physics.Raycast(spawnPoint.position, spawnPoint.forward, out RaycastHit hit, float.MaxValue, hitableLayers))
        {
            if (hit.transform.TryGetComponent(out IHealth health))
            {
                health.TakeDamage(damage);
            }
        }
    }

    public override void StoreWeapon()
    {
        StopLaser();
        base.StoreWeapon();
    }

    public void StopLaser()
    {
        audioObj.StopSound(true, 0.2f);
        anim.Play("Idle");
        laserStarted=false;
        charge.Stop();
        impact.Stop();

        foreach (LineRenderer lineRenderer in lineRenderer)
        {
            lineRenderer.enabled = false;
        }
    }
    private void Update()
    {
        if(laserStarted)
        {
            Vector3 endPoint = spawnPoint.forward * 1000;
            if (Physics.Raycast(spawnPoint.position, spawnPoint.forward, out RaycastHit hit, float.MaxValue, hitableLayers))
            {
                endPoint = hit.point;


            }
            impact.transform.position = endPoint;
            foreach (LineRenderer lineRenderer in lineRenderer)
            {
                lineRenderer.SetPosition(0, spawnPoint.position);
                lineRenderer.SetPosition(1, endPoint);
                
            }
        }
    }
}
