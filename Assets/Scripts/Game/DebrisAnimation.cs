using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class DebrisAnimation : MonoBehaviour
{
    [Header("Debris")]
    [SerializeField] private Transform debrisParent;

    [Header("Rotation")]
    [SerializeField] private Vector3 rotationSpeed = new Vector3(40f, 25f, 35f);
    [SerializeField] private float rotationDuration = 8f;

    [Header("Floating")]
    [SerializeField] private float floatDistance = 0.4f;
    [SerializeField] private float floatDuration = 3f;

    [Header("Tilt")]
    [SerializeField] private float tiltAngle = 10f;
    [SerializeField] private float tiltDuration = 2.5f;

    private void Start()
    {
        foreach (Transform piece in debrisParent)
        {
            if (piece == null)
                continue;

            Vector3 startPos = piece.localPosition;

         
            piece.DORotate(
                rotationSpeed * 360f,
                rotationDuration,
                RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental);

          
            Vector3 randomOffset = new Vector3(
                Random.Range(-floatDistance, floatDistance),
                Random.Range(-floatDistance, floatDistance),
                Random.Range(-floatDistance, floatDistance));

            piece.DOLocalMove(startPos + randomOffset, floatDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(Random.Range(0f, 1f));

         
            Vector3 tilt = new Vector3(
                Random.Range(-tiltAngle, tiltAngle),
                Random.Range(-tiltAngle, tiltAngle),
                Random.Range(-tiltAngle, tiltAngle));

            piece.DOLocalRotate(tilt, tiltDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(Random.Range(0f, 1f));
        }
    }
}