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
    private IntSO adjustHealthSO;
    [SerializeField]
    private BoolSO isPlayerDeadSO;
    [SerializeField]
    private PlayerComponentManager PCM;
    [SerializeField]
    private float iframeDur;

    private int timerPos = (int)PlayerTimer.healthDrain;
    private int iFrames = (int)PlayerTimer.Iframes;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerCurrentHealthSO.Int = playerMaxHealth;

        PCM.timer.timer.ModifyTimerMode(timerPos, TimerMode.Precise);
        PCM.timer.timer.SetTime(timerPos, 1f);
        PCM.timer.timer.ResumeTimer(timerPos);
        PCM.timer.timer.SetIsLooping(timerPos, true);
        PCM.timer.timer.SetAdditionalLoops( timerPos, -1);
        PCM.timer.timer.SubscribeToTimerIsZero(timerPos , OnHealthDrain);
        adjustHealthSO.onValueChanged += AdjustHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnHealthDrain(object sender, EventArgs e)
    {
        playerCurrentHealthSO.Int -= healthDrainRate;
        CheckHealth();
    }

    public void AdjustHealth(object sender, EventArgs e)
    {
        if(adjustHealthSO.Int < 0)
        {
            if (!PCM.timer.timer.IsTimeZero(iFrames))
            {
                return;
            }
            else
            {
                PCM.timer.timer.SetTime(iFrames, iframeDur);
            }
        }
        playerCurrentHealthSO.Int += adjustHealthSO.Int;
        adjustHealthSO.ResetValue();
        CheckHealth();
    }

    public void CheckHealth()
    {
        if (playerCurrentHealthSO.Int <= 0)
        {
            playerCurrentHealthSO.Int = 0;
            PCM.timer.timer.StopSpecific(timerPos);
            isPlayerDeadSO.Bool = true;
            PCM.input.DisablePlayerInputs();
        }
    }
}
