using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [Header("Volume Sliders")]
    [SerializeField] private TextMeshProUGUI musicValue;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TextMeshProUGUI sfxValue;
    [SerializeField] private Slider sfxSlider;

    void Start()
    {
        musicSlider.value = AudioManager.Instance.bgmVolume;
        sfxSlider.value = AudioManager.Instance.sfxVolume;
    }

    void Update()
    {
        AudioManager.Instance.ModifyBGMVolume(musicSlider.value);
        AudioManager.Instance.ModifySFXVolume(sfxSlider.value);

        musicValue.text = $"{(int)AudioManager.Instance.bgmVolume}";
        sfxValue.text = $"{(int)AudioManager.Instance.sfxVolume}";
    }

    public void OnBackClicked()
    {
        AudioManager.Instance.PlaySound(AudioRef.Click);

        gameObject.SetActive(false);
    }
}
