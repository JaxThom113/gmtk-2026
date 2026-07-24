using UnityEngine;

public class HammerHitbox : MonoBehaviour
{
    [SerializeField]
    protected HammerBehaviour m_Behaviour;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        m_Behaviour.DoDamage(other);
    }
}
