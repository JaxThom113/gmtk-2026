using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class WinScreen : MonoBehaviour
{
    [Header("Menu References")]
    [SerializeField] private MainMenu mainMenu;
    [SerializeField] private GameObject hud;

    public event Action OnWinGame;
    public event Action OnWinStartGame;

    public void OnPlayAgainClicked()
    {
        OnWinStartGame.Invoke();

        gameObject.SetActive(false);
    }

    public void OnMainMenuClicked()
    {
        OnWinGame.Invoke();

        hud.SetActive(false);
        gameObject.SetActive(false);
        mainMenu.gameObject.SetActive(true);
    }
}
