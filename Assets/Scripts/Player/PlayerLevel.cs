using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    [Header("SO References")]
    [SerializeField]
    private IntSO playerLevelSO;
    [SerializeField]
    private IntSO playerExperienceSO;
    [SerializeField]
    private IntSO playerExperienceToNextLevelSO;
    [SerializeField]
    private IntSO adjustExp;

    [Header("Exp Settings")]
    [SerializeField]
    private int initialExpRequirement;
    [SerializeField]
    private float expIncreaseRate;

    void Start()
    {
        adjustExp.onValueChanged += AdjustExperience;

        // requirement to get to lvl 2 is 100 exp, scales from here
        playerExperienceToNextLevelSO.Int = initialExpRequirement;
    }

    public void AdjustExperience(object sender, System.EventArgs e)
    {
        // add however much exp is dropped by an enemy
        playerExperienceSO.Int += adjustExp.Int;
        adjustExp.ResetValue();

        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        if (playerExperienceSO.Int >= playerExperienceToNextLevelSO.Int)
        {
            // reset current experience and increase level
            playerExperienceSO.Int -= playerExperienceToNextLevelSO.Int;
            playerLevelSO.Int++;

            // scale exp requirement for next level
            playerExperienceToNextLevelSO.Int = Mathf.RoundToInt(playerExperienceToNextLevelSO.Int * expIncreaseRate);
        }
    }
}
