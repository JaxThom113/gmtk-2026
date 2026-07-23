using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using System.Globalization;
using UnityEngine.UIElements;

[Serializable]
public enum TimerMode
{
    /// <summary>
    /// standard countdown timer
    /// </summary>
    Default,
    /// <summary>
    /// precise countdown timer with millisecond precision
    /// </summary>
    Precise,
    /// <summary>
    /// counting timer
    /// </summary>
    StopWatch,
    /// <summary>
    /// standard countdown timer, ignores time scale
    /// </summary>
    IgnoreTimeScale,
    /// <summary>
    /// precise countdown timer with millisecond precision, ignores time scale
    /// </summary>
    IgnoreTimeScalePrecie,
    /// <summary>
    /// counting timer, ignores time scale
    /// </summary>
    IgnoreTimeScaleStopWatch
}
public class Timer : MonoBehaviour
{
    [field: SerializeField]
    private Times[] times { get; set; }

    [Serializable]
    private struct Times
    {
        [SerializeField]
        private string name;
        [Tooltip("What mode the timer should be.Default : standard countdown timer. Precise : extra millisecond precision for looping/resetting timer. StopWatch : tracks amount of time passed")]
        public TimerMode timerMode;
        [Tooltip("Should the timer start working instantly on awake")]
        public bool StartOnAwake;
        [Tooltip("Current value of the timer")]
        public float time;
        [Tooltip("Default starting time for the timer in seconds")]
        public float defaultTime;
        [Tooltip("Should the timer loop")]
        public bool isLooping;
        [Min(-1)]
        [Tooltip("Set to -1 for infinite looping, additionalLoop indicates how many additional time the timer should repeat, eg. a count of 1 will run the timer a total of 2 times.")]
        public int additionalLoop;
        public int defaultLoopCount { get; set; }
        public float overflowTime { get; set; }
        [field: SerializeField]
        public bool isPaused { get; set; }
        public bool isPrecise { get; set; }
        public bool isIgnoreTimescale { get; set; }
        public bool isCounter { get; set; }
        public UnityEvent onTimeIsZero;
        public EventHandler OnTimeIsZero;
        public void SetName(string name)
        {
            this.name = name;
        }
        public string GetName()
        {
            return name;
        }
    }

    #region Unity Functions
    private void Start()
    {
        for (int i = 0; i < times.Length; i++)
        {
            SetTime(i, times[i].defaultTime, times[i].StartOnAwake);
            ModifyTimerMode(i, times[i].timerMode);

            times[i].defaultLoopCount = times[i].additionalLoop;
        }
    }
    private void LateUpdate()
    {
        for (int i = 0; i < times.Length; i++)
        {
            if (times[i].isPaused)
                continue;
            if (times[i].isCounter)
                times[i].time += times[i].isIgnoreTimescale ? Time.unscaledDeltaTime : Time.deltaTime;
            else
            {
                if (times[i].time > 0f)
                    times[i].time -= times[i].isIgnoreTimescale ? Time.unscaledDeltaTime : Time.deltaTime;
                if (times[i].time < 0f)
                {
                    if (times[i].isPrecise)
                        times[i].overflowTime = times[i].time;
                    times[i].time = 0;
                    InvokeOnTimeIsZero(i);
                    if (times[i].isLooping && times[i].additionalLoop != 0)
                        ResetTime(i);
                    if (times[i].additionalLoop > 0)
                        times[i].additionalLoop--;
                }
            }
        }
    }
    #endregion

