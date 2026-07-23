using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem;
using AYellowpaper.SerializedCollections;
//using UnityEngine.InputSystem.Controls;

public class InputController : MonoBehaviour
{
    // Start is called before the first frame update
    private PlayerInputs input;
    private PlayerInputs.PlayerActions player;
    [SerializedDictionary("key name", "value name")]
    SerializedDictionary<int, int> example;

    [SerializeField]
    private PlayerComponentManager PCM;
    private void Awake()
    {
        input = new PlayerInputs();
        player = input.Player;
    }

    private void OnEnable()
    {
        EnablePlayerInputs();
        player.Move.performed += PCM.control.SetDirection;
        player.Move.canceled += PCM.control.SetDirection;
        player.Attack.performed += PCM.control.AttemptAttack;
        player.Attack.canceled += PCM.control.StopAttack;
        player.SwitchWeapon.performed += PCM.control.SwitchWeapons;
        player.Dash.performed += PCM.control.Dash;
        player.FreezeTime.performed += PCM.abilities.UseTimeSlow;
    }

    public void EnablePlayerInputs()
    {
        player.Enable();
    }

    private void OnDisable()
    {
        player.Move.performed -= PCM.control.SetDirection;
        player.Move.canceled -= PCM.control.SetDirection;
        player.Attack.performed -= PCM.control.AttemptAttack;
        player.Attack.canceled -= PCM.control.StopAttack;
        player.SwitchWeapon.performed -= PCM.control.SwitchWeapons;
        player.Dash.performed -= PCM.control.Dash;
        player.FreezeTime.performed -= PCM.abilities.UseTimeSlow;
        DisablePlayerInputs();
    }

    public void DisablePlayerInputs()
    {
        player.Disable();
    }
    // Update is called once per frame

    void Update()
    {
        
    }
}
