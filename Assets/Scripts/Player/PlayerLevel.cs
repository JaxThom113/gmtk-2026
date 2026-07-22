using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    [SerializeField]
    private IntSO playerLevelSO;
    [SerializeField]
    private int currentExperience;
    [SerializeField]
    private int initialExpToNextLvl;
    [SerializeField]
    private float expIncreaseRate;
    [SerializeField]
    private IntSO adjustExp;
    void Start()
    {
        adjustExp.onValueChanged += AdjustExperience;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AdjustExperience(object sender, System.EventArgs e)
    {
        currentExperience += adjustExp.Int;
        adjustExp.ResetValue();
        CheckLevelUp();
    }

    private void CheckLevelUp()
    {
        if (currentExperience >= initialExpToNextLvl)
        {
            currentExperience -= initialExpToNextLvl;
            playerLevelSO.Int++;
            initialExpToNextLvl = Mathf.RoundToInt(initialExpToNextLvl * expIncreaseRate);
        }
    }
}
