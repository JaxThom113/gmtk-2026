using DG.Tweening;
using KevinCastejon.MissingFeatures.MissingAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;


public class PlayerController : MonoBehaviour
{
    private enum playerState
    {
        moving,
        dashing,
    }

    [field: Header("Core variables")]
    [field: SerializeField]
    public Rigidbody rb { get; private set; } 
    [SerializeField]  private Transform pivotTransform; 
    [SerializeField]  private Transform pivotTransformActive; 
    [SerializeField]  private Transform modelTransform;
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
    private IntSO speedSO;
    [SerializeField]
    private float drag;
    [SerializeField] [ReadOnlyProp]
    private float currentMaxSpeed;    
    [SerializeField] [ReadOnlyProp]
    private float currentSpeed;
    [SerializeField]
    private float weaponRotSpeed;

    [Header("Dash")]
    [SerializeField]
    private float dashDistance;
    [SerializeField]
    private float dashDuration;
    [SerializeField]
    private CapsuleCollider playerCol;
    [SerializeField]
    private LayerMask enemyLayer;
    [SerializeField]
    private float dashCD;
    [SerializeField]
    private SkinnedMeshRenderer rend;
    [SerializeField]
    [ColorUsage(true, true)]
    private Color dashColor;
    [SerializeField]
    private float spiralness;
    [SerializeField]
    private float blinkDuration;
    [SerializeField]
    private GameObject modelObj;
    [SerializeField]
    private LayerMask playerLayer;
    [SerializeField]
    private FloatSO dashCDSO;

    [Header("Animation")]
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private float modelTurnSpeed = 12f;
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
    [SerializeField,ReadOnlyProp]
    private playerState state;

    private int CDTimer = (int)PlayerTimer.AttackCD;
    private int DashCD = (int)PlayerTimer.DashCD;
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
        dashCDSO.Float = PCM.timer.timer.RatioOfTimePassed(DashCD);
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
    
    public void Dash(InputAction.CallbackContext context)
    {
        if (!PCM.unlocks.isDashUnlocked)
            return;
        if(PCM.timer.timer.IsTimeZero(DashCD))
            PlayerDash();
    }
    public void SetDirection(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>().normalized;
        direction = new Vector3(input.x, 0,input.y);
    }
    private bool isAttacking = false;
    public void AttemptAttack(InputAction.CallbackContext context)
    {
        
        isAttacking = true; 
        
        PCM.timer.timer.SubscribeToTimerIsZero(CDTimer, StartAttacking); 
        
        if (PCM.timer.timer.IsTimeZero(CDTimer))
        {
            Attack();
        }
    }

    public void StartAttacking(object sender, EventArgs e)
    {
        if(isAttacking)
            Attack();
    }
    public void StopAttack(InputAction.CallbackContext callback)
    {
        isAttacking=false;
        if (activeWeapon is LaserBehaviour)
        {
            (activeWeapon as LaserBehaviour).StopLaser();
        }
        PCM.timer.timer.UnsubscribeToTimerIsZero(CDTimer, StartAttacking);

    }

    public void SwitchWeapons(InputAction.CallbackContext context)
    {
        float input = context.ReadValue<float>();
        PCM.unlocks.WeaponSwitch((int)input);
    }
    public void Attack()
    {
        PCM.timer.timer.SetTime(CDTimer, activeWeapon.GetAttackInterval());
        if (!PCM.systems.UseHealth(PCM.unlocks.timeCost[activeWeapon.weaponType]))
        {
            if (activeWeapon is LaserBehaviour)
            {
                (activeWeapon as LaserBehaviour).StopLaser();
            }
            return;
        }
        activeWeapon.Attack((mousePos - transform.position).normalized);
        
    }

