using KevinCastejon.MissingFeatures.MissingAttributes;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnlocks : MonoBehaviour
{
    [Header("Weapon pos")]
    [SerializeField]
    private Transform activeWeapon;
    [SerializeField]
    private List<Transform> weaponSlots;
    [Header("Weapon")]
    private Dictionary<int, weaponPos> weapons = new Dictionary<int, weaponPos>();
    [SerializeField]
    private WeaponBase startingWeapon;
    [SerializeField, ReadOnlyProp]
    private int selectedWeapon;
    [SerializeField]
    private BoolSO weaponsFull;
    private weaponPos current;

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
    }

    private void SetWeapon(weaponPos weapon)
    {
        current = weapon;
        current.weapon.SwitchWeapon();
        current.currentSpot = activeWeapon;
        current.weapon.transform.SetParent(current.currentSpot, false);
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void WeaponSwitch(int switchDir)
    {
        selectedWeapon += switchDir;
        if (selectedWeapon < 0)
            selectedWeapon += 4;
        selectedWeapon = selectedWeapon % 4;

        weaponPos selected = weapons[selectedWeapon];
        current.currentSpot = selected.currentSpot;
        current.weapon.transform.SetParent(current.currentSpot, false);
        SetWeapon(selected);
        
    }

    public void AddWeapon(WeaponBase newWeapon)
    {
        weaponPos newWep = new weaponPos(weaponSlots[weapons.Count - 1], newWeapon);
        newWeapon.transform.SetParent(newWep.currentSpot, false);
        weapons.Add(weapons.Count, newWep);
        
        if (weapons.Count == 4)
            weaponsFull.Bool = true;
    }

    private struct weaponPos
    {
        public weaponPos(Transform pos, WeaponBase weapon)
        {
            currentSpot = pos;
            this.weapon = weapon;
        }
        public Transform currentSpot;
        public WeaponBase weapon;
    }
}
