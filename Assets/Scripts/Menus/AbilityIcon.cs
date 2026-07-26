using System;
using UnityEngine;
using UnityEngine.UI;

public class AbilityIcon : MonoBehaviour
{
    [SerializeField]
    private FloatSO timerRatio;
    [SerializeField]
    private Image image;
    [SerializeField]
    private Image fillImage;
    [SerializeField]
    private Sprite initial;
    [SerializeField]
    private Sprite upgrade;
    [SerializeField]
    private BoolSO UnlockUpgradedSO;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UnlockUpgradedSO.onValueChanged += UpdateIcon;
    }
    private void UpdateIcon(object sender, EventArgs e)
    {
        image.sprite = upgrade;
    }

    public void ResetIcon()
    {
        UnlockUpgradedSO.onValueChanged += UpdateIcon;
        image.sprite = initial;
    }
    // Update is called once per frame
    void Update()
    {
        fillImage.fillAmount = timerRatio.Float;
    }
}