    #region Modify Timer Mode
    /// <summary>
    /// Modify the Timer mode of a specific timer
    /// </summary>
    /// <param name="position"></param>
    /// <param name="mode"></param>
    public void ModifyTimerMode(int position, TimerMode mode)
    {
        times[position].timerMode = mode;
        ResetTimesBool(position);
        switch (times[position].timerMode)
        {
            case TimerMode.Precise:
                times[position].isPrecise = true;
                break;
            case TimerMode.StopWatch:
                times[position].isCounter = true;
                break;
            case TimerMode.IgnoreTimeScale:
                times[position].isIgnoreTimescale = true;
                break;
            case TimerMode.IgnoreTimeScalePrecie:
                times[position].isPrecise = true;
                times[position].isIgnoreTimescale = true;
                break;
            case TimerMode.IgnoreTimeScaleStopWatch:
                times[position].isCounter = true;
                times[position].isIgnoreTimescale = true;
                break;
            default: break;
        }
    }
    /// <summary>
    /// Modify the Timer mode of the first timer
    /// </summary>
    /// <param name="mode"></param>
    public void ModifyTimerMode(TimerMode mode)
    {
        ModifyTimerMode(0, mode);
    }
    private void ResetTimesBool(int position)
    {
        times[position].isPrecise = false;
        times[position].isCounter = false;
        times[position].isIgnoreTimescale = false;
    }

    #endregion

    #region Generate Timer
    /// <summary>
    /// Generate a single timer
    /// </summary>
    /// <param name="mode"></param>
    public void GenerateTimer(TimerMode mode = TimerMode.Default)
    {
        times = new Times[1];
        for (int i = 0; i < times.Length; i++)
        {
            times[i].SetName("timer " + i.ToString());
            ModifyTimerMode(i, mode);
            times[i].StartOnAwake = true;
        }
    }
    /// <summary>
    /// Generate a timer using ints
    /// </summary>
    /// <param name="amountOfTimers"></param>
    /// <param name="owner"></param>
    public void GenerateTimer(int amountOfTimers, TimerMode mode = TimerMode.Default)
    {
        times = new Times[amountOfTimers];
        for (int i = 0; i < times.Length; i++)
        {
            times[i].SetName("timer " + i.ToString()); 
            ModifyTimerMode(i, mode);
        }
    }
    /// <summary>
    /// Generate Timer using an Enum
    /// </summary>
    /// <param name="enumName"></param>
    /// <param name="owner"></param>
    public void GenerateTimer(Type enumName, GameObject owner, TimerMode mode = TimerMode.Default)
    {
        int length = Enum.GetValues(enumName).Length;
        times = new Times[length];
        for (int i = 0; i < length; i++)
        {
            times[i].SetName(Enum.GetName(enumName, i));
            ModifyTimerMode(i, mode);
        }
    }

    /// <summary>
    /// Generate Timer using a string list
    /// </summary>
    /// <param name="list"></param>
    /// <param name="owner"></param>
    public Timer(List<string> list, GameObject owner)
    {
        times = new Times[list.Count];
        for (int i = 0; i < times.Length; i++)
        {
            times[i].SetName(list[i]);
        }
    }
    #endregion

    #region SetName
    /// <summary>
    /// Change the name of the timer, used primarily for insepector view and debugging;
    /// </summary>
    /// <param name="position"></param>
    /// <param name="name"></param>
    public void SetName(int position, string name)
    {
        if (ErrorPosition(position, "SetName"))
            return;
        times[position].SetName(name);
    }
    /// <summary>
    /// Change the name of the timer, used primarily for insepector view and debugging;
    /// </summary>
    /// <param name="name"></param>
    public void SetName(string name)
    {
        SetName(0, name);
    }
    #endregion

    #region GetName
    /// <summary>
    /// Get the name of a timer;
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public string GetName(int position)
    {
        return times[position].GetName();
    }
    /// <summary>
    /// Get the name of a timer;
    /// </summary>
    /// <returns></returns>
    public string GetName()
    {
        return GetName(0);
    }

    #endregion

