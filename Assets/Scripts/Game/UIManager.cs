using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;
using System;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
	[SerializeField] private TextMeshProUGUI waveCounter;
	[SerializeField] private GameObject screen;
	[SerializeField] private TextMeshProUGUI clock;
	[SerializeField] private TextMeshProUGUI levelNumber;
	[SerializeField] private Slider experienceSlider;

    [Header("SO References")]
    [SerializeField] private IntSO playerHealthSO;
    [SerializeField] private IntSO playerLevelSO;
    [SerializeField] private IntSO playerExperienceSO;
    [SerializeField] private IntSO playerExperienceToNextLevelSO;

    bool isFlashing;
    private Sequence flash;
    private Color clockTextColor;

    void Start()
    {
        isFlashing = false;
        clockTextColor = clock.color;
    }

    void Update()
    {
        UpdateClock();
        UpdateLevel();
        UpdateExperience();
    }   

    private void UpdateClock()
    {
        int minutes = playerHealthSO.Int / 60;
        int seconds = playerHealthSO.Int - (60 * minutes);

        string minutesString = minutes.ToString();
        string secondsString = seconds.ToString();

        if (minutes < 10)
            minutesString = "0" + minutesString;
        if (seconds < 10)
            secondsString = "0" + secondsString; 

        clock.text = $"{minutesString}:{secondsString}";

        Debug.Log(playerHealthSO.Int);

        if (playerHealthSO.Int <= 10)
        {
            // flash red, resume timer to call the Flash() function
            if (!isFlashing)
            {
                isFlashing = true;

                // smoothly flash to red then back to white
                flash = DOTween.Sequence();
                flash.Append(screen.GetComponent<Image>().DOColor(Color.red, 0.5f));
                flash.Join(clock.DOColor(Color.red, 0.5f));
                flash.SetLoops(-1, LoopType.Yoyo);
            }
        }
        else
        {
            // stop flashing
            flash.Kill();
            flash = null;

            // reset colors
            screen.GetComponent<Image>().color = Color.white;
            clock.color = clockTextColor;

            isFlashing = false;
        }
    }

    private void UpdateLevel()
    {
        string levelString = playerLevelSO.Int.ToString();
        if (playerLevelSO.Int < 10)
            levelString = "0" + levelString;

        levelNumber.text = $"LVL {levelString}";
    }

    private void UpdateExperience()
    {
        experienceSlider.maxValue = playerExperienceToNextLevelSO.Int;
        experienceSlider.value = playerExperienceSO.Int;
    }
}
