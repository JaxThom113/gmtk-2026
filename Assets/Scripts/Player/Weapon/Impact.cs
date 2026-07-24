using Sezylrin.SimplePooling;
using UnityEngine;

public class Impact : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    private ParticleSystem particle;

    public void OnSpawn()
    {
        particle.Play(true);
    }

    private void Update()
    {
        if (!particle.isPlaying && gameObject.activeSelf)
            Pooler.PoolObject(gameObject);
    }
}
