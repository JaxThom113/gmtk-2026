using AYellowpaper.SerializedCollections;
using DG.Tweening;
using KevinCastejon.MissingFeatures.MissingAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum Costs
{
    dash,
    blink,
    slow,
    freeze,
    rapid,
    arsenal,
    hammer,
    pistol,
    railgun,
    laser,
    shotgun,
    sword
}
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
    [SerializeField]
    private IntSO playerSelectedWeapon;
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

        isDashUnlockedSO.onValueChanged += UnlockDash;
        isBlinkUnlockedSO.onValueChanged += UnlockBlink;
        isSlowTimeSO.onValueChanged += UnlockSlow;
        isFreezeTimeSO.onValueChanged += UnlockFreeze;
        isRapidFireSO.onValueChanged += UnlockRapid;
        isArsenalUnleashSO.onValueChanged += UnlockArsenal;
    }

    private void OnDisable()
    {
        newWeapon.onValueChanged -= AddNewWeapon;

        isDashUnlockedSO.onValueChanged -= UnlockDash;
        isBlinkUnlockedSO.onValueChanged -= UnlockBlink;
        isSlowTimeSO.onValueChanged -= UnlockSlow;
        isFreezeTimeSO.onValueChanged -= UnlockFreeze;
        isRapidFireSO.onValueChanged -= UnlockRapid;
        isArsenalUnleashSO.onValueChanged -= UnlockArsenal;
    }

    private void SetWeapon(weaponPos weapon)
    {
        current = weapon;
        current.currentSpot = activeWeapon;
        current.weapon.transform.SetParent(current.currentSpot,true);
        current.weapon.transform.DOLocalMove(Vector3.zero, 0.25f);
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void WeaponSwitch(int switchDir)
    {
        playerSelectedWeapon.Int += switchDir;
        if (playerSelectedWeapon.Int < 0)
            playerSelectedWeapon.Int += weapons.Count;
        playerSelectedWeapon.Int = playerSelectedWeapon.Int % (weapons.Count);


        weaponPos selected = weapons[playerSelectedWeapon.Int];
        current.currentSpot = selected.currentSpot;
        current.weapon.transform.SetParent(current.currentSpot, true);
        current.weapon.transform.DOLocalMove(Vector3.zero, 0.25f);

        current.weapon.StoreWeapon();
        SetWeapon(selected);
        PCM.control.SwitchActiveWeapon(selected.weapon);
        selected.weapon.ActiveWeapon();
    }
    private void AddNewWeapon(object sender, EventArgs e)
    {
        Debug.Log("so weapon: " + newWeapon.WeaponBase);
        AddWeapon(newWeapon.WeaponBase);
        newWeapon.ResetValue();
    }
    private void AddWeapon(WeaponBase newWeapon)
    {
        if(newWeapon is RailgunBehaviour)
        {
            (newWeapon as RailgunBehaviour).SetPlayerController(PCM.control);
        }
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
    private void UnlockDash(object sender, EventArgs e)
    {
        isDashUnlocked = isDashUnlockedSO.Bool;
    }
    public bool isDashUnlocked;
    [SerializeField]
    private BoolSO isBlinkUnlockedSO; 
    private void UnlockBlink(object sender, EventArgs e)
    {
        isBlinkUnlocked = isBlinkUnlockedSO.Bool;
    }
    public bool isBlinkUnlocked;
    [SerializeField]
    private BoolSO isSlowTimeSO;
    private void UnlockSlow(object sender, EventArgs e)
    {
        isSlowTime = isSlowTimeSO.Bool;
    }
    public bool isSlowTime;
    [SerializeField]
    private BoolSO isFreezeTimeSO;
    private void UnlockFreeze(object sender, EventArgs e)
    {
        isFreezeTime = isFreezeTimeSO.Bool;
    }
    public bool isFreezeTime;
    [SerializeField]
    private BoolSO isRapidFireSO;
    private void UnlockRapid(object sender, EventArgs e)
    {
        isRapidFire = isRapidFireSO.Bool;
    }
    public bool isRapidFire;
    [SerializeField]
    private BoolSO isArsenalUnleashSO;
    private void UnlockArsenal(object sender, EventArgs e)
    {
        isArsenalUnleash = isArsenalUnleashSO.Bool;
    }
    public bool isArsenalUnleash;

    public SerializedDictionary<Costs, int> timeCost = new SerializedDictionary<Costs, int>();
}
