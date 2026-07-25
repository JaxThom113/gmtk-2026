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

    [SerializeField] private PlayerComponentManager PCM;
    [SerializeField] private PauseMenu pauseMenu;

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
        if(pauseMenu != null ) 
            player.Pause.performed += pauseMenu.OnPause;
        player.SwitchWeapon.performed += PCM.control.SwitchWeapons;
        player.Dash.performed += PCM.control.Dash;
        player.FreezeTime.performed += PCM.abilities.UseTimeSlow;
        player.RapidFire.performed += PCM.abilities.UseArsenal;
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
        if (pauseMenu != null)
            player.Pause.performed -= pauseMenu.OnPause;
        player.SwitchWeapon.performed -= PCM.control.SwitchWeapons;
        player.Dash.performed -= PCM.control.Dash;
        player.FreezeTime.performed -= PCM.abilities.UseTimeSlow;
        player.FreezeTime.performed -= PCM.abilities.UseArsenal;
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

    public void SetPauseMenu(PauseMenu pauseMenu)
    {
        if (this.pauseMenu == pauseMenu) 
            return;

        if (this.pauseMenu != null)
            player.Pause.performed -= this.pauseMenu.OnPause;

        this.pauseMenu = pauseMenu;

        if (this.pauseMenu != null)
            player.Pause.performed += this.pauseMenu.OnPause;
    }
}
