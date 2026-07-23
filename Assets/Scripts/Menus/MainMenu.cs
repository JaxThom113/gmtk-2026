using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Menu References")]
    [SerializeField] private OptionsMenu optionsMenu;
    [SerializeField] private TutorialMenu tutorialMenu;

    public void OnStartClicked()
    {
        SceneManager.LoadScene("UI");
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
