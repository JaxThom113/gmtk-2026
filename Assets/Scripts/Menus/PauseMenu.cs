using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class PauseMenu : MonoBehaviour
{
    [Header("Menu References")]
    [SerializeField] private OptionsMenu optionsMenu;
    [SerializeField] private MainMenu mainMenu;
    [SerializeField] private GameObject hud;
    [SerializeField] private GameObject overlay;

    public event Action OnEndGamePause;

    public void OnPause(InputAction.CallbackContext context)
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            overlay.SetActive(false);
            Time.timeScale = 1;
        }
        else
        {
            gameObject.SetActive(true);
            overlay.SetActive(true);
            Time.timeScale = 0;
        }
    }

    public void OnResumeClicked()
    {
        gameObject.SetActive(false);
        overlay.SetActive(false);
        Time.timeScale = 1;
    }

    public void OnOptionsClicked()
    {
        optionsMenu.gameObject.SetActive(true);
    }

    public void OnMainMenuClicked()
    {
        Time.timeScale = 1;

        OnEndGamePause.Invoke();

        gameObject.SetActive(false);
        overlay.SetActive(false);
        hud.SetActive(false);
        mainMenu.gameObject.SetActive(true);
    }
}
