using UnityEngine;
public enum PlayerTimer
{
    healthDrain,
    AttackCD,
    Iframes,
}
public class PlayerTimers : MonoBehaviour
{
    [SerializeField]
    private PlayerComponentManager PCM;
    [SerializeField]
    public Timer timer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        timer.GenerateTimer(typeof(PlayerTimer),gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
