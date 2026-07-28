using DG.Tweening;
using Sezylrin.SimplePooling;
using TMPro;
using UnityEngine;

public class DamageNumberUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text text;
    [SerializeField]
    private float duration;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Initialize()
    {

    }

    public void ResetObj(string text, Color color)
    {
        this.text.text = text;
        this.text.color = color;
        Color invis = color;
        invis.a = 0;
        this.text.DOColor(invis, duration).SetEase(Ease.InQuint);
        transform.DOMove(transform.position + Vector3.up,duration)
            .SetEase(Ease.InQuint)
            .onComplete += () => Pooler.PoolObject(gameObject);
    }

    private void Update()
    {
        
    }

}
