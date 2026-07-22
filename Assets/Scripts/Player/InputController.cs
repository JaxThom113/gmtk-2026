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
        if (PlayerComponentManager.Instance != gameObject)
            return;
        //Debug.Log("called");
        EnablePlayerInputs();
        player.Move.performed += PCM.control.SetDirection;
        player.Move.canceled += PCM.control.SetDirection;
        player.Attack.performed += PCM.control.Attack;
    }

    public void EnablePlayerInputs()
    {
        player.Enable();
    }

    private void OnDisable()
    {
        if (PlayerComponentManager.Instance != gameObject)
            return;
        player.Move.performed -= PCM.control.SetDirection;
        player.Move.canceled -= PCM.control.SetDirection;
        player.Attack.performed -= PCM.control.Attack;
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
