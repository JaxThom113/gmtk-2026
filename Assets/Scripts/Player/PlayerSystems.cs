using DG.Tweening;
using Sezylrin.SimplePooling;
using System;
using UnityEngine;

public class PlayerSystems : MonoBehaviour
{
    [Header("Core")]
    [SerializeField]
    private PlayerComponentManager PCM;
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
    [Header("Iframe")]
    [SerializeField]
    private float iframeDur;
    [SerializeField]
    private Transform shield;
    private Vector3 scaleSize;
    [SerializeField]
    private float shieldTweenDur;

    [Header("death")]
    [SerializeField]
    private SkinnedMeshRenderer rend;
    [SerializeField]
    float duration;
    [ColorUsage(true, true),SerializeField]
    private Color deathColor;

    [Header("UI")]
    [SerializeField]
    private GameObject UiPF;
    [SerializeField]
    [ColorUsage(true)]
    private Color positive;
    [SerializeField]
    [ColorUsage(true)]
    private Color negative;

    private int timerPos = (int)PlayerTimer.healthDrain;
    private int iFrames = (int)PlayerTimer.Iframes;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scaleSize = shield.localScale;
        shield.localScale = Vector3.zero;
        playerCurrentHealthSO.Int = playerMaxHealth.Int;
        shield.gameObject.SetActive(false);

        PCM.timer.timer.ModifyTimerMode(timerPos, TimerMode.Precise);
        PCM.timer.timer.SetTime(timerPos, secondLength);
        PCM.timer.timer.ResumeTimer(timerPos);
        PCM.timer.timer.SetIsLooping(timerPos, true);
        PCM.timer.timer.SetAdditionalLoops( timerPos, -1);
        PCM.timer.timer.SubscribeToTimerIsZero(timerPos , OnHealthDrain);
        adjustHealthSO.onValueChanged += AdjustHealth;

        PCM.timer.timer.SubscribeToTimerIsZero(iFrames, IframeOver);
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

    private void IframeOver(object sender, EventArgs e)
    {
        shield.transform.DOScale(Vector3.zero, shieldTweenDur)
            .SetEase(Ease.InBack)
            .onComplete += () => shield.gameObject.SetActive(true);
    }

    private void SpawnUI(int amount)
    {
        if (amount == 0)
            return;
        Color toUse;
        string text;
        if (amount < 0)
        {
            toUse = negative;
            text = "-" + (Mathf.Abs(amount)).ToString();
        }
        else
        {
            toUse = positive;
            text = "+" + amount.ToString();
        }
        Pooler.GetObject<DamageNumberUI>(UiPF, transform.position + new Vector3(0,2,1), UiPF.transform.rotation,
            onGet: (s) => s.ResetObj(text, toUse));
    }
    public bool UseHealth(int amount)
    {
        if(playerCurrentHealthSO.Int <= amount)
        {
            return false;
        }
        else
        {
            playerCurrentHealthSO.Int -= amount;
            SpawnUI(-amount);
            return true;
        }
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
                shield.gameObject.SetActive(true);
                shield.transform.DOScale(scaleSize,shieldTweenDur)
                    .SetEase(Ease.OutBack);
            }
        }

        // add health (time) back when killing an enemy
        if (playerCurrentHealthSO.Int + adjustHealthSO.Int >= playerMaxHealth.Int)
            playerCurrentHealthSO.Int = playerMaxHealth.Int;
        else
            playerCurrentHealthSO.Int += adjustHealthSO.Int;
        SpawnUI(adjustHealthSO.Int);
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

            propertyBlock.SetColor("_OutlineColour", deathColor);
            propertyBlock.SetFloat("_SpiralStrength", 0);
            DOVirtual.Float(1.1f, 0, duration, onVirtualUpdate: (f) =>
            {
                propertyBlock.SetFloat("_DissolveAmount", f);
                rend.SetPropertyBlock(propertyBlock);
            }).OnComplete(() => isPlayerDeadSO.Bool = true);
;
        }
    }
}
