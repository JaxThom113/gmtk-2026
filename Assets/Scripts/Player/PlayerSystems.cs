using System;
using UnityEngine;

public class PlayerSystems : MonoBehaviour
{
    [Header("SO References")]
    [SerializeField]
    private IntSO playerCurrentHealthSO;
    [SerializeField]
    private IntSO adjustHealthSO;
    [SerializeField]
    private BoolSO isPlayerDeadSO;

    [Header("Health/Clock Settings")]
    [SerializeField]
    private int playerMaxHealth; // max time the player can have on the clock
    [SerializeField]
    private int healthDrainRate; // how many seconds are subtracted per second
    [SerializeField]
    private float secondLength; // length of a second, decrease to make seconds go by faster

    [Header("Other")]
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
        PCM.timer.timer.SetTime(timerPos, secondLength);
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

        // add health (time) back when killing an enemy
        if (playerCurrentHealthSO.Int + adjustHealthSO.Int >= playerMaxHealth)
            playerCurrentHealthSO.Int = playerMaxHealth;
        else
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
