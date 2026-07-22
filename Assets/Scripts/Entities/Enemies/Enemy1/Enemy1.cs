using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy1 : Enemy
{
    [Header("Enemy Stats")]
	[SerializeField] private float damage;

    [Header("Movement Settings")]
	[SerializeField] private float speed;
	[SerializeField] private float stopDistance;

    private Vector3 playerDir;

    private Rigidbody rb;

    /*
        This enemy moves continuously toward the player, then attacks in melee
    */

    protected override void Start()
    {
        base.Start();
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
            rb.linearVelocity = playerDir * speed;
        else
            rb.linearVelocity = Vector3.zero;
    }

    private void FacePlayer()
    {
        // face toward the player
        if (playerDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(playerDir);
    }
}
