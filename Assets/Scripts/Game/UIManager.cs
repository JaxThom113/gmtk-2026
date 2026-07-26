using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;
using System;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [Header("Topbar")]
	[SerializeField] private TextMeshProUGUI waveCounter;
    [SerializeField] private float charactersPerSecond;
	[SerializeField] private GameObject screen;
	[SerializeField] private TextMeshProUGUI clock;
	[SerializeField] private TextMeshProUGUI levelNumber;
	[SerializeField] private Slider experienceSlider;

    [Header("Bottombar")]
    [SerializeField] private RectTransform selection;
    [SerializeField] private Image slot1;
    [SerializeField] private Image slot2;
    [SerializeField] private Image slot3;
    [SerializeField] private Image slot4;
   
    [Header("Menu References")]
    [SerializeField] private UpgradeMenu upgradeMenu;
    [SerializeField] private DeathScreen deathScreen;
    [SerializeField] private WinScreen winScreen;

    [Header("SO References")]
    [SerializeField] private IntSO playerHealthSO;
    [SerializeField] private IntSO playerLevelSO;
    [SerializeField] private IntSO playerExperienceSO;
    [SerializeField] private IntSO playerExperienceToNextLevelSO;
    [SerializeField] private IntSO playerSelectedWeaponSO;
    [SerializeField] private IntSO playerWeaponCountSO;
    [SerializeField] private BoolSO playerDeadSO;
    [SerializeField] private IntSO gameWaveSO;
    [SerializeField] private BoolSO gameWinSO;

    [Header("Game Settings")]
    [SerializeField] private GameSO game;

    bool isFlashing;
    private Sequence flash;
    private Color clockTextColor;
    private int previousLevel;
    private int previousWave;

    void OnEnable()
    {
        upgradeMenu.OnNewWeapon += UpdateSlots;

        // reset everything when enabled
        waveCounter.text = "Wave 0";
        levelNumber.text = "Lvl 00";
        slot2.gameObject.SetActive(false);
        slot3.gameObject.SetActive(false);
        slot4.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        upgradeMenu.OnNewWeapon -= UpdateSlots;
    }

    void Start()
    {
        isFlashing = false;
        clockTextColor = clock.color;
        previousLevel = playerLevelSO.Int;
    }

    void Update()
    {
        UpdateClock();
        UpdateExperience();
        UpdateHotbar();
        
        if (gameWaveSO.Int != previousWave)
            UpdateWave();

        if (playerLevelSO.Int != previousLevel && playerLevelSO.Int != 1)
            UpdateLevel();
        
        if (!deathScreen.gameObject.activeSelf && playerDeadSO.Bool)
            deathScreen.gameObject.SetActive(true);

        if (!winScreen.gameObject.activeSelf && gameWinSO.Bool)
        {
            winScreen.gameObject.SetActive(true);
            gameWinSO.Bool = false;
        }

        previousLevel = playerLevelSO.Int;
        previousWave = gameWaveSO.Int;
    }   

    private void UpdateClock()
    {
        if (playerHealthSO.Int <= 0)
            return;

        int minutes = playerHealthSO.Int / 60;
        int seconds = playerHealthSO.Int - (60 * minutes);

        string minutesString = minutes.ToString();
        string secondsString = seconds.ToString();

        if (minutes < 10)
            minutesString = "0" + minutesString;
        if (seconds < 10)
            secondsString = "0" + secondsString; 

        clock.text = $"{minutesString}:{secondsString}";

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

    private void UpdateExperience()
    {
        experienceSlider.maxValue = playerExperienceToNextLevelSO.Int;
        experienceSlider.value = playerExperienceSO.Int;
    }

    private void UpdateLevel()
    {
        string levelString = playerLevelSO.Int.ToString();
        if (playerLevelSO.Int < 10)
            levelString = "0" + levelString;

        levelNumber.text = $"LVL {levelString}";

        // display upgrade menu s
        upgradeMenu.gameObject.SetActive(true);
    }

    private void UpdateWave()
    {
        StartCoroutine(UpdateText());
    }

    private void UpdateHotbar()
    {
        switch (playerSelectedWeaponSO.Int)
        {
            case 0: selection.position = slot1.rectTransform.position; break;
            case 1: selection.position = slot2.rectTransform.position; break;
            case 2: selection.position = slot3.rectTransform.position; break;
            case 3: selection.position = slot4.rectTransform.position; break;
        }
    }

    private void UpdateSlots(Sprite sprite)
    {
        // add new weapon to a slot
        playerWeaponCountSO.Int += 1;
        switch (playerWeaponCountSO.Int)
        {
            case 1: 
                slot2.gameObject.SetActive(true);
                slot2.sprite = sprite; 
                break;
            case 2: 
                slot3.gameObject.SetActive(true);
                slot3.sprite = sprite; 
                break;
            case 3:
                slot4.gameObject.SetActive(true);
                slot4.sprite = sprite; 
                break;
        }
    }

    public IEnumerator UpdateText()
    {
        StartCoroutine(TypeRoutine($"Starting Wave {gameWaveSO.Int}..."));
        yield return new WaitForSeconds(game.waveDelay);
        StartCoroutine(TypeRoutine($"Wave {gameWaveSO.Int}"));
    }

    private IEnumerator TypeRoutine(string message)
    {
        waveCounter.text = "";

        foreach (char c in message)
        {
            waveCounter.text += c;
            yield return new WaitForSeconds(1f / charactersPerSecond);
        }

    }
}
