using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class CrystalAnimation : MonoBehaviour
{
    [Header("Crystals")]
    [SerializeField] private List<Transform> crystals = new();
    [Header("Rotation")]
    public float rotationDuration = 5f;
    [Header("Floating")]
    public float floatHeight = 0.3f;
    public float floatDuration = 2f;


 
    void Start()
    {
        foreach(Transform crystal in crystals)
        {
            if (crystal == null)
                continue;
            Vector3 startPos = crystal.position;
            crystal.DORotate(new Vector3(0f, 360f, 0f), rotationDuration, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
            crystal.DOMoveY(startPos.y + floatHeight, floatDuration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        }  
        
    }

}