    #region SetTime
    /// <summary>
    /// Sets a new time for the timer at the given position
    /// </summary>
    /// <param name="position"></param>
    /// <param name="amount">The duration to set in seconds</param>
    /// <param name="startInstantly">If the timer should begin</param>
    public void SetTime(int position, float amount, bool startInstantly = true)
    {
        if (ErrorPosition(position, "SetTime"))
            return;
        if (!startInstantly)
            PauseTimer(position);
        else
            ResumeTimer(position);
        times[position].time = amount;
        times[position].defaultTime = amount;
    }
    /// <summary>
    /// Sets a new time for the timer at the first position
    /// </summary>
    /// <param name="amount">The duration to set in seconds</param>
    public void SetTime(float amount, bool startInstantly = true)
    {
        SetTime(0, amount, startInstantly);
    }
    #endregion

    #region GetTime
    /// <summary>
    /// returns the current time at the int position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public float GetTime(int position)
    {
        if (ErrorPosition(position, "GetTime"))
            return -1;
        return times[position].time;
    }
    /// <summary>
    /// returns the current time at the first position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public float GetTime()
    {
        return GetTime(0);
    }
    #endregion

    #region IsTimeZero
    /// <summary>
    /// returns true if the time at the int position is zero
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool IsTimeZero(int position)
    {
        if (ErrorPosition(position, "IsTimeZero"))
            return false;
        return times[position].time == 0;
    }
    /// <summary>
    /// returns true if the time at the first position is zero
    /// </summary>
    /// <returns></returns>
    public bool IsTimeZero()
    {
        return IsTimeZero(0);
    }
    #endregion

    #region ModifyTimeLeft
    /// <summary>
    /// use to modify the time stored in the index position, can be used to add or remove time.
    /// can restrain the modified time value to not extend past default time based on timer mode.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="amount"> duration to add (positive values) or subtract (negative values)</param>
    /// <param name="limitToDefault">Limit the final modify time to not extend past the default value</param>
    public void ModifyTimeLeft(int position, float amount, bool limitToDefault = false)
    {
        if (ErrorPosition(position, "ModifyTimeLeft"))
            return;
        times[position].time += amount;
        if (limitToDefault)
        {
            if (times[position].isCounter ? GetTime(position) < times[position].defaultTime : GetTime(position) > times[position].defaultTime)
                ResetTime(position);
        }
    }
    /// <summary>
    /// use to modify the time stored in the first position, can be used to add or remove time.
    /// can restrain the modified time value to not extend past default time based on timer mode.
    /// </summary>
    /// <param name="amount"> duration to add (positive values) or subtract (negative values)</param>
    /// <param name="limitToDefault">Limit the final modify time to not extend past the default value</param>
    public void ModifyTimeLeft(float amount, bool limitToDefault = false)
    {
        ModifyTimeLeft(0, amount, limitToDefault);
    }
    #endregion

    #region Looping
    /// <summary>
    /// Modifies looping for a given timer
    /// </summary>
    /// <param name="position"></param>
    /// <param name="isLooping"></param>
    public void SetIsLooping(int position ,bool isLooping)
    {
        if (ErrorPosition(position, "SetIsLooping"))
            return;
        times[position].isLooping = isLooping;
    }
    /// <summary>
    /// Modifies looping for the first timer
    /// </summary>
    /// <param name="isLooping"></param>
    public void SetIsLooping(bool isLooping)
    {
        SetIsLooping(0, isLooping);
    }
    /// <summary>
    /// Sets the amount of additional times the given timer will repeat itself. This will set the default loop amount for when a timer is restarted.
    /// -1 for infinitely repeating, 0 will effectively be no loop.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="amount"></param>
    public void SetAdditionalLoops(int position, int amount)
    {
        if (ErrorPosition(position, "SetAdditionalLoops"))
            return;
        times[position].additionalLoop = Mathf.Clamp(amount, -1, int.MaxValue);
        times[position].defaultLoopCount = times[position].additionalLoop;
    }
    /// <summary>
    /// Sets the amount of additional times the first timer will repeat itself.
    /// -1 for infinitely repeating, 0 will effectively be no loop.
    /// </summary>
    /// <param name="amount"></param>
    public void SetAdditionalLoops(int amount)
    {
        SetAdditionalLoops(0, amount);
    }
    #endregion

