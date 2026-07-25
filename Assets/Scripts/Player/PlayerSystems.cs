using DG.Tweening;
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
    private IntSO playerMaxHealth; // max time the player can have on the clock
    [SerializeField]
    private int healthDrainRate; // how many seconds are subtracted per second
    [SerializeField]
    private float secondLength; // length of a second, decrease to make seconds go by faster

    [Header("Other")]
    [SerializeField]
    private PlayerComponentManager PCM;
    [SerializeField]
    private float iframeDur;

    [Header("death")]
    [SerializeField]
    private SkinnedMeshRenderer rend;
    [SerializeField]
    float duration;

    private int timerPos = (int)PlayerTimer.healthDrain;
    private int iFrames = (int)PlayerTimer.Iframes;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerCurrentHealthSO.Int = playerMaxHealth.Int;

        PCM.timer.timer.ModifyTimerMode(timerPos, TimerMode.Precise);
        PCM.timer.timer.SetTime(timerPos, secondLength);
        PCM.timer.timer.ResumeTimer(timerPos);
        PCM.timer.timer.SetIsLooping(timerPos, true);
        PCM.timer.timer.SetAdditionalLoops( timerPos, -1);
        PCM.timer.timer.SubscribeToTimerIsZero(timerPos , OnHealthDrain);
        adjustHealthSO.onValueChanged += AdjustHealth;
    }

    private void OnDisable()
    {
        adjustHealthSO.onValueChanged -= AdjustHealth;
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
                adjustHealthSO.ResetValue();
                return;
            }
            else
            {
                PCM.timer.timer.SetTime(iFrames, iframeDur);
            }
        }

        // add health (time) back when killing an enemy
        if (playerCurrentHealthSO.Int + adjustHealthSO.Int >= playerMaxHealth.Int)
            playerCurrentHealthSO.Int = playerMaxHealth.Int;
        else
            playerCurrentHealthSO.Int += adjustHealthSO.Int;

        adjustHealthSO.ResetValue();
        CheckHealth();
    }

    public void CheckHealth()
    {
        if (playerCurrentHealthSO.Int <= 0 && !isPlayerDeadSO.Bool)
        {
            playerCurrentHealthSO.Int = 0;
            PCM.timer.timer.StopSpecific(timerPos);
            PCM.input.DisablePlayerInputs();
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            
            DOVirtual.Float(1.1f, 0, duration, onVirtualUpdate: (f) =>
            {
                propertyBlock.SetFloat("_DissolveAmount", f);
                rend.SetPropertyBlock(propertyBlock);
            }).OnComplete(() => isPlayerDeadSO.Bool = true);
;
        }
    }
}
