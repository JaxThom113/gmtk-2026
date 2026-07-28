using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UpgradeUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image artwork;

    [SerializeField] private Button selectButton;
    private UpgradeSO upgrade;
    public event Action<UpgradeSO> OnSelected;

    private Color32 weaponColor = new Color32(255, 100, 0, 255); // orange
    private Color32 abilityColor = new Color32(175, 0, 255, 255); // purple
    private Color32 passiveColor = new Color32(75, 255, 0, 255); // green

    public void Initialize(UpgradeSO data, int currentLevel)
    {
        upgrade = data;

        Color32 currentColor = new Color32(255, 255, 255, 255);;
        switch (data.type)
        {
            case UpgradeType.Weapon: currentColor = weaponColor; break;
            case UpgradeType.Ability: currentColor = abilityColor; break;
            case UpgradeType.Passive: currentColor = passiveColor; break;
        }

        levelText.text = "";
        for (int i = 0; i < data.level; i++)
            levelText.text += "*"; 
        levelText.color = currentColor;

        titleText.text = data.title;
        titleText.color = currentColor;

        typeText.text = $"- {data.type.ToString()} -";
        typeText.color = new Color32(currentColor.r, currentColor.g, currentColor.b, 64);

        artwork.sprite = data.artwork;
        descriptionText.text = data.description;

        // when the select button is clicked, return the UpgradeSO
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() =>{ OnSelected?.Invoke(upgrade); });
    }
}