using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy3 : Enemy
{

    [Header("Movement Settings")]
	[SerializeField] private float speed;
	[SerializeField] private float stepSize;
	[SerializeField] private float stopDistance;

    private Vector3 playerDir;
    private Vector3 stepPos;
    private bool stepping = false;


    /*
        This enemy moves continuously toward the player in steps
    */

    
    void FixedUpdate()
    {
        FacePlayer();
        Move();
    }

    private void Move()
    {
        if (!stepping)
            StartCoroutine(TakeStep());
    }

    private void FacePlayer()
    {
        // face toward the player
        if (playerDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(playerDir);
    }

    private IEnumerator TakeStep()
    {
        if (player == null)
            yield break;

        // move towards player
        float playerDistance = Vector3.Distance(transform.position, player.transform.position);
        if (playerDistance <= stopDistance)
            yield break;

        stepping = true;

        playerDir = (player.transform.position - transform.position).normalized;

        // don't overshoot the stop distance
        float step = Mathf.Min(stepSize, playerDistance - stopDistance);
        stepPos = transform.position + playerDir * step;

        // take a step (move toward step position)
        while (Vector3.Distance(rb.position, stepPos) > 0.01f)
        {
            Vector3 previousPosition = rb.position;

            float move = speed * Time.fixedDeltaTime;
            rb.MovePosition(Vector3.MoveTowards(rb.position, stepPos, move));

            yield return new WaitForFixedUpdate();

            // if the enemy barely moved this frame, its step was block, so break out of the loop
            if ((rb.position - previousPosition).sqrMagnitude < 0.000001f)
                break;
        }

        // snap exactly to the destination
        rb.MovePosition(stepPos);

        yield return new WaitForSeconds(1f);

        stepping = false;
    }
}
