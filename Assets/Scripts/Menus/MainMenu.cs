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
        OnStartGame.Invoke();

        hud.SetActive(true);
        gameObject.SetActive(false);
    }

    public void OnHowToPlayClicked()  
    {
        tutorialMenu.gameObject.SetActive(true);
    }

    public void OnOptionsClicked()  
    {
        optionsMenu.gameObject.SetActive(true);
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }
}
