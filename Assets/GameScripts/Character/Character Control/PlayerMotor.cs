﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class PlayerMotor : Character
{
    #region All Variables

    #region Stamina
    [EditorToolbar("Debug", order =5)]
    [Header("--- Debug Info ---")]
    public bool debugWindow;

    [EditorToolbar("Stamina", order =1)]
    public float maxStamina = 200f;
    public float staminaRecovery = 1.2f;
    [HideInInspector]
    public float currentStamina;
    protected bool recoveringStamina;
    [HideInInspector]
    public float currentStaminaRecoveryDelay;
    public float sprintStamina = 30f;
    public float jumpStamina = 30f;
    public float rollStamina = 25f;
    [EditorToolbar("Events")]
    public UnityEvent OnStaminaEnd;

    #endregion

    #region Layers

    [EditorToolbar("Layers", order =2)]
    [Tooltip("Layers that the character can walk on")]
    public LayerMask groundLayer = 1 << 0;
    [Tooltip("What objects can make the character auto crouch")]
    public LayerMask autoCrouchLayer = 1 << 0;
    [Tooltip("[SPHERECAST] ADJUST IN PLAY MODE - White Spherecast put just above the head, this will make the character Auto-Crouch if something hit the sphere.")]
    public float headDetect = .95f;
    [Tooltip("Select the layers the your character will stop moving when close to")]
    public LayerMask stopMoveLayer;
    [Tooltip("[RAYCAST] Stopmove Raycast Height")]
    public float stopMoveHeight = .65f;
    [Tooltip("[RAYCAST] Stopmove Raycast Distance")]
    public float stopMoveDistance = .5f;

    #endregion

    #region Character Variables

    [EditorToolbar("Locomotion", order =3)]
    [Tooltip("Turn off if you have 'in place' animations and use this values above to move the character, or use with root motion as extra speed")]
    public bool useRootMotion = false;

    public enum LocomotionType
    {
        TalkCamera,
        FreeWithStrafe,
        OnlyStrafe,
        OnlyFree,
    }
    [Tooltip("In this moment this method can changed just here in inspector")]
    public LocomotionType locomotionType = LocomotionType.FreeWithStrafe;

    public vMovementSpeed freeSpeed, strafeSpeed, talkCamera;
    [Tooltip("Use this to rotate the character using the World axis, or false to use the camera axis - CHECK for Isometric Camera")]
    public bool rotateByWorld = false;
    [Header("Use this bool if you want animations to character agressive turns")]
    [Tooltip("Check this to use the TurnOnSpot animations")]
    public bool turnOnSpotAnim = false;
    [Tooltip("Can control the roll direction")]
    public bool rollControl = false;
    [Tooltip("Put your Random Idle animations at the AnimatorController and select a value to randomize, 0 is disable.")]
    public float randomIdleTime = 0f;

    [EditorToolbar("Jump", order =3)]
    [Tooltip("Check to control the character while jumping")]
    public bool jumpAirControl = true;
    [Tooltip("How much time the character will be jumping")]
    public float jumpTimer = .3f;
    [HideInInspector]
    public float jumpCounter;
    [Tooltip("Add Extra jump speed, based on your speed input the character will move forward")]
    public float jumpForward = 3f;
    [Tooltip("Add Extra jump height, if you want to jump only with Root Motion leave the value with 0.")]
    public float jumpHeight = 4f;

    public enum GroundCheckMethod { Low, High }

    [EditorToolbar("Grounded", order =4)]
    [Tooltip("Ground Check Method To check ground Distance and ground angle\n*Simple: Use just a single Raycast\n*Normal: Use Raycast and SphereCast\n*Complex: Use SphereCastAll")]
    public GroundCheckMethod groundCheckMethod = GroundCheckMethod.High;
    [Tooltip("Distance to became not grounded")]
    [SerializeField]
    protected float groundMinDistance = .25f;
    [SerializeField]
    protected float groundMaxDistance = .5f;
    [Tooltip("ADJUST IN PLAY MODE - Offset height limit for sters - GREY Raycast in front of the legs")]
    public float stepOffsetEnd = .45f;
    [Tooltip("ADJUST IN PLAY MODE - Offset height origin for sters, make sure to keep slight above the floor - GREY Raycast in front of the legs")]
    public float stepOffsetStart = 0f;
    [Tooltip("Higher value will result jittering on ramps, lower values will have difficulty on steps")]
    public float stepSmooth = 4f;
    [Tooltip("Max angle to walk")]
    [Range(30, 75)]
    public float slopeLimit = 75f;
    [Tooltip("Velocity to slide when on a slope limit ramp")]
    [Range(0, 10)]
    public float slideVelocity = 7;
    [Tooltip("Apply extra gravity when the character is not grounded")]
    public float extraGravity = -10f;
    [Tooltip("Turn the Ragdoll On when falling at high speed (check VerticalVelocity) - leave the value with 0 if you don't want this feature")]
    public float ragdollVel = -16f;

    protected float groundDistance;
    public RaycastHit groundHit;

    #endregion

    #region Actions

    public bool isStrafing
    {
        get
        {
            return _isStrafing || lockInStrafe;
        }
        set
        {
            _isStrafing = value;
        }
    }

    //Movement bools
    [HideInInspector]
    public bool
            isGrounded,
            isCrouching,
            inCrouchArea,
            isSprinting,
            isSliding,
            stopMove,
            autoCrouch;

    // action bools
    [HideInInspector]
    public bool
            isTalking,
            isRolling,
            isJumping,
            isGettingUp,
            inTurn,
            quickStop,
            landHigh;

    [HideInInspector]
    public bool customAction;

    // one bool to rule then all
    [HideInInspector]
    public bool actions
    {
        get
        {
            return isRolling || quickStop || landHigh || customAction;
        }
    }

    protected void RemoveComponents()
    {
        if (!removeComponentsAfterDie) return;
        if (_capsuleCollider != null) Destroy(_capsuleCollider);
        if (_rigidbody != null) Destroy(_rigidbody);
        if (anime != null) Destroy(anime);
        var comps = GetComponents<MonoBehaviour>();
        for (int i = 0; i < comps.Length; i++)
        {
            Destroy(comps[i]);
        }
    }

    #endregion

    #region Direction Variable

    [HideInInspector]
    public Vector3 targetDirection;
    [HideInInspector]
    public Quaternion targetRotation;
    [HideInInspector]
    public float strafeMagnitude;
    [HideInInspector]
    public Quaternion freeRotation;
    [HideInInspector]
    public bool keepDirection;
    [HideInInspector]
    public Vector2 oldInput;

    #endregion

    #region Components

    [HideInInspector]
    internal Rigidbody _rigidbody;                                                  //acess the rigidbody component
    [HideInInspector]
    public PhysicMaterial frictionPhysics, maxFrictionPhysics, slippyPhysics;       // create PhysicMaterial for the Rigidbody
    [HideInInspector]
    public CapsuleCollider _capsuleCollider;                                        //acess capsuleCollider information

    #endregion

    #region Hidden Variables

    [HideInInspector]
    public bool lockMovement;
    [HideInInspector]
    public bool lockInStrafe;
    [HideInInspector]
    public bool lockSpeed;
    [HideInInspector]
    public bool lockRotation;
    [HideInInspector]
    public bool forceRootMotion;
    [HideInInspector]
    public float colliderRadius, colliderHeight;        //Storage the collider extra information
    [HideInInspector]
    public Vector3 colliderCenter;                      //Storage the center of the capsule collider info
    [HideInInspector]
    public float speed, direction, verticalVelocity;    //general variables to the locomotion
    [HideInInspector]
    public Vector2 input;                               //generate variable for the controller
    [HideInInspector]
    public float velocity;                              //velocity to apply to rigidbody
    //Get layers from the Animator controller
    [HideInInspector]
    public AnimatorStateInfo baseLayerInfo, underBodyInfo, rightArmInfo, leftArmInfo, fullBodyInfo, upperBodyInfo;
    [HideInInspector]
    public float jumpMultiplier = 1;
    private bool _isStrafing;
    protected float timeToResetJumpMultiplier;

    #endregion

    #endregion

    #region Start & Update Methods

    public override void Init()
    {
        base.Init();

        anime.updateMode = AnimatorUpdateMode.AnimatePhysics;

        //slides the character throughs walls and edges
        frictionPhysics = new PhysicMaterial();
        frictionPhysics.name = "frictionPhysics";
        frictionPhysics.staticFriction = .25f;
        frictionPhysics.dynamicFriction = .25f;
        frictionPhysics.frictionCombine = PhysicMaterialCombine.Multiply;

        //prevents the collider from sliping on ramps ou ladders
        maxFrictionPhysics = new PhysicMaterial();
        maxFrictionPhysics.name = "maxFrictionPhysics";
        maxFrictionPhysics.staticFriction = 1f;
        maxFrictionPhysics.dynamicFriction = 1f;
        maxFrictionPhysics.frictionCombine = PhysicMaterialCombine.Maximum;

        //air physics for better airing
        slippyPhysics = new PhysicMaterial();
        slippyPhysics.name = "slipyPhysics";
        slippyPhysics.staticFriction = 0f;
        slippyPhysics.dynamicFriction = 0f;
        slippyPhysics.frictionCombine = PhysicMaterialCombine.Minimum;

        // rigidbody info
        _rigidbody = GetComponent<Rigidbody>();

        // capsule collider info
        _capsuleCollider = GetComponent<CapsuleCollider>();

        // save your collider preferences 
        colliderCenter = GetComponent<CapsuleCollider>().center;
        colliderRadius = GetComponent<CapsuleCollider>().radius;
        colliderHeight = GetComponent<CapsuleCollider>().height;

        // avoid collision detection with inside colliders 
        Collider[] AllColliders = this.GetComponentsInChildren<Collider>();
        Collider thisCollider = GetComponent<Collider>();
        for (int i = 0; i < AllColliders.Length; i++)
        {
            Physics.IgnoreCollision(thisCollider, AllColliders[i]);
        }

        // health info
        currentHealth = maxHealth;  //update heath
        currentHealthRecoveryDelay = healthRecoveryDelay;
        currentStamina = maxStamina;
        ResetJumpMultiplier();
        isGrounded = true;
    }

    public virtual void UpdateMotor()
    {
        CheckHealth();
        CheckStamina();
        CheckGround();
        CheckRagdoll();
        StopMove();
        ControlCapsuleHeight();
        ControlJumpBehaviour();
        //ControlLocomotion();
        StaminaRecovery();
        HealthRecovery();
    }

    #endregion

    #region Health & Stamina

    public override void TakeDamage(Damage damage)
    {
        // don't apply damage if the character is rolling, you can add more conditions here
        if (currentHealth <= 0 || (!damage.ignoreDefense && isRolling))
            return;
        base.TakeDamage(damage);
        sInput.instance.GamepadVibration(0.25f);//Add vibration on controller if used
    }

    protected override void TriggerDamageRection(Damage damage)
    {
        var hitReactionConditions = !actions || !customAction;
        if (hitReactionConditions) base.TriggerDamageRection(damage);
    }

    public void ReduceStamina(float value, bool accumulative)
    {
        if (accumulative) currentStamina -= value * Time.deltaTime;
        else currentStamina -= value;
        if (currentStamina < 0)
        {
            currentStamina = 0;
            OnStaminaEnd.Invoke();
        }
    }

    public virtual void ChangeStamina(int value)
    {
        currentStamina += value;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }/// Change the currentStamina of Character

    public virtual void ChangeMaxStamina(int value)
    {
        maxStamina += value;
        if (maxStamina < 0)
            maxStamina = 0;
    }/// Change the MaxStamina of Character

    public void DeathBehaviour()
    {
        // lock the player input
        lockMovement = true;
        // change the culling mode to render the animation until finish
        anime.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        // trigger die animation            
        if (deathBy == DeathBy.Animation || deathBy == DeathBy.AnimationWithRagdoll)
            anime.SetBool("IsDead", true);
    }

    void CheckHealth()
    {
        if (isDead && currentHealth > 0)
        {
            isDead = false;
        }
    }

    void CheckStamina()
    {
        // check how much stamina this action will consume
        if (isSprinting)
        {
            currentStaminaRecoveryDelay = .25f;
            ReduceStamina(sprintStamina, true);
        }
    }

    public void StaminaRecovery()
    {
        if (currentStaminaRecoveryDelay > 0)
        {
            currentStaminaRecoveryDelay -= Time.deltaTime;
        }
        else
        {
            if (currentStamina > maxStamina)
                currentStamina = maxStamina;
            if (currentStamina < maxStamina)
                currentStamina += staminaRecovery;
        }
    }

    #endregion

    #region Locomotion

    public virtual void ControlLocomotion()
    {
        //if lock if die or cutscenes
        if (lockMovement || currentHealth <= 0) return;

        if (locomotionType.Equals(LocomotionType.FreeWithStrafe) && !isStrafing || locomotionType.Equals(LocomotionType.OnlyFree))
            FreeMovement();
        else if (locomotionType.Equals(LocomotionType.OnlyStrafe) || locomotionType.Equals(LocomotionType.FreeWithStrafe) && isStrafing)
            StrafeMovement();
    }


    /// <summary>
    /// Este metodo é chamado quando se entra em modo de conversa
    /// </summary>
    /// <param name="_isTalking"></param>
    public virtual void TalkCamera(bool _isTalking)
    {
        lockMovement = _isTalking;                  //lock player movement
        input = Vector2.zero;                       //if oldinput has value set to 0;
        isStrafing = false;                         //set strafing to not wreid on talk
        isCrouching = false;                        //set false if isCrouchinto to talk
        anime.SetFloat("InputMagnitude", 0, 0, 0);  //************
        isTalking = _isTalking;                     //set isTalking state true in Playerinputs state
    }//block all movements to talk with npc

    public virtual void StrafeMovement()
    {
        isStrafing = true;

        if (strafeSpeed.walkByDefault)
            StrafeLimitSpeed(.5f);
        else
            StrafeLimitSpeed(1f);
        if (stopMove) strafeMagnitude = 0f;
        anime.SetFloat("InputMagnitude", strafeMagnitude, .2f, Time.deltaTime);
    }

    protected virtual void StrafeLimitSpeed(float value)
    {
        var limitInput = isSprinting ? value + .5f : value;
        var _input = input * limitInput;
        var _speed = Mathf.Clamp(_input.y, -limitInput, limitInput);
        var _direction = Mathf.Clamp(_input.x, -limitInput, limitInput);
        speed = _speed;
        direction = _direction;
        var newInput = new Vector2(speed, direction);
        strafeMagnitude = Mathf.Clamp(newInput.magnitude, 0, limitInput);
    }

    public virtual void FreeMovement()
    {
        //set speed to both vertical and horizontal (return positive value only)
        speed = Mathf.Abs(input.x) + Mathf.Abs(input.y);

        //Limits the character moviment by default
        if (freeSpeed.walkByDefault)
            speed = Mathf.Clamp(speed, 0, .5f);
        else
            speed = Mathf.Clamp(speed, 0, 1f);

        //add .5f sprint
        if (isSprinting) speed += .5f;
        if (stopMove || lockSpeed) speed = 0f;

        anime.SetFloat("InputMagnitude", speed, .2f, Time.deltaTime); //************call walk/walking animations

        var conditions = (!actions || quickStop || isRolling && rollControl);

        if (input != Vector2.zero && targetDirection.magnitude > .1f && conditions && !lockRotation)
        {
            Vector3 lookDirection = targetDirection.normalized;
            freeRotation = Quaternion.LookRotation(lookDirection, transform.up);
            var diferenceRotation = freeRotation.eulerAngles.y - transform.eulerAngles.y;
            var eulerY = transform.eulerAngles.y;
            // apply free directional rotation while not turning180 animations
            if (isGrounded || (!isGrounded && jumpAirControl))
            {
                if (diferenceRotation < 0 || diferenceRotation > 0) eulerY = freeRotation.eulerAngles.y;
                var euler = new Vector3(transform.eulerAngles.x, eulerY, transform.eulerAngles.z);
                if (inTurn) return;
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(euler), freeSpeed.rotationSpeed * Time.deltaTime);
            }
            if (!keepDirection)
                oldInput = input;
            if (Vector2.Distance(oldInput, input) > .9f && keepDirection)
                keepDirection = false;
        }
    }

    public virtual void ControlSpeed(float velocity)
    {
        if (Time.deltaTime == 0) return;

        // use RootMotion and extra speed values to move the character
        if (useRootMotion && !actions && !customAction)
        {
            this.velocity = velocity;
            var deltaPosition = new Vector3(anime.deltaPosition.x, transform.position.y, anime.deltaPosition.z);
            Vector3 v = (deltaPosition * (velocity > 0 ? velocity : 1f)) / Time.deltaTime;
            v.y = _rigidbody.velocity.y;
            _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, v, 20f * Time.deltaTime);
        }
        // use only RootMotion 
        else if (actions || customAction || lockMovement || forceRootMotion)
        {
            this.velocity = velocity;
            Vector3 v = Vector3.zero;
            v.y = _rigidbody.velocity.y;
            _rigidbody.velocity = v;
            transform.position = anime.rootPosition;
            if (forceRootMotion)
                transform.rotation = anime.rootRotation;
        }
        //use only Rigibody Force to move the character (ideal for 'inplace' animations and better behaviour in general)
        else
        {
            if (isStrafing)
            {
                StrafeVelocity(velocity);
            }
            else
            {
                FreeVelocity(velocity);
            }
        }
    }

    protected virtual void StrafeVelocity(float velocity)
    {
        Vector3 v = (transform.TransformDirection(new Vector3(input.x, 0, input.y)) * (velocity > 0 ? velocity : 1f));
        v.y = _rigidbody.velocity.y;
        _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, v, 20f * Time.deltaTime);
    }

    protected virtual void FreeVelocity(float velocity)
    {
        var _targetVelocity = transform.forward * velocity * speed;
        _targetVelocity.y = _rigidbody.velocity.y;
        _rigidbody.velocity = _targetVelocity;
        //_rigidbody.AddForce(transform.forward * ((velocity * speed) * Time.deltaTime), ForceMode.VelocityChange);
    }

    protected void StopMove()
    {
        if (input.sqrMagnitude < .1f) return;

        RaycastHit hitinfo;
        Ray ray = new Ray(transform.position + Vector3.up * stopMoveHeight, targetDirection.normalized);
        var hitAngle = 0f;
        if (Physics.Raycast(ray, out hitinfo, _capsuleCollider.radius + stopMoveDistance, stopMoveLayer))
        {
            stopMove = true;
            return;
        }

        if (!isGrounded || isJumping)
        {
            stopMove = isSliding;
            return;
        }

        if (Physics.Linecast(transform.position + Vector3.up * (_capsuleCollider.height * .5f), transform.position + targetDirection.normalized * (_capsuleCollider.radius + .2f), out hitinfo, groundLayer))
        {
            hitAngle = Vector3.Angle(Vector3.up, hitinfo.normal);
            Debug.DrawLine(transform.position + Vector3.up * (_capsuleCollider.height * .5f), transform.position + targetDirection.normalized * (_capsuleCollider.radius + .2f), (hitAngle > slopeLimit) ? Color.yellow : Color.blue, .01f);
            var targetPoint = hitinfo.point + targetDirection.normalized * _capsuleCollider.radius;
            if ((hitAngle > slopeLimit) && Physics.Linecast(transform.position + Vector3.up * (_capsuleCollider.height * .5f), targetPoint, out hitinfo, groundLayer))
            {
                Debug.DrawRay(hitinfo.point, hitinfo.normal);
                hitAngle = Vector3.Angle(Vector3.up, hitinfo.normal);

                if (hitAngle > slopeLimit && hitAngle < 85f)
                {
                    Debug.DrawLine(transform.position + Vector3.up * (_capsuleCollider.height * .5f), hitinfo.point, Color.red, .01f);
                    stopMove = true;
                    return;
                }
                else
                {
                    Debug.DrawLine(transform.position + Vector3.up * (_capsuleCollider.height * .5f), hitinfo.point, Color.green, .01f);
                }
            }
        }
        else Debug.DrawLine(transform.position + Vector3.up * (_capsuleCollider.height * .5f), transform.position + targetDirection.normalized * (_capsuleCollider.radius * .2f), Color.blue, .01f);

        stopMove = false;
    }

    #endregion

    #region Jump Methods

    protected void ControlJumpBehaviour()
    {
        if (!isJumping) return;
        //counter timer
        jumpCounter -= Time.deltaTime;
        if (jumpCounter <= 0)
        {
            jumpCounter = 0;
            isJumping = false;
        }
        //apply extra force to the jump heigh
        var vel = _rigidbody.velocity;
        vel.y = jumpHeight * jumpMultiplier;
        _rigidbody.velocity = vel;
    }//make the jump behaviour raw

    public void SetJumpMultiplier(float jumpMultiplier, float timeToReset = 1f)
    {
        this.jumpMultiplier = jumpMultiplier;
        if (timeToResetJumpMultiplier <= 0)
        {
            timeToResetJumpMultiplier = timeToReset;
            StartCoroutine(ResetJumpMultiplierRoutine());
        }
        else timeToResetJumpMultiplier = timeToReset;
    }

    public void ResetJumpMultiplier()
    {
        StopCoroutine("ResetJumpMultiplierRoutine");
        timeToResetJumpMultiplier = 0;
        jumpMultiplier = 1;
    }

    protected IEnumerator ResetJumpMultiplierRoutine()
    {
        while (timeToResetJumpMultiplier > 0 && jumpMultiplier != 1)
        {
            timeToResetJumpMultiplier -= Time.deltaTime;
            yield return null;
        }
        jumpMultiplier = 1;
    }

    public void AirControl()
    {
        if (isGrounded) return;         //IS on ground
        if (!jumpFwdCondition) return;  //check if collider touch layer

        var velY = transform.forward * jumpForward * speed; //get players position and add forward force
        velY.y = _rigidbody.velocity.y;
        var velX = transform.right * jumpForward * direction;
        velX.x = _rigidbody.velocity.x;

        EnableGravityAndCollision(0f);

        if (jumpAirControl)
        {
            if (isStrafing)
            {
                _rigidbody.velocity = new Vector3(velX.x, velY.y, _rigidbody.velocity.z);
                var vel = transform.forward * (jumpForward * speed) + transform.right * (jumpForward * direction);
                _rigidbody.velocity = new Vector3(vel.x, _rigidbody.velocity.y, vel.z);
            }
            else
            {
                var vel = transform.forward * (jumpForward * speed);
                _rigidbody.velocity = new Vector3(vel.x, _rigidbody.velocity.y, vel.z);
            }
        }
        else
        {
            var vel = transform.forward * (jumpForward);
            _rigidbody.velocity = new Vector3(vel.x, _rigidbody.velocity.y, vel.z);
        }
    }

    protected bool jumpFwdCondition
    {
        get
        {
            Vector3 p1 = transform.position + _capsuleCollider.center + Vector3.up * -_capsuleCollider.height * .5f;//get capsule and player position
            Vector3 p2 = p1 + Vector3.up * _capsuleCollider.height; //get and add capsule height
            return Physics.CapsuleCastAll(p1, p2, _capsuleCollider.radius * .5f, transform.forward, .6f, groundLayer).Length == 0;//check distance about ground and floor
        }
    }

    #endregion

    #region Ground Check

    void CheckGround()
    {
        CheckGroundDistance();

        if (customAction)
        {
            isGrounded = true;
            return;
        } //todo here custom action and is died method

        //change the physycs material to very slip when not grounded
        _capsuleCollider.material = (isGrounded && GroundAngle() <= slopeLimit + 1) ? frictionPhysics : slippyPhysics;

        if (isGrounded && input == Vector2.zero)
            _capsuleCollider.material = maxFrictionPhysics;
        else if (isGrounded && input != Vector2.zero)
            _capsuleCollider.material = frictionPhysics;
        else
            _capsuleCollider.material = slippyPhysics;

        //we don't stick player if one this bool is true
        bool checkGroundConditions = !isRolling;

        var magVel = (float)System.Math.Round(new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z).magnitude, 2);
        magVel = Mathf.Clamp(magVel, 0, 1);

        var groundCheckDistance = groundMinDistance;
        if (magVel > .25f) groundCheckDistance = groundMaxDistance;

        //avoid isGrounded to false if in the air
        if (checkGroundConditions)
        {
            //clear the cheground to free the character to attack on air
            var onStep = stepOffset();

            if (groundDistance <= .05f)
            {
                isGrounded = true;
                //Slidind(); todo here 
            }
            else
            {
                if (groundDistance >= groundCheckDistance)
                {
                    isGrounded = false;
                    //check vertical velocity
                    verticalVelocity = _rigidbody.velocity.y;
                    //apply extra gravity when falling
                    if (!onStep && !isJumping)
                        _rigidbody.AddForce(transform.up * extraGravity * Time.deltaTime, ForceMode.VelocityChange);
                }
                else if (!onStep && !isJumping)
                {
                    _rigidbody.AddForce(transform.up * (extraGravity * 2 * Time.deltaTime), ForceMode.VelocityChange);
                }
            }
        }
    }

    void CheckGroundDistance()
    {
        //if (isDead) return; <-- todo in heath class********************************************************
        if (_capsuleCollider != null)
        {
            //radius of the shpereCast
            float radius = _capsuleCollider.radius * .9f;
            var dist = 10f;
            //ray for Raycast
            Ray ray2 = new Ray(transform.position + new Vector3(0, colliderHeight / 2, 0), Vector3.down);
            //raycast for check the ground distance
            if (Physics.Raycast(ray2, out groundHit, colliderHeight / 2 + 2f, groundLayer))
                dist = transform.position.y - groundHit.point.y;
            //spherecast aroud tha base of capsule to check the ground distance
            if (groundCheckMethod == GroundCheckMethod.High)
            {
                Vector3 pos = transform.position + Vector3.up * (_capsuleCollider.radius);
                Ray ray = new Ray(pos, -Vector3.up);
                if (Physics.SphereCast(ray, radius, out groundHit, _capsuleCollider.radius + 2f, groundLayer))
                {
                    //check if spherecast distance is small than ray cast distance
                    if (dist > (groundHit.distance - _capsuleCollider.radius * .1f))
                        dist = (groundHit.distance - _capsuleCollider.radius * .1f);
                }
            }
            groundDistance = (float)System.Math.Round(dist, 2);
        }
    }

    public virtual float GroundAngle()
    {
        var groundAngle = Vector3.Angle(groundHit.normal, Vector3.up);
        return groundAngle;
    }// Return the ground angle

    bool stepOffset()
    {
        if (input.sqrMagnitude < 0.1 || !isGrounded || stopMove) return false;

        var _hit = new RaycastHit();
        var _movementDirection = isStrafing && input.magnitude > 0 ? (transform.right * input.x + transform.forward * input.y).normalized : transform.forward;
        Ray rayStep = new Ray((transform.position + new Vector3(0, stepOffsetEnd, 0) + _movementDirection * ((_capsuleCollider).radius + 0.05f)), Vector3.down);

        if (Physics.Raycast(rayStep, out _hit, stepOffsetEnd - stepOffsetStart, groundLayer) && !_hit.collider.isTrigger)
        {
            if (_hit.point.y >= (transform.position.y) && _hit.point.y <= (transform.position.y + stepOffsetEnd))
            {
                var _speed = Mathf.Clamp(input.magnitude, 0, 1f);
                var velocityDirection = (_hit.point - transform.position);
                var vel = _rigidbody.velocity;
                vel.y = (velocityDirection * stepSmooth * (_speed * (velocity > 1 ? velocity : 1))).y;
                _rigidbody.velocity = vel;
                return true;
            }
        }
        return false;
    }

    #endregion

    #region Cooliders Check

    void ControlCapsuleHeight()
    {
        if(isCrouching || isRolling || landHigh)
        {
            _capsuleCollider.center = colliderCenter / 1.5f;
            _capsuleCollider.height = colliderHeight / 1.5f;
        }
        else
        {
            //return to original values
            _capsuleCollider.center = colliderCenter;
            _capsuleCollider.radius = colliderRadius;
            _capsuleCollider.height = colliderHeight;
        }
    }

    public void DisableGravityAndCollision()
    {
        anime.SetFloat("InputHorizontal", 0f);
        anime.SetFloat("InputVertical", 0f);
        anime.SetFloat("VerticalVelocity", 0f);
        _rigidbody.useGravity = false;
        _capsuleCollider.isTrigger = true;
    }// Disables rigibody gravity, turn the capsule collider trigger and reset all input from the animator.

    /// <summary>
    /// desativado no final de aninamação (subir escadas por exemplo)
    /// Ativado quando se precisa gravidade e retornar ao controle 
    /// </summary>
    /// <param name="normalizedTime"></param>
    public void EnableGravityAndCollision(float normalizedTime)
    {
        //enable coolider and gravity at the end of the animation
        if (baseLayerInfo.normalizedTime >= normalizedTime)
        {
            _capsuleCollider.isTrigger = false;
            _rigidbody.useGravity = true;
        }
    }/// Turn rigidbody gravity on the uncheck the capsulle collider as Trigger when the animation has finish playing

    #endregion

    #region Camera Methods

    public virtual void RotateToTarget(Transform target)
    {
        if (target)
        {
            Quaternion rot = Quaternion.LookRotation(target.position - transform.position);
            var newPos = new Vector3(transform.eulerAngles.x, rot.eulerAngles.y, transform.eulerAngles.z);
            targetRotation = Quaternion.Euler(newPos);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(newPos), strafeSpeed.rotationSpeed * Time.deltaTime);
        }
    }

    public virtual void RotateToDirection(Vector3 direction, bool ignoreLerp = false)
    {
        Quaternion rot = Quaternion.LookRotation(direction);
        var newPos = new Vector3(transform.eulerAngles.x, rot.eulerAngles.y, transform.eulerAngles.z);
        targetRotation = Quaternion.Euler(newPos);
        if (ignoreLerp)
            transform.rotation = Quaternion.Euler(newPos);
        else transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(newPos), strafeSpeed.rotationSpeed * Time.deltaTime);
        targetDirection = direction;
    }

    public virtual void UpdateTargetDirection(Transform referenceTransform = null)
    {
        if (referenceTransform && !rotateByWorld)
        {
            var forward = keepDirection ? referenceTransform.forward : referenceTransform.TransformDirection(Vector3.forward);
            forward.y = 0;

            forward = keepDirection ? forward : referenceTransform.TransformDirection(Vector3.forward);
            forward.y = 0; //set to 0 because of referenceTransform rotation on the X axis

            //get the right-facing direction of the referenceTransform
            var right = keepDirection ? referenceTransform.right : referenceTransform.TransformDirection(Vector3.right);

            // determine the direction the player will face based on input and the referenceTransform's right and forward directions
            targetDirection = input.x * right + input.y * forward;
        }
        else
            targetDirection = keepDirection ? targetDirection : new Vector3(input.x, 0, input.y);
    }// Update the targetDirection variable using referenceTransform or just input.Rotate by word  the referenceDirection


    #endregion

    #region Ragdoll

    void CheckRagdoll()
    {
        if (ragdollVel == 0) return;

        // check your verticalVelocity and assign a value on the variable RagdollVel at the Player Inspector
        if (verticalVelocity <= ragdollVel && groundDistance <= .1f)
        {
            onActiveRagdoll.Invoke();
        }
    }

    public override void ResetRagdoll()
    {
        lockMovement = false;
        verticalVelocity = 0;
        ragdolled = false;
        _rigidbody.WakeUp();

        _rigidbody.useGravity = true;
        _rigidbody.isKinematic = false;
        _capsuleCollider.isTrigger = false;
    }

    public override void EnableRagdoll()
    {
        anime.SetFloat("InputHorizontal", 0f);
        anime.SetFloat("InputVertical", 0f);
        anime.SetFloat("VerticalVelocity", 0f);
        ragdolled = true;
        _capsuleCollider.isTrigger = true;
        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = true;
        lockMovement = true;
    }

    #endregion

    #region Debug

    public virtual string DebugInfo(string additionalText = "")
    {
        string debugInfo = string.Empty;
        if (debugWindow)
        {
            float delta = Time.smoothDeltaTime;
            float fps = 1 / delta;

            debugInfo =
                "FPS " + fps.ToString("#,##0 fps") + "\n" +
                "inputVertical = " + input.y.ToString("0.0") + "\n" +
                "inputHorizontal = " + input.x.ToString("0.0") + "\n" +
                "verticalVelocity = " + verticalVelocity.ToString("0.00") + "\n" +
                "groundDistance = " + groundDistance.ToString("0.00") + "\n" +
                "groundAngle = " + GroundAngle().ToString("0.00") + "\n" +
                "useGravity = " + _rigidbody.useGravity.ToString() + "\n" +
                "colliderIsTrigger = " + _capsuleCollider.isTrigger.ToString() + "\n" +

                "\n" + "--- Movement Bools ---" + "\n" +
                "onGround = " + isGrounded.ToString() + "\n" +
                "lockMovement = " + lockMovement.ToString() + "\n" +
                "stopMove = " + stopMove.ToString() + "\n" +
                "sliding = " + isSliding.ToString() + "\n" +
                "sprinting = " + isSprinting.ToString() + "\n" +
                "crouch = " + isCrouching.ToString() + "\n" +
                "strafe = " + isStrafing.ToString() + "\n" +
                "landHigh = " + landHigh.ToString() + "\n" +

                "\n" + "--- Actions Bools ---" + "\n" +
                "roll = " + isRolling.ToString() + "\n" +
                "isJumping = " + isJumping.ToString() + "\n" +
                "ragdoll = " + ragdolled.ToString() + "\n" +
                "actions = " + actions.ToString() + "\n" +
                "customAction = " + customAction.ToString() + "\n" + additionalText;
        }
        return debugInfo;
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying && debugWindow)
        {
            //// debug auto crouch
            Vector3 posHead = transform.position + Vector3.up * ((colliderHeight * .5f) - colliderRadius);
            Ray ray1 = new Ray(posHead, Vector3.up);
            Gizmos.DrawWireSphere(ray1.GetPoint((headDetect - (colliderRadius * 0.1f))), colliderRadius * .9f);
            //// debug stopmove            
            Ray ray3 = new Ray(transform.position + new Vector3(0, stopMoveHeight, 0), transform.forward);
            Debug.DrawRay(ray3.origin, ray3.direction * (_capsuleCollider.radius + stopMoveDistance), Color.blue);
            //// debug slopelimit            
            Ray ray4 = new Ray(transform.position + new Vector3(0, colliderHeight / 3.5f, 0), transform.forward);
            Debug.DrawRay(ray4.origin, ray4.direction * 1f, Color.cyan);
            //// debug stepOffset
            var dir = isStrafing && input.magnitude > 0 ? (transform.right * input.x + transform.forward * input.y).normalized : transform.forward;
            Ray ray5 = new Ray((transform.position + new Vector3(0, stepOffsetEnd, 0) + dir * ((_capsuleCollider).radius + .05f)), Vector3.down);
            Debug.DrawRay(ray5.origin, ray5.direction * (stepOffsetEnd - stepOffsetStart), Color.yellow);
        }
    }

    #endregion

    [System.Serializable]
    public class vMovementSpeed
    {
        [Tooltip("Rotation speed of the character")]
        public float rotationSpeed = 10f;
        [Tooltip("Character will walk by default and run when the sprint input is pressed. The Sprint animation will not play")]
        public bool walkByDefault = false;
        [Tooltip("Speed to Walk using rigibody force or extra speed if you're using RootMotion")]
        public float walkSpeed = 2f;
        [Tooltip("Speed to Run using rigibody force or extra speed if you're using RootMotion")]
        public float runningSpeed = 3f;
        [Tooltip("Speed to Sprint using rigibody force or extra speed if you're using RootMotion")]
        public float sprintSpeed = 4f;
        [Tooltip("Speed to Crouch using rigibody force or extra speed if you're using RootMotion")]
        public float crouchSpeed = 2f;
    }
}
