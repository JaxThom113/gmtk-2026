using System.Collections;
using UnityEngine;

public class Enemy3 : Enemy
{
    [Header("Movement Settings")]
    [SerializeField] private float speed;
    [SerializeField] private float stepSize;
    [SerializeField] private float stopDistance;
    [SerializeField] private float StartShootingRange = 10f;
    [SerializeField] private float stepDelay = 1f;

    private Vector3 playerDir;
    private Vector3 stepPos;
    private bool stepping;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    public AnimationClip moveAnim;
    public AnimationClip shootMoveAnim;
    public AnimationClip standShootAnim;

    private int animFrame;
    private AnimationClip lastClip;
    private const int FrameStep = 10;

    void FixedUpdate()
    {
        FacePlayer();
        Move();
    }

    private void Move()
    {
        if (stepping || player == null || rb == null)
            return;

        float playerDistance = Vector3.Distance(transform.position, player.position);

        stepping = true;
        StartCoroutine(TakeStep(playerDistance));
    }

    private void FacePlayer()
    {
        if (playerDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(playerDir);
    }

    private IEnumerator TakeStep(float playerDistance)
    {
        playerDir = (player.position - transform.position).normalized;

        // Only move when outside stop range; still animate while stopped to shoot
        if (playerDistance > stopDistance)
        {
            float step = Mathf.Min(stepSize, playerDistance - stopDistance);
            stepPos = transform.position + playerDir * step;
            rb.MovePosition(stepPos);
        }

        AnimationClip clip = PickClip(playerDistance);
        if (animator != null && clip != null)
            GoToFrame(animator, clip, CurrentFrame(clip));

        yield return new WaitForSeconds(stepDelay);

        stepping = false;
    }

    private AnimationClip PickClip(float distanceFromPlayer)
    {
        AnimationClip clip;
        if (distanceFromPlayer <= stopDistance)
            clip = standShootAnim;
        else if (distanceFromPlayer <= StartShootingRange)
            clip = shootMoveAnim;
        else
            clip = moveAnim;

        if (clip != lastClip && animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
            animFrame = 0;
            lastClip = clip;
        }

        return clip;
    }

    private int CurrentFrame(AnimationClip clip)
    {
        int frameCount = Mathf.Max(1, Mathf.RoundToInt(clip.length * clip.frameRate));
        int frame = animFrame % frameCount;
        animFrame = (animFrame + FrameStep) % frameCount;
        return frame;
    }
}
