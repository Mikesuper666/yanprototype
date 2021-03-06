﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// here you can modify the Melee Combat inputs
// if you want to modify the Basic Locomotion inputs, go to the PlayerInput
[ClassHeader ("PLAYER CONTROLLER MANAGER", iconName = "inputIcon")]
public class PlayerController : PlayerAnimator
{
    public static PlayerController instance;

    protected virtual void Awake()
    {
        StartCoroutine(UpdateRaycast()); // limit raycasts calls for better performance
    }

    protected override void Start()
    {
        base.Start();
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            this.gameObject.name = gameObject.name + " PlayerInstance";
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
    }

    #region Locomotion Actions

    public virtual void Jump(bool consumeStamina = false)
    {
        if (customAction) return;

        // know if has enough stamina to make this action
        bool staminaConditions = currentStamina > jumpStamina;
        // conditions to do this action
        bool jumpConditions = !isCrouching && isGrounded && !actions && staminaConditions && !isJumping;
        // return if jumpCondigions is false
        if (!jumpConditions) return;
        // trigger jump behaviour
        jumpCounter = jumpTimer;
        isJumping = true;
        // trigger jump animations
        if (input.sqrMagnitude < 0.1f)
            anime.CrossFadeInFixedTime("Jump", 0.1f);
        else
            anime.CrossFadeInFixedTime("JumpMove", .2f);
        // reduce stamina
        if (consumeStamina)
        {
            ReduceStamina(jumpStamina, false);
            currentStaminaRecoveryDelay = 1f;
        }
    }

    public virtual void Roll()
    {
        bool staminaCondition = currentStamina > rollStamina;
        // can roll even if it's on a quickturn or quickstop animation
        bool actionsRoll = !actions || (actions && (quickStop));
        // general conditions to roll
        bool rollConditions = (input != Vector2.zero || speed > 0.25f) && actionsRoll && isGrounded && staminaCondition && !isJumping;

        if (!rollConditions || isRolling) return;

        anime.CrossFadeInFixedTime("Roll", 0.1f);
        ReduceStamina(rollStamina, false);
        currentStaminaRecoveryDelay = 2f;
    }

    public virtual void Sprint(bool value)
    {
        if (value)
        {
            if (currentStamina > 0 && input.sqrMagnitude > 0.1f)
            {
                if (isGrounded && !isCrouching)
                    isSprinting = !isSprinting;
            }
        }
        else if (currentStamina <= 0 || input.sqrMagnitude < 0.1f || isCrouching || !isGrounded || actions || isStrafing && !strafeSpeed.walkByDefault && (direction >= 0.5 || direction <= -0.5 || speed <= 0))
        {
            isSprinting = false;
        }
    }

    public virtual void Strafe()
    {
        isStrafing = !isStrafing;
    }//called by input

    public virtual void Crounch()
    {
        if (isGrounded && !actions)
        {
            if (isCrouching && CanExitCrouch())
                isCrouching = false;
            else
                isCrouching = true;
        }
    }// Use another transform as  reference to rotate

    public virtual void RotateWithAnotherTransform(Transform referenceTransform)
    {
        var newRotation = new Vector3(transform.eulerAngles.x, referenceTransform.eulerAngles.y, transform.eulerAngles.z);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(newRotation), strafeSpeed.rotationSpeed * Time.deltaTime);
        targetRotation = transform.rotation;
    }

    #endregion

    #region Update Raycasts  

    protected override void OnTriggerStay(Collider other)
    {
        try
        {
            CheckForAutoCrouch(other);
        }
        catch (UnityException e)
        {
            Debug.LogWarning(e.Message);
        }
        base.OnTriggerStay(other);
    } /// Call this in OnTriggerEnter or OnTriggerStay to check if enter in triggerActions 

    protected override void OnTriggerExit(Collider other)
    {
        AutoCrouchExit(other);
        base.OnTriggerExit(other);
    }/// Call this in OnTriggerExit to check if exit of triggerActions 

    protected IEnumerator UpdateRaycast()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();

            AutoCrouch();
            //StopMove();
        }
    }

    #endregion

    #region Crouch Methods

    protected virtual void AutoCrouch()
    {
        if (autoCrouch)
            isCrouching = true;

        if (autoCrouch && !inCrouchArea && CanExitCrouch())
        {
            autoCrouch = false;
            isCrouching = false;
        }
    }

    protected virtual bool CanExitCrouch()
    {
        //radius of spherecast
        float radius = _capsuleCollider.radius * .9f;
        //position of spherecast origin in base capsule
        Vector3 pos = transform.position + Vector3.up * ((colliderHeight * .5f) - colliderRadius);
        //Ray for sphereCast
        Ray ray2 = new Ray(pos, Vector3.up);

        //Sphere cast around the base of capsule for check ground distance
        if (Physics.SphereCast(ray2, radius, out groundHit, headDetect - (colliderRadius * .1f), autoCrouchLayer))
            return false;
        else
            return true;
    }

    protected virtual void AutoCrouchExit(Collider other)
    {
        if (other.CompareTag("AutoCrouch"))
        {
            inCrouchArea = false;
        }
    }

    protected virtual void CheckForAutoCrouch(Collider other)
    {
        if (other.gameObject.CompareTag("AutoCrouch"))
        {
            autoCrouch = true;
            inCrouchArea = true;
        }
    }

    #endregion
}
