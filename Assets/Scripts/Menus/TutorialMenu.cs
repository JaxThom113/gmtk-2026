using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialMenu : MonoBehaviour
{
    public void OnBackClicked()
    {
        AudioManager.Instance.PlaySound(AudioRef.Click);

        gameObject.SetActive(false);
    }
}
