﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerAnimator : PlayerMotor
{
    #region Variables        
    // match cursorObject to help animation to reach their cursorObject
    [HideInInspector]
    public Transform matchTarget;
    // head track control, if you want to turn off at some point, make it 0
    [HideInInspector]
    public float lookAtWeight;

    private float randomIdleCount, randomIdle;
    private Vector3 lookPosition;
    private float _speed = 0;
    private float _direction = 0;
    private bool triggerDieBehaviour;

    int baseLayer { get { return anime.GetLayerIndex("Base Layer"); } }
    int underBodyLayer { get { return anime.GetLayerIndex("UnderBody"); } }
    int rightArmLayer { get { return anime.GetLayerIndex("RightArm"); } }
    int leftArmLayer { get { return anime.GetLayerIndex("LeftArm"); } }
    int upperBodyLayer { get { return anime.GetLayerIndex("UpperBody"); } }
    int fullbodyLayer { get { return anime.GetLayerIndex("FullBody"); } }

    #endregion

    public void OnAnimatorMove()
    {

        if (!this.enabled) return;

        UpdateAnimator();                // call ThirdPersonAnimator methods

        // we implement this function to override the default root motion.
        // this allows us to modify the positional speed before it's applied.
        if (isGrounded)
        {
            if (customAction && !isRolling)
                transform.rotation = anime.rootRotation;

            //strafe extra speed
            if (isStrafing)
            {
                var _speed = Mathf.Abs(strafeMagnitude);
                if (_speed <= .5f)
                    ControlSpeed(strafeSpeed.walkSpeed);
                else if (_speed > .5f && _speed <= 1f)
                    ControlSpeed(strafeSpeed.runningSpeed);
                else
                    ControlSpeed(strafeSpeed.sprintSpeed);

                if (isCrouching)
                    ControlSpeed(strafeSpeed.crouchSpeed);
            }
            else if (!isStrafing)
            {
                //free extra speed
                if (speed <= .5f)
                    ControlSpeed(freeSpeed.walkSpeed);
                else if (speed > .5 && speed <= 1f)
                    ControlSpeed(freeSpeed.runningSpeed);
                else
                    ControlSpeed(freeSpeed.sprintSpeed);

                if (isCrouching)
                    ControlSpeed(freeSpeed.crouchSpeed);
            }
        }
    }

    public virtual void UpdateAnimator()
    {
        //update main animations
        if (anime == null || !anime.enabled) return;

        LayerControl();
        ActionsControl();

        RandomIdle();

        // trigger by input
        RollAnimation();
        // trigger at any time using conditions
        TriggerLandHighAnimation();

        LocomotionAnimation();
        DeadAnimation();
    }

    public void LayerControl()
    {
        baseLayerInfo = anime.GetCurrentAnimatorStateInfo(baseLayer);
        underBodyInfo = anime.GetCurrentAnimatorStateInfo(underBodyLayer);
        rightArmInfo = anime.GetCurrentAnimatorStateInfo(rightArmLayer);
        leftArmInfo = anime.GetCurrentAnimatorStateInfo(leftArmLayer);
        upperBodyInfo = anime.GetCurrentAnimatorStateInfo(upperBodyLayer);
        fullBodyInfo = anime.GetCurrentAnimatorStateInfo(fullbodyLayer);
    }

    public void ActionsControl()
    {
        // to have better control of your actions, you can filter the animations state using bools 
        // this way you can know exactly what animation state the character is playing

        landHigh = baseLayerInfo.IsName("LandHigh");
        quickStop = baseLayerInfo.IsName("QuickStop");

        isRolling = baseLayerInfo.IsName("Roll");
        inTurn = baseLayerInfo.IsName("TurnOnSpot");

        // locks player movement while a animation with tag 'LockMovement' is playing
        lockMovement = IsAnimatorTag("LockMovement");
        // ! -- you can add the Tag "CustomAction" into a AnimatonState and the character will not perform any Melee action -- !            
        customAction = IsAnimatorTag("CustomAction");
    }

    #region Locomotion Animations

    void RandomIdle()
    {
        if (input != Vector2.zero || actions) return;

        if (randomIdleTime > 0)
        {
            if (input.sqrMagnitude == 0 && !isCrouching && _capsuleCollider.enabled && isGrounded)
            {
                randomIdleCount += Time.fixedDeltaTime;
                if (randomIdleCount > 6)
                {
                    randomIdleCount = 0;
                    anime.SetTrigger("IdleRandomTrigger");
                    anime.SetInteger("IdleRandom", Random.Range(1, 4));
                }
            }
            else
            {
                randomIdleCount = 0;
                anime.SetInteger("IdleRandom", 0);
            }
        }
    }

    /// <summary>
    /// Atualiza as animações - Chama as animações conforme a necessidade
    /// </summary>
    public void LocomotionAnimation()
    {
        anime.SetBool("IsStrafing", isStrafing);
        anime.SetBool("IsCrouching", isCrouching);
        anime.SetBool("IsGrounded", isGrounded);
        anime.SetBool("IsDead", isDead);
        anime.SetFloat("GroundDistance", groundDistance);

        //if is not on ground set VerticalVelocity 
        if (!isGrounded)
            anime.SetFloat("VerticalVelocity", verticalVelocity);

        if (isStrafing)
        {
            // strafe movement get the input 1 or -1
            anime.SetFloat("InputHorizontal", !stopMove && !lockMovement ? direction : 0f, .25f, Time.deltaTime);
            anime.SetFloat("InputVertical", !stopMove && !lockMovement ? speed : 0f, .25f, Time.deltaTime);
        }
        else
        {
            var dir = transform.InverseTransformDirection(targetDirection);
            dir.z *= speed;
            anime.SetFloat("InputVertical", !stopMove && !lockMovement ? dir.z : 0f, .25f, Time.deltaTime);
            anime.SetFloat("InputHorizontal", !stopMove && !lockMovement ? dir.x : 0f, .25f, Time.deltaTime);
        }

        if (turnOnSpotAnim)
        {
            GetTurnOnSpotDirection(transform, Camera.main.transform, ref _speed, ref _direction, input);
            FreeTurnOnSpot(_direction * 180);//to call this need be active, animations to turn
        }
    }

    public void FreeTurnOnSpot(float direction)
    {
        bool inTransition = anime.IsInTransition(0);
        float directionDampTime = inTurn || inTransition ? 1000000 : 0;
        anime.SetFloat("TurnOnSpotDirection", direction, directionDampTime, Time.deltaTime);
    }

    public void GetTurnOnSpotDirection(Transform root, Transform camera, ref float _speed, ref float _direction, Vector2 input)
    {
        Vector3 rootDirection = root.forward;
        Vector3 stickDirection = new Vector3(input.x, 0, input.y);

        // Get camera rotation.    
        Vector3 CameraDirection = camera.forward;
        CameraDirection.y = .0f; // kill Y
        Quaternion referentialShift = Quaternion.FromToRotation(Vector3.forward, CameraDirection);
        // Convert joystick input in Worldspace coordinates            
        Vector3 moveDirection = rotateByWorld ? stickDirection : referentialShift * stickDirection;

        Vector2 speedVec = new Vector2(input.x, input.y);
        _speed = Mathf.Clamp(speedVec.magnitude, 0, 1);

        if (_speed > .01f) // dead zone
        {
            Vector3 axis = Vector3.Cross(rootDirection, moveDirection);
            _direction = Vector3.Angle(rootDirection, moveDirection) / 180.0f * (axis.y < 0 ? -1 : 1);
        }
        else
        {
            _direction = .0f;
        }
    }

    #endregion

    #region Action Animations  

    protected virtual void RollAnimation()
    {
        if (isRolling)
        {
            autoCrouch = true;

            if (isStrafing && (input != Vector2.zero || speed > 0.25f))
            {
                var rot = rollRotation;
                var eulerAngles = new Vector3(transform.eulerAngles.x, rot.eulerAngles.y, transform.eulerAngles.z);
                transform.eulerAngles = eulerAngles;
            }

            if (baseLayerInfo.normalizedTime > 0.1f && baseLayerInfo.normalizedTime < 0.3f)
                _rigidbody.useGravity = false;

            // prevent the character to rolling up 
            if (verticalVelocity >= 1)
                _rigidbody.velocity = Vector3.ProjectOnPlane(_rigidbody.velocity, groundHit.normal);

            // reset the rigidbody a little ealier to the character fall while on air
            if (baseLayerInfo.normalizedTime > 0.3f)
                _rigidbody.useGravity = true;
        }
    }

    protected virtual Quaternion rollRotation
    {
        get
        {
            Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDirection, 25f * Time.fixedDeltaTime, 0.0f);
            return Quaternion.LookRotation(newDir);
        }
    }

    /// <summary>
    /// Chama sequenquência de animação de morte do personagem
    /// </summary>
    protected virtual void DeadAnimation()
    {
        //life system todo
        if (!isDead) return;

        if (!triggerDieBehaviour)
        {
            triggerDieBehaviour = true;
            DeathBehaviour();
        }

        // death by animation
        if (deathBy == DeathBy.Animation)
        {
            if (fullBodyInfo.IsName("Dead"))
            {
                if (fullBodyInfo.normalizedTime >= .99f && groundDistance <= .15f)
                    RemoveComponents();
            }
        }
        // death by animation & ragdoll after a time
        else if (deathBy == DeathBy.AnimationWithRagdoll)
        {
            if (fullBodyInfo.IsName("Dead"))
            {
                // activate the ragdoll after the animation finish played
                if (fullBodyInfo.normalizedTime >= 0.8f)
                    onActiveRagdoll.Invoke();
            }
        }
        // death by ragdoll
        else if (deathBy == DeathBy.Ragdoll)
            onActiveRagdoll.Invoke();
    }

    public virtual void SetActionState(int value)
    {
        anime.SetInteger("ActionState", value);
    }

    public virtual void MatchTarget(Vector3 matchPosition, Quaternion matchRotation, AvatarTarget target, MatchTargetWeightMask weightMask, float normalisedStartTime, float normalisedEndTime)
    {
        if (anime.isMatchingTarget || anime.IsInTransition(0))
            return;

        float normalizeTime = Mathf.Repeat(anime.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f);

        if (normalizeTime > normalisedEndTime)
            return;

        anime.MatchTarget(matchPosition, matchRotation, target, weightMask, normalisedStartTime, normalisedEndTime);
    }

    #endregion

    #region Trigger Animations       

    public void TriggerAnimationState(string animationClip, float transition)
    {
        anime.CrossFadeInFixedTime(animationClip, transition);
    }

    public bool IsAnimatorTag(string tag)
    {
        if (anime == null) return false;
        if (baseLayerInfo.IsTag(tag)) return true;
        if (underBodyInfo.IsTag(tag)) return true;
        if (rightArmInfo.IsTag(tag)) return true;
        if (leftArmInfo.IsTag(tag)) return true;
        if (upperBodyInfo.IsTag(tag)) return true;
        if (fullBodyInfo.IsTag(tag)) return true;
        return false;
    }

    void TriggerLandHighAnimation()
    {
        if (landHigh)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            sInput.instance.GamepadVibration(.25f);
#endif
        }
    }

    #endregion
}
