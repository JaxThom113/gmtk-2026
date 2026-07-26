using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class DeathScreen : MonoBehaviour
{
    [Header("Menu References")]
    [SerializeField] private MainMenu mainMenu;
    [SerializeField] private GameObject hud;

    public event Action OnEndGame;
    public event Action OnDeathStartGame;

    public void OnPlayAgainClicked()
    {
        OnDeathStartGame.Invoke();

        gameObject.SetActive(false);
    }

    public void OnMainMenuClicked()
    {
        OnEndGame.Invoke();

        hud.SetActive(false);
        gameObject.SetActive(false);
        mainMenu.gameObject.SetActive(true);
    }
}
