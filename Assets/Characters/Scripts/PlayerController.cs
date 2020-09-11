﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : PlayerAnimator
{
    public static PlayerController instance;

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

        //know if has enough stamina to make this action
        bool staminaConditions = currentStamina > jumpStamina;
        //conditions to do this action
        bool jumpConditions = !isCrouching && isGrounded && !actions && staminaConditions && !isJumping;
        //retun if condtions is false
        if (!jumpConditions) return;
        //trigger jump behaviour
        jumpCounter = jumpTimer;
        isJumping = true;
        //trigger jump animations
        if (input.sqrMagnitude < .1f)
            anime.CrossFadeInFixedTime("Jump", .1f);
        else
            anime.CrossFadeInFixedTime("JumpMove", .2f);
        //reduce stamina
        //todo here
    }

    public virtual void Roll()
    {
        bool staminaCondition = currentStamina > rollStamina;
        //can roll even if it's on a quickturn or quckstop animation
        bool actionsRoll = !actions || (actions && (quickStop));
        //general conditions to roll
        bool rollConditions = (input != Vector2.zero || speed > .25f) && actionsRoll && isGrounded && staminaCondition && !isJumping;

        if (!rollConditions || isRolling) return;

        anime.CrossFadeInFixedTime("Roll", .1f);
        //to do stamina
    }

    public virtual void Sprint(bool value)
    {
        isSprinting = value;
    }

    public virtual void Strafe()
    {
        isStrafing = !isStrafing;
    }

    public virtual void Crounch()
    {
        if (isGrounded && !actions)
        {
            if (isCrouching && CanExitCrounch())
                isCrouching = false;
            else
                isCrouching = true;
        }
    }

    #endregion


    #region Crouch Methods

    protected virtual bool CanExitCrounch()
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

    #endregion
}