    #region Stop
    /// <summary>
    /// /// use to set all timers back to zero. Ignores looping timer
    /// Does not trigger any events. Ignores looping timers by default
    /// </summary>
    /// <param name="ignoreLoop">If looping timers get affected and reset to zero</param>
    /// <param name="disableLoop">If looping timers should get looping disabled</param>
    public void StopAll(bool ignoreLoop = true, bool disableLoop = false)
    {
        for (int i = 0; i < times.Length; i++)
        {
            if (!ignoreLoop && times[i].isLooping)
                continue;                
            times[i].time = 0;
            if (disableLoop)
                times[i].isLooping = false;
        }
    }
    /// <summary>
    /// Reset specific timer to zero, does not result in invoke of the action.
    /// Use TriggerTimer if you wish to invoke an action instantly and stop the timer
    /// </summary>
    /// <param name="position"></param>
    public void StopSpecific(int position)
    {
        if (ErrorPosition(position, "ResetSpecificToZero"))
            return;
        times[position].time = 0;
    }
    /// <summary>
    /// Reset first timer to zero, does not result in invoke of the action.
    /// Use TriggerTimer if you wish to invoke an action instantly and stop the timer
    /// </summary>
    public void StopSpecific()
    {
        StopSpecific(0);
    }
    #endregion

    #region Trigger Event
    /// <summary>
    /// Sets timer at position to 0 and invokes the event
    /// </summary>
    /// <param name="position"></param>
    public void TriggerTimer(int position)
    {
        if (ErrorPosition(position, "TriggerTimer"))
            return;
        times[position].time = 0;
        InvokeOnTimeIsZero(position);
    }
    /// <summary>
    /// Sets first timer to 0 and invokes the event
    /// </summary>
    public void TriggerTimer()
    {
        TriggerTimer(0);
    }
    private void InvokeOnTimeIsZero(int timeSlot)
    {
        times[timeSlot].OnTimeIsZero?.Invoke(this, EventArgs.Empty);
        times[timeSlot].onTimeIsZero?.Invoke();
    }
    #endregion

    #region Subscribe Unsubscribe to Event
    /// <summary>
    /// Subscribe to the first timer's OnTimeIsZero event
    /// </summary>
    /// <param name="handle"></param>
    public void SubscribeToTimerIsZero(EventHandler handle)
    {
        SubscribeToTimerIsZero(0, handle);
    }
    /// <summary>
    /// Subscribe to the timer at position's OnTimeIsZero event
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="position"></param>
    public void SubscribeToTimerIsZero(int position, EventHandler handle)
    {
        if (ErrorPosition(position, "SubscribeToEvent"))
            return;
        times[position].OnTimeIsZero += handle;
    }/// <summary>
     /// Unsubscribe to the first timer's OnTimeIsZero event
     /// </summary>
     /// <param name="handle"></param>
    public void UnsubscribeToTimerIsZero(EventHandler handle)
    {
        UnsubscribeToTimerIsZero(0, handle);
    }
    /// <summary>
    /// Unsubscribe to the timer at position's OnTimeIsZero event
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="position"></param>
    public void UnsubscribeToTimerIsZero(int position, EventHandler handle)
    {
        if (ErrorPosition(position, "SubscribeToEvent"))
            return;
        times[position].OnTimeIsZero -= handle;
    }
    #endregion

