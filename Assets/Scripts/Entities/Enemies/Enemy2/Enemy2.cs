using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy2 : Enemy
{
    [Header("Gun References")]
    [SerializeField] private Transform bullet;
    [SerializeField] private Transform shootPos;

    [Header("Enemy Stats")]
	[SerializeField] private float health;
	[SerializeField] private float damage;

    [Header("Movement Settings")]
	[SerializeField] private float speed;
	[SerializeField] private float stopDistance;

    private Vector3 playerDir;
    private bool shooting;

    private Rigidbody rb;

    /*
        This enemy moves toward the player, stops at a certain distance, then shoots
    */

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        FacePlayer();
        Move();
    }

    private void Move()
    {
        if (player == null)
            return;
        
        playerDir = (player.transform.position - transform.position).normalized;

        // move towards player
        if (Vector3.Distance(transform.position, player.transform.position) > stopDistance)
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

    private void FacePlayer()
    {
        // face toward the player
        if (playerDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(playerDir);
    }

    private IEnumerator Shoot()
    {
        shooting = true;

        yield return new WaitForSeconds(1f);

        Instantiate(bullet, shootPos.position, shootPos.rotation);

        // if player moved, break and chase player again
        if (Vector3.Distance(transform.position, player.transform.position) > stopDistance)
            yield break;

        shooting = false;
    }
}
