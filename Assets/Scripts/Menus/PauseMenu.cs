using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Menu References")]
    [SerializeField] private OptionsMenu optionsMenu;
    [SerializeField] private GameObject overlay;

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
        SceneManager.LoadScene("MainMenu");
    }
}
