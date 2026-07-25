using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreen : MonoBehaviour
{
    [Header("Menu References")]
    [SerializeField] private MainMenu mainMenu;
    [SerializeField] private GameObject hud;

    [Header("SO References")]
    [SerializeField] private BoolSO gamePlaying;

    [Header("Camera Manager")]
    [SerializeField] private CameraManager cameraManager;

    public void OnPlayAgainClicked()
    {
        // reload scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnMainMenuClicked()
    {
        cameraManager.ActivateCamera(0);
        gamePlaying.Bool = false;
        hud.SetActive(false);

        gameObject.SetActive(false);
        mainMenu.gameObject.SetActive(true);
    }
}
