using UnityEngine;

public class Enemy3 : Enemy
{
    [Header("Enemy3 Ranges")]
    [SerializeField] private float StartShootingRange = 10f;

    [Header("Enemy3 Animations")]
    public AnimationClip moveAnim;
    public AnimationClip shootMoveAnim;
    public AnimationClip standShootAnim;

    protected override AnimationClip PickClip(float distanceFromPlayer)
    {
        if (distanceFromPlayer <= stopDistance)
            return standShootAnim;
        if (distanceFromPlayer <= StartShootingRange)
            return shootMoveAnim;
        return moveAnim;
    }
}
