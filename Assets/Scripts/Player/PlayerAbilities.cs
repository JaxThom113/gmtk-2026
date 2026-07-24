using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAbilities : MonoBehaviour
{
    [SerializeField]
    private PlayerComponentManager PCM;
    [SerializeField]
    private BoolSO isTimeSlow;
    [SerializeField]
    private float timeSlowDuration;
    [SerializeField]
    private float timeSlowCD;

    private int Dur = (int)PlayerTimer.timeSlowDuration;
    private int CD = (int)PlayerTimer.timeSlowCD;

    private void Start()
    {
        PCM.timer.timer.SubscribeToTimerIsZero(Dur, StartCD);
        
        PCM.timer.timer.SetTime(Dur, timeSlowDuration, false);
        PCM.timer.timer.SetTime(CD, timeSlowCD,false);
        PCM.timer.timer.StopSpecific(CD);
    }
    #region inputs
    public void UseTimeSlow(InputAction.CallbackContext context)
    {
        if (!PCM.unlocks.isSlowTime)
            return;
        if (PCM.timer.timer.IsTimeZero(CD))
        {
            PCM.timer.timer.RestartTimer(Dur);
            isTimeSlow.Bool = true;
        }
    }

    private void StartCD(object sender, EventArgs e)
    {
        PCM.timer.timer.RestartTimer(CD);
        isTimeSlow.Bool = false;
    }
    #endregion
}
