using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System;
using DG.Tweening;

public class UpgradeMenu : MonoBehaviour
{
    [Header("Upgrades")]
    [SerializeField] private UpgradeUI upgradeTemplate;
    [SerializeField] private List<RectTransform> slotPositions;

    [Header("Menu References")]
    [SerializeField] private GameObject overlay;

    [Header("Manager References")]
    [SerializeField] private GameManager gameManager;

    [Header("SO References")]
    [SerializeField] private IntSO playerWeaponCount;

    [Header("Card Movement Settings")]
    [SerializeField] private float moveDistance = 40f;
    [SerializeField] private float duration = 0.4f;

    private List<UpgradeUI> currentCards;

    public event Action<Sprite> OnNewWeapon;

    void OnEnable()
    {
        overlay.SetActive(true);
        Time.timeScale = 0;

        currentCards = new List<UpgradeUI>();

        // remove weapons if the player already has all slots filled
        if (playerWeaponCount.Int == 3)
        {
            var toRemove = new List<UpgradeSO>();
            foreach (var upgrade in gameManager.upgrades)
            {
                if (upgrade.type == UpgradeType.Weapon)
                    toRemove.Add(upgrade);
            }

            foreach (var upgrade in toRemove)
                gameManager.upgrades.Remove(upgrade);
        }

        StartCoroutine(SpawnCards());
    }

    public void OnSkipClicked()
    {
        AudioManager.Instance.PlaySound(AudioRef.Click);

        foreach (var card in currentCards)
            Destroy(card.gameObject);
        
        gameObject.SetActive(false);
        overlay.SetActive(false);
        Time.timeScale = 1;
    }

    private IEnumerator SpawnCards()
    {
        // copy from the upgrades list in GameManager
        List<UpgradeSO> remainingUpgrades = new List<UpgradeSO>(gameManager.upgrades);
        
        // pick 3 random upgrades to display
        for (int i = 0; i < 3; i++)
        {
            int randUpgrade = UnityEngine.Random.Range(0, remainingUpgrades.Count);

            // instantiate an empty card and fill it with data from a random SO
            UpgradeUI card = Instantiate(upgradeTemplate, slotPositions[i]);
            card.Initialize(remainingUpgrades[randUpgrade], 1);
            card.OnSelected += OnUpgradeSelected;
            currentCards.Add(card);

            RectTransform rect = card.GetComponent<RectTransform>();
            CanvasGroup canvasGroup = card.GetComponent<CanvasGroup>();

            // start invisible
            canvasGroup.alpha = 0f;

            // start below the final position
            Vector2 endPos = Vector2.zero;
            rect.anchoredPosition = endPos - Vector2.up * moveDistance;

            // move up to slot position and fade in
            Sequence slideIn = DOTween.Sequence().SetUpdate(true);
            slideIn.Join(rect.DOAnchorPos(endPos, duration).SetEase(Ease.OutCubic));
            slideIn.Join(canvasGroup.DOFade(1f, duration));

            // remove it from remaining list so upgrades don't appear twice
            remainingUpgrades.RemoveAt(randUpgrade);

            yield return new WaitForSecondsRealtime(0.2f);
        }
    }

    private void OnUpgradeSelected(UpgradeSO selectedUpgrade)
    {
        AudioManager.Instance.PlaySound(AudioRef.Click);

        switch (selectedUpgrade.type)
        {
            case UpgradeType.Weapon:
                WeaponSO weapon = selectedUpgrade as WeaponSO;
                if (weapon != null)
                {
                    WeaponBase temp = Instantiate(weapon.weaponPF, Vector3.zero, Quaternion.identity).GetComponent<WeaponBase>();
                    
                    weapon.weaponSO.WeaponBase = temp;
                    OnNewWeapon?.Invoke(weapon.icon);
                }
                else
                {
                    Debug.Log("weapon not yet created");
                }
                break;
            case UpgradeType.Ability:
                AbilitySO ability = selectedUpgrade as AbilitySO;
                if (ability.unlockAbility != null)
                {
                    ability.unlockAbility.Bool = true;
                }
                break;
            case UpgradeType.Passive:
                PassiveSO passive = selectedUpgrade as PassiveSO;
                passive.statToIncrease.Int += passive.increase;
                break;
        }

        // remove the current upgrade, add the leveled up version if there is one
        gameManager.upgrades.Remove(selectedUpgrade);
        if (selectedUpgrade.nextLevel != null)
            gameManager.upgrades.Add(selectedUpgrade.nextLevel);

        foreach (var card in currentCards)
            Destroy(card.gameObject);

        gameObject.SetActive(false);
        overlay.SetActive(false);
        Time.timeScale = 1;
    }
}
