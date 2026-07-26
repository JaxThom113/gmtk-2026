using System;
using System.Collections.Generic;
using UnityEngine;

public class UnlockAbility : MonoBehaviour
{
    [SerializeField]
    private GameObject dashIcon;
    [SerializeField]
    private BoolSO UnlockDash;
    [SerializeField]
    private GameObject freezeIcon;
    [SerializeField]
    private BoolSO UnlockFreeze;
    [SerializeField]
    private GameObject rapidIcon;
    [SerializeField]
    private BoolSO UnlockRapid;

    [SerializeField]
    private List<AbilityIcon> iconList = new List<AbilityIcon>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UnlockDash.onValueChanged += UnlockDashAbility;
        UnlockFreeze.onValueChanged += UnlockFreezeAbility;
        UnlockRapid.onValueChanged += UnlockRapidAbility;
    }
    private void LateReset()
    {
        UnlockDash.onValueChanged += UnlockDashAbility;
        UnlockFreeze.onValueChanged += UnlockFreezeAbility;
        UnlockRapid.onValueChanged += UnlockRapidAbility;
        foreach (AbilityIcon icon in iconList)
        {
            icon.ResetIcon();
        }
    }

    private void UnlockDashAbility(object sender, EventArgs e)
    {
        Debug.Log("attempt unlock");
        if (UnlockDash.Bool)
        {
            Debug.Log("unlocked");
            dashIcon.SetActive(true);
        }
    }
    private void UnlockFreezeAbility(object sender, EventArgs e)
    {
        if (UnlockFreeze.Bool)
            freezeIcon.SetActive(true);
    }
    private void UnlockRapidAbility(object sender, EventArgs e)
    {
        if (UnlockRapid.Bool)
            rapidIcon.SetActive(true);
    }

    public void ResetIcon()
    {
        dashIcon.SetActive(false);
        freezeIcon.SetActive(false);
        rapidIcon.SetActive(false);

        
        Invoke("LateReset", 0.1f);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
