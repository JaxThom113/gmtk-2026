using KevinCastejon.MissingFeatures.MissingAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;


public class PlayerController : MonoBehaviour
{
    

    [field: Header("Core variables")]
    [field: SerializeField]
    public Rigidbody rb { get; private set; }
    [SerializeField]
    private CapsuleCollider col;
    [SerializeField]
    private PlayerComponentManager PCM;

    [SerializeField]
    private BoolSO isPlayerDeadSO;

    [Header("Speed Stats")]

    [SerializeField]
    private float acceleration;
    [SerializeField]
    private float maxSpeed;
    [SerializeField]
    private float drag;
    [SerializeField] [ReadOnlyProp]
    private float currentMaxSpeed;    
    [SerializeField] [ReadOnlyProp]
    private float currentSpeed;

    [Header("Attack Stats")]
    [SerializeField]
    private float attackCD;

    [Header("Camera")]
    /*[SerializeField, Range(0f, 8f)]
    private float cameraMouseMin;
    [SerializeField, Range(0f, 16f)]
    private float cameraMouseMax;
    [SerializeField, Range(0,16)]
    private float cameraMaxOffset;
    [field: SerializeField]
    public Transform CameraFollowPoint { get; private set; }*/
    [SerializeField]
    private LayerMask cameraGroundCollisionMask;

    [Header("Debug")]
    [SerializeField, ReadOnlyProp]
    private Vector3 direction;
    [SerializeField, ReadOnlyProp]
    private Vector3 mousePos;
    [SerializeField, ReadOnlyProp]
    private Vector2 rawPos;

    private int CDTimer = (int)PlayerTimer.AttackCD;
    #region Unity Function
    void Awake()
    {
    }
    public void Start()
    {
    }
    #region Updates
    // Update is called once per frame
    void Update()
    {
    }

    private void FixedUpdate()
    {
        Move();
        UpdateMousePos();
        RotateTo();
    }

    #endregion

    #endregion

    #region GetInputs
    public void SetDirection(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>().normalized;
        direction = new Vector3(input.x, 0,input.y);
    }
    public void Attack(InputAction.CallbackContext context)
    {
        if(PCM.timer.timer.IsTimeZero(CDTimer))
        {
            PCM.anim.PlayAttack();
            PCM.timer.timer.SetTime(CDTimer, attackCD);
            PCM.timer.timer.RestartTimer(CDTimer);
        }
    }

    private void UpdateMousePos()
    {
        rawPos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(rawPos);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, cameraGroundCollisionMask))
        {
            mousePos = hit.point;
            return;
        }
    }


    #endregion

    #region Attack
    #endregion

    #region Movement

    private void Move()
    {
        
        currentMaxSpeed = maxSpeed;
        currentSpeed = rb.linearVelocity.magnitude;
        if (direction.Equals(Vector2.zero))
        {
            rb.linearDamping = drag;
            return;
        }
        else
        {
            rb.linearDamping = 0;
            rb.linearVelocity += direction * acceleration * rb.mass;
            if ((rb.linearVelocity + direction * acceleration * rb.mass).magnitude > currentMaxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * currentMaxSpeed;
            }
        }
    }

    private void RotateTo()
    {
        if(isPlayerDeadSO.Bool || !PCM.timer.timer.IsTimeZero(CDTimer))
        {
            return;
        }
        transform.LookAt(new Vector3(mousePos.x, transform.position.y, mousePos.z));
    }
    #endregion
}