    #region Resetting
    private void ResetTime(int position)
    {
        if (ErrorPosition(position, "ResetTime"))
            return;
        times[position].time = times[position].defaultTime;
        if (times[position].isPrecise)
            ModifyTimeLeft(position, times[position].overflowTime);
    }
    /// <summary>
    /// Reset the timer at the given position to the initial default time and starts playing.
    /// Set time must have been used initially else timer will be set to 0 without invoking event.
    /// Will reset additional loops if applicable
    /// </summary>
    /// <param name="position"></param>
    public void RestartTimer(int position, bool startPlaying = true)
    {
        ResetTime(position);
        if(startPlaying)
            ResumeTimer(position);
        else
            PauseTimer(position);
        if (times[position].isLooping)
            times[position].additionalLoop = times[position].defaultLoopCount;
    }
    /// <summary>
    /// Reset the first time to the initial default time.
    /// Set time must have been used initially else timer will be set to 0 without invoking event.
    /// Will reset additional loops if applicable
    /// </summary>
    public void RestartTimer(bool startPlaying = true)
    {
        RestartTimer(0, startPlaying);
    }
    #endregion

    #region Pause and Resume
    /// <summary>
    /// Pause the timer at int position
    /// </summary>
    /// <param name="position"></param>
    public void PauseTimer(int position)
    {
        if (ErrorPosition(position, "PauseTimer"))
            return;
        times[position].isPaused = true;
    }
    /// <summary>
    /// Pause the first timer
    /// </summary>
    public void PauseTimer()
    {
        PauseTimer(0);
    }
    /// <summary>
    /// Resume the timer at int position
    /// </summary>
    /// <param name="position"></param>
    public void ResumeTimer(int position)
    {
        if (ErrorPosition(position, "ResumeTimer"))
            return;
        times[position].isPaused = false;
    }
    /// <summary>
    /// Resume the first timer
    /// </summary>
    public void ResumeTimer()
    {
        ResumeTimer(0);
    }
    #endregion

    #region Utility
    /// <summary>
    /// For the timer at the given position,
    /// gives a ratio inbetween 0-1 with 0 being no time has passed since setting the timer and 1 being the timer is completed. Based on the default time. 
    /// returns 0 for counter mode timers.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public float RatioOfTimePassed(int position)
    {
        if (ErrorPosition(position, "RatioOfTimePassed"))
            return 0;
        if (times[position].isCounter)
            return 0;
        if (times[position].defaultTime == 0)
            return 1;
        else
            return 1 - (times[position].time / times[position].defaultTime);
    }
    /// <summary>
    /// For the timer at the first position,
    /// gives a ratio inbetween 0-1 with 0 being no time has passed since setting the timer and 1 being the timer is completed. Based on the default time. 
    /// 
    /// </summary>
    /// <returns></returns>
    public float RatioOfTimePassed()
    {
        return RatioOfTimePassed(0);
    }

    /// <summary>
    /// Check if timer at position is paused
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool IsPaused(int position)
    {
        if (ErrorPosition(position, "IsPaused"))
            return false;
        return times[position].isPaused;
    }
    /// <summary>
    /// Check if first timer is paused
    /// </summary>
    /// <returns></returns>
    public bool IsPaused()
    {
        return IsPaused(0);
    }
    #endregion

