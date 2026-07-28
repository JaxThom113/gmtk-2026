using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAbilities : MonoBehaviour
{
    [SerializeField]
    private PlayerComponentManager PCM;
    [Header("slow time")]
    [SerializeField]
    private BoolSO isTimeSlow;
    [SerializeField]
    private float timeSlowDuration;
    [SerializeField]
    private float timeSlowCD;
    [SerializeField]
    private BoolSO isRapidFire;
    [SerializeField]
    private float timeArsenalDuration;
    [SerializeField]
    private float timeArsenalCD;

    [SerializeField]
    private FloatSO rapidRatioSO;
    [SerializeField]
    private FloatSO slowRatioSO;


    private int TimeDur = (int)PlayerTimer.timeSlowDuration;
    private int TimeCD = (int)PlayerTimer.timeSlowCD;
    private int ArsenalDur = (int)PlayerTimer.fullArsenalDuration;
    private int ArsenalCD = (int)(PlayerTimer.fullArsenalCD);

    private void Update()
    {
        if ((PCM.timer.timer.IsPaused(TimeDur) || !PCM.timer.timer.IsTimeZero(TimeDur)) && PCM.timer.timer.IsTimeZero(TimeCD))
        {
            slowRatioSO.Float =1 - PCM.timer.timer.RatioOfTimePassed(TimeDur);
        }
        else
        {
            slowRatioSO.Float = PCM.timer.timer.RatioOfTimePassed(TimeCD);
        }
        if ((PCM.timer.timer.IsPaused(ArsenalDur) || !PCM.timer.timer.IsTimeZero(ArsenalDur)) && PCM.timer.timer.IsTimeZero(ArsenalCD))
        {
            rapidRatioSO.Float = 1 - PCM.timer.timer.RatioOfTimePassed(ArsenalDur);
        }
        else
        {
            rapidRatioSO.Float = PCM.timer.timer.RatioOfTimePassed(ArsenalCD);
        }
    }
    private void Start()
    {
        PCM.timer.timer.SubscribeToTimerIsZero(TimeDur, StartCD);
        
        PCM.timer.timer.SetTime(TimeDur, timeSlowDuration, false);
        PCM.timer.timer.SetTime(TimeCD, timeSlowCD,true);
        PCM.timer.timer.StopSpecific(TimeCD);

        PCM.timer.timer.SubscribeToTimerIsZero(ArsenalDur, StartArsenalCD);

        PCM.timer.timer.SetTime(ArsenalDur, timeArsenalDuration, false);
        PCM.timer.timer.SetTime(ArsenalCD, timeArsenalCD, true);
        PCM.timer.timer.StopSpecific(ArsenalCD);
    }
    #region inputs
    public void UseTimeSlow(InputAction.CallbackContext context)
    {
        if (!PCM.unlocks.isSlowTime)
            return;
        if (isTimeSlow.Bool)
            return;
        if (PCM.timer.timer.IsTimeZero(TimeCD))
        {
            if (PCM.unlocks.isFreezeTime)
            {
                if (!PCM.systems.UseHealth(PCM.unlocks.timeCost[Costs.freeze]))
                    return;
            }
            else
            {
                if (!PCM.systems.UseHealth(PCM.unlocks.timeCost[Costs.slow]))
                    return;
            }
            PCM.timer.timer.RestartTimer(TimeDur);
            isTimeSlow.Bool = true;
            AudioManager.Instance.PlaySound(AudioRef.TimeSlow, volume:0.3f);
        }
    }
    public void UseArsenal(InputAction.CallbackContext context)
    {
        if (!PCM.unlocks.isRapidFire)
            return;
        if (PCM.timer.timer.IsTimeZero(ArsenalCD))
        {
            if (PCM.unlocks.isArsenalUnleash)
            {
                if (!PCM.systems.UseHealth(PCM.unlocks.timeCost[Costs.arsenal]))
                    return;
            }
            else
            {
                if (!PCM.systems.UseHealth(PCM.unlocks.timeCost[Costs.rapid]))
                    return;
            }
            PCM.timer.timer.RestartTimer(ArsenalDur);
            isRapidFire.Bool = true;
        }
    }
    private void StartCD(object sender, EventArgs e)
    {
        PCM.timer.timer.RestartTimer(TimeCD);
        isTimeSlow.Bool = false;
    }
    private void StartArsenalCD(object sender, EventArgs e)
    {
        PCM.timer.timer.RestartTimer(ArsenalCD);
        isRapidFire.Bool = false;
    }
    #endregion
}
