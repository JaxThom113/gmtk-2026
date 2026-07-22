using System;
using UnityEngine;

public class PlayerSystems : MonoBehaviour
{
    [SerializeField]
    private IntSO playerCurrentHealthSO;
    [SerializeField]
    private int playerMaxHealth;
    [SerializeField]
    private int healthDrainRate;
    [SerializeField]
    private Timer timer;
    [SerializeField]
    private IntSO adjustHealthSO;
    [SerializeField]
    private BoolSO isPlayerDeadSO;
    [SerializeField]
    private PlayerComponentManager PCM;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerCurrentHealthSO.Int = playerMaxHealth;
        timer.GenerateTimer(TimerMode.Precise);
        timer.SetTime(1f);
        timer.SetIsLooping(true);
        timer.SetAdditionalLoops(-1);
        timer.SubscribeToTimerIsZero(OnHealthDrain);
        adjustHealthSO.onValueChanged += AdjustHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnHealthDrain(object sender, EventArgs e)
    {
        playerCurrentHealthSO.Int -= healthDrainRate;
        if (playerCurrentHealthSO.Int <= 0)
        {
            playerCurrentHealthSO.Int = 0;
            timer.StopAll();
            isPlayerDeadSO.Bool = true;
            PCM.input.DisablePlayerInputs();
        }
    }

    public void AdjustHealth(object sender, EventArgs e)
    {
        playerCurrentHealthSO.Int += adjustHealthSO.Int;
        adjustHealthSO.ResetValue();
    }
}
