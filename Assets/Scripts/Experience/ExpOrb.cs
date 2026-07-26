using DG.Tweening;
using Sezylrin.SimplePooling;
using UnityEngine;

public class ExpOrb : MonoBehaviour
{
    [SerializeField]
    private LayerMask playerLayer;
    [SerializeField]
    private SphereCollider detectionRange;
    [SerializeField]
    private SphereCollider collisionRange;
    [SerializeField]
    private int expAmount;
    [SerializeField]
    private IntSO adjustExp;
    [SerializeField]
    private float tweenDur;
    private bool isCollected = false;
    [SerializeField]
    private float volume;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    
    public void ResetObj()
    {
        isCollected = false;
        detectionRange.enabled = true;
    }
    public void SetExpAmount(int amount)
    {
        expAmount = amount;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isCollected)
        {
            transform.parent = other.transform;
            Tweener move = transform.DOLocalMove(Vector3.zero, tweenDur)
            .SetEase(Ease.Linear)
            .SetAutoKill(false);
            move.onComplete += () => transform.SetParent(null, true);
            isCollected = true;
            detectionRange.enabled = false;
        }
        else
        {
            AudioManager.Instance.PlaySound(AudioRef.ExpPickup, volume: volume);
            adjustExp.Int = expAmount;
            Pooler.PoolObject(gameObject);
        }
    }
}
