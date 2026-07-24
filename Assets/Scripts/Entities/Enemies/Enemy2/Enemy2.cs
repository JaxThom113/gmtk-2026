using Sezylrin.SimplePooling;
using System.Collections;
using UnityEngine;

public class Enemy2 : Enemy
{
    [Header("Gun References")]
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform shootPos;

    private bool shooting;

    protected override void FixedUpdate()
    {
        FacePlayer();
        MoveContinuous();
    }

    private void MoveContinuous()
    {
        if (player == null)
            return;

        playerDir = (player.position - transform.position).normalized;

        if (Vector3.Distance(transform.position, player.position) > stopDistance)
        {
            rb.linearVelocity = playerDir * speed;
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
            if (!shooting)
                StartCoroutine(Shoot());
        }
    }

    private IEnumerator Shoot()
    {
        shooting = true;

        yield return new WaitForSeconds(1f);
        Pooler.GetObject<Bullet>(bullet, shootPos.position, shootPos.rotation,
            onNewInstance: (b) => b.Initialise(damage),
            onGet: (b) => b.ResetObj()
            );

        shooting = false;

        if (Vector3.Distance(transform.position, player.position) > stopDistance)
            yield break;
    }
}
