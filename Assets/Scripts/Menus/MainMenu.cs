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

    [Header("SO References")]
    [SerializeField] private BoolSO gamePlaying;

    [Header("Camera Manager")]
    [SerializeField] private CameraManager cameraManager;

    public void OnStartClicked()
    {
        cameraManager.ActivateCamera(1);
        gamePlaying.Bool = true;
        hud.SetActive(true);
        
        gameObject.SetActive(false);
    }

    public void OnHowToPlaylicked()  
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
