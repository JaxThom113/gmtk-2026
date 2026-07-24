using AYellowpaper.SerializedCollections;
using DG.Tweening;
using KevinCastejon.MissingFeatures.MissingAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnlocks : MonoBehaviour
{
    [SerializeField]
    private PlayerComponentManager PCM;
    [SerializeField]
    private WeaponBaseSO newWeapon;
    [Header("Weapon pos")]
    [SerializeField]
    private Transform activeWeapon;
    [SerializeField]
    private List<Transform> weaponSlots;
    
    [Header("Weapon")]
    [SerializeField]
    private SerializedDictionary<int, weaponPos> weapons = new SerializedDictionary<int, weaponPos>();
    [SerializeField]
    private WeaponBase startingWeapon;
    [SerializeField, ReadOnlyProp]
    private int selectedWeapon;
    [SerializeField]
    private BoolSO weaponsFull;
    [SerializeField]
    private weaponPos current;
    [SerializeField]
    private float weaponSwitchSpeed;

    [Header("debug")]
    [SerializeField]
    private WeaponBase testWeapon;

    [ContextMenu("addWeapon")]
    private void testAdd()
    {
        AddWeapon(testWeapon);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        weapons.Add(weapons.Count, new weaponPos(activeWeapon,startingWeapon));
        SetWeapon(weapons[0]);
        PCM.control.SwitchActiveWeapon(startingWeapon);
        newWeapon.onValueChanged += AddNewWeapon;

        isDashUnlockedSO.onValueChanged += (object sender, EventArgs e) => isDashUnlocked = isDashUnlockedSO.Bool;
        isBlinkUnlockedSO.onValueChanged += (object sender, EventArgs e) => isBlinkUnlocked = isBlinkUnlockedSO.Bool;
        isSlowTimeSO.onValueChanged += (object sender, EventArgs e) => isSlowTime = isSlowTimeSO.Bool;
        isFreezeTimeSO.onValueChanged += (object sender, EventArgs e) => isFreezeTime = isFreezeTimeSO.Bool;
        isRapidFireSO.onValueChanged += (object sender, EventArgs e) => isRapidFire = isRapidFireSO.Bool;
        isArsenalUnleashSO.onValueChanged += (object sender, EventArgs e) => isArsenalUnleash = isArsenalUnleashSO.Bool;
    }

    private void SetWeapon(weaponPos weapon)
    {
        current = weapon;
        current.currentSpot = activeWeapon;
        current.weapon.transform.SetParent(current.currentSpot);
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void WeaponSwitch(int switchDir)
    {
        selectedWeapon += switchDir;
        if (selectedWeapon < 0)
            selectedWeapon += weapons.Count;
        selectedWeapon = selectedWeapon % (weapons.Count);


        weaponPos selected = weapons[selectedWeapon];
        current.currentSpot = selected.currentSpot;
        current.weapon.transform.SetParent(current.currentSpot, true);
        current.weapon.StoreWeapon();
        SetWeapon(selected);
        PCM.control.SwitchActiveWeapon(selected.weapon);
        selected.weapon.ActiveWeapon();
    }
    private void AddNewWeapon(object sender, EventArgs e)
    {
        AddWeapon(newWeapon.WeaponBase);
        newWeapon.ResetValue();
    }
    private void AddWeapon(WeaponBase newWeapon)
    {
        weaponPos newWep = new weaponPos(weaponSlots[weapons.Count - 1], newWeapon);
        newWeapon.transform.SetParent(newWep.currentSpot, false);
        weapons.Add(weapons.Count, newWep);
        newWeapon.StoreWeapon();
        
        if (weapons.Count == 4)
            weaponsFull.Bool = true;
    }
    [Serializable]
    private class weaponPos
    {
        public weaponPos(Transform pos, WeaponBase weapon)
        {
            currentSpot = pos;
            this.weapon = weapon;
        }
        public Transform currentSpot;
        public WeaponBase weapon;
    }
    [SerializeField]
    private BoolSO isDashUnlockedSO;
    public bool isDashUnlocked;
    [SerializeField]
    private BoolSO isBlinkUnlockedSO;
    public bool isBlinkUnlocked;
    [SerializeField]
    private BoolSO isSlowTimeSO;
    public bool isSlowTime;
    [SerializeField]
    private BoolSO isFreezeTimeSO;
    public bool isFreezeTime;
    [SerializeField]
    private BoolSO isRapidFireSO;
    public bool isRapidFire;
    [SerializeField]
    private BoolSO isArsenalUnleashSO;
    public bool isArsenalUnleash;
    

}