    public Vector3 GetAttackDir()
    {
        return (mousePos - transform.position).normalized;
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
    private WeaponBase activeWeapon;

    public void SwitchActiveWeapon(WeaponBase activeWeapon)
    {
        this.activeWeapon = activeWeapon;
    }
    #endregion

    #region Movement
    private void PlayerDash()
    {
        if (PCM.unlocks.isBlinkUnlocked)
        {
            if (!PCM.systems.UseHealth(PCM.unlocks.timeCost[Costs.blink]))
            {
                return;
            }
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetColor("_OutlineColour", dashColor);
            propertyBlock.SetFloat("_SpiralStrength", spiralness);
            modelObj.layer = LayerMask.NameToLayer("Default");

            DOVirtual.Float(1.1f, 0, blinkDuration,
                onVirtualUpdate: (f) =>
                {
                    propertyBlock.SetFloat("_DissolveAmount", f);
                    rend.SetPropertyBlock(propertyBlock);
                }
                ).OnComplete(
                () =>
                {
                    transform.position = direction * dashDistance + transform.position;
                    Collider[] enemies = Physics.OverlapSphere(transform.position, 3, enemyLayer);
                    foreach (Collider c in enemies)
                    {
                        if(c.TryGetComponent(out Enemy enemy))
                        {
                            Vector3 knockpackDir = c.transform.position - transform.position;
                            knockpackDir.y = 0;
                            knockpackDir.Normalize();
                            enemy.TakeKnockback(knockpackDir, 3);
                        }

                    }
                    DOVirtual.Float(0, 1.1f, blinkDuration,
                    onVirtualUpdate: (f) =>
                    {
                        propertyBlock.SetFloat("_DissolveAmount", f);
                        rend.SetPropertyBlock(propertyBlock);
                    }
                    ).OnComplete(()=>
                    {
                        modelObj.layer = LayerMask.NameToLayer("Player");
                    });
                }
                );
        }
        else
        {
            if (!PCM.systems.UseHealth(PCM.unlocks.timeCost[Costs.dash]))
            {
                return;
            }
            playerCol.excludeLayers += enemyLayer;
            state = playerState.dashing;
            Tween tween = transform.DOMove(direction * dashDistance + transform.position, dashDuration)
                .SetEase(Ease.OutCubic);
            tween.onComplete = () => {
                state = playerState.moving;
                playerCol.excludeLayers -= enemyLayer;
            };
        }
        PCM.timer.timer.SetTime(DashCD, dashCD);

    }
    private void Move()
    {
        if (state.Equals(playerState.dashing))
            return;
        currentMaxSpeed = speedSO.Int;
        currentSpeed = rb.linearVelocity.magnitude;
        if (direction.Equals(Vector2.zero))
        {
            rb.linearDamping = drag;
            return;
        }
        else
        {
            rb.linearDamping = 0;
            rb.linearVelocity += direction * acceleration * Time.fixedDeltaTime;
            if ((rb.linearVelocity + direction * acceleration * Time.fixedDeltaTime).magnitude > currentMaxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * currentMaxSpeed;
            }
        } 
        
        ModelControl();

    }

    private void ModelControl() { 
        if(animator != null) { 
            animator.SetFloat("Speed", currentSpeed); 
            
        }  
        if (direction.sqrMagnitude > 0.001f)
        {
            Vector3 flat = new Vector3(direction.x, 0f, direction.z);
            Quaternion targetRot = Quaternion.LookRotation(flat);
            modelTransform.rotation = Quaternion.Slerp( modelTransform.rotation, targetRot, modelTurnSpeed * Time.fixedDeltaTime);
        }
    }
    private void RotateTo()
    {
        if(isPlayerDeadSO.Bool /*|| (!PCM.timer.timer.IsTimeZero(CDTimer) && activeWeapon is MeleeWeapon*/)
        {
            return;
        }
        Vector3 mouse = new Vector3(mousePos.x, transform.position.y, mousePos.z);
        Vector3 lookdir = mouse - transform.position;
        if (lookdir.Equals(Vector3.zero))
            lookdir = Vector3.forward;
        Quaternion targetRotation = Quaternion.LookRotation(lookdir,Vector3.up);

        // 4. Smoothly rotate from current rotation to target rotation
        pivotTransform.rotation = Quaternion.Slerp(pivotTransform.rotation, targetRotation, weaponRotSpeed*Time.deltaTime);
        if (activeWeapon is MeleeWeapon && (activeWeapon as MeleeWeapon).GetAnimState())
            return;
        pivotTransformActive.rotation = Quaternion.Slerp(pivotTransformActive.rotation, targetRotation, weaponRotSpeed * Time.deltaTime);
        //pivotTransform.LookAt(new Vector3(mousePos.x, transform.position.y, mousePos.z));
    }
    #endregion
}

