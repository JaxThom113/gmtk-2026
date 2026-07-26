using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    [SerializeField]
    private Enemy1 main;

    private void OnTriggerEnter(Collider other)
    {
        main.DoDamage();
    }
}
