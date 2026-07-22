using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    public TMP_Text healthText;
    public IntSO playerCurrentHealthSO;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        playerCurrentHealthSO.onValueChanged += UpdateHealthText;
    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void UpdateHealthText(object sender, System.EventArgs e)
    {
        healthText.text = playerCurrentHealthSO.Int.ToString();
    }
}
