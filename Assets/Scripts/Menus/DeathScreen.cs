using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreen : MonoBehaviour
{
    public void OnPlayAgainClicked()
    {
        SceneManager.LoadScene("UI");
    }

    public void OnMainMenuClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
