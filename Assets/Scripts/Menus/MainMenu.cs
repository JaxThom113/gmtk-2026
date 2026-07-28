using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Menu References")]
    [SerializeField] private OptionsMenu optionsMenu;
    [SerializeField] private TutorialMenu tutorialMenu;
    [SerializeField] private GameObject hud;

    public event Action OnStartGame;

    public void OnStartClicked()
    {
        AudioManager.Instance.PlaySound(AudioRef.Click);

        OnStartGame.Invoke();

        hud.SetActive(true);
        gameObject.SetActive(false);
    }

    public void OnHowToPlayClicked()  
    {
        AudioManager.Instance.PlaySound(AudioRef.Click);

        tutorialMenu.gameObject.SetActive(true);
    }

    public void OnOptionsClicked()  
    {
        AudioManager.Instance.PlaySound(AudioRef.Click);

        optionsMenu.gameObject.SetActive(true);
    }

    public void OnQuitClicked()
    {
        AudioManager.Instance.PlaySound(AudioRef.Click);

        Application.Quit();
    }
}