    #region time to string
    /// <summary>
    /// Returns string form of given timer. formated in s without millisecond and s.ms with
    /// </summary>
    /// <param name="position"></param>
    /// <param name="withMilliSecond">returns string with or without millisecond</param>
    /// <returns></returns>
    public string ToStringInSeconds(int position, bool withMilliSecond = false)
    {
        if (ErrorPosition(position, "ToStringInSeconds"))
            return "";
        if(withMilliSecond)
            return times[position].time.ToString("F3", CultureInfo.InvariantCulture);
        else
            return times[position].time.ToString("F0", CultureInfo.InvariantCulture);
    }
    /// <summary>
    /// Returns string form of first timer. formated in s without millisecond and s.ms with
    /// </summary>
    /// <param name="withMilliSecond">returns string with or without millisecond</param>
    /// <returns></returns>
    public string ToStringInSeconds(bool withMilliSecond = false)
    {
        return ToStringInSeconds(0,withMilliSecond);
    }
    /// <summary>
    /// Returns string form of given timer rounded to the closest minute.
    /// </summary>
    /// <returns></returns>
    public string ToStringInClosestMinute(int position)
    {
        if (ErrorPosition(position, "ToStringInClosestMinute"))
            return "";
        return (Mathf.RoundToInt(times[position].time / 60f)).ToString();
    }
    /// <summary>
    /// Returns string form of first timer rounded to the closest minute.
    /// </summary>
    /// <returns></returns>
    public string ToStringInClosestMinute()
    {
        return ToStringInClosestMinute(0);
    }
    /// <summary>
    /// Returns string form of given timer rounded to the closest Hour.
    /// </summary>
    /// <returns></returns>
    public string ToStringInClosestHour(int position)
    {
        if (ErrorPosition(position, "ToStringInClosestHour"))
            return "";
        return (Mathf.RoundToInt(times[position].time / 360f)).ToString();
    }
    /// <summary>
    /// Returns string form of first timer rounded to the closest Hour.
    /// </summary>
    /// <returns></returns>
    public string ToStringInClosestHour()
    {
        return ToStringInClosestHour(0);
    }
    /// <summary>
    /// Returns string form of given timer rounded to lowest closest Hour.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="withDeciamlTillNextHour">returns string with decimalRatio till next hour in format h.decimal</param>
    /// <returns></returns>
    public string ToStringLowestHour(int position, bool withDeciamlTillNextHour = false)
    {
        if (ErrorPosition(position, "ToStringLowestHour"))
            return "";
        if (withDeciamlTillNextHour) 
           return (times[position].time / 3600f).ToString("F1", CultureInfo.InvariantCulture);
        else
            return (times[position].time / 3600f).ToString("F0", CultureInfo.InvariantCulture);
    }
    /// <summary>
    /// Returns string form of first timer rounded to lowest closest Hour.
    /// </summary>
    /// <param name="withDeciamlTillNextHour">returns string with decimalRatio till next hour in format h.decimal</param>
    /// <returns></returns>
    public string ToStringLowestHour(bool withDeciamlTillNextHour = false)
    {
        return ToStringLowestHour(0, withDeciamlTillNextHour);
    }
    /// <summary>
    /// Return string form of given timer formated to standard time notation of h:mm:ss.ms
    /// </summary>
    /// <param name="position"></param>
    /// <param name="alwaysShowHour">If the string should always return the current hour even if the timer hasn't past a hour</param>
    /// <param name="withMillisecond">If the string should return with milliseconds or round to nearest second</param>
    /// <returns></returns>
    public string ToStringHourMinuteSecond(int position, bool alwaysShowHour = false, bool withMillisecond = false)
    {
        if (ErrorPosition(position, "ToStringHourMinuteSecond"))
            return "";
        string hour = ToStringLowestHour(position) + ":"; 
        string minute = ((Mathf.RoundToInt(times[position].time / 60f)) % 60).ToString("D2") + ":";
        string second;
        if (withMillisecond)
            second = (times[position].time % 60f).ToString("00.000", CultureInfo.InvariantCulture);
        else
            second = (times[position].time % 60f).ToString("00", CultureInfo.InvariantCulture);
        return (alwaysShowHour || times[position].time >= 3600f)? hour + minute + second : minute + second;
    }
    public string ToStringHourMinuteSecond(bool alwaysShowHour = false, bool withMillisecond = false)
    {
        return ToStringHourMinuteSecond(0, alwaysShowHour, withMillisecond);
    }
    #endregion
    private bool ErrorPosition(int position, string var)
    {
        if (position >= times.Length || position < 0)
        {
#if UNITY_EDITOR
            Debug.Break();
            Debug.LogWarning(var + " Call's position is out of bound, check the position value compared to amount of timers");
#endif
            return true;
        }
        return false;
    }
}