using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerAnimator : PlayerMotor
{
    protected virtual void OnAnimatorMove()
    {
        if (!this.enabled) return;

        UpdateAnimator();                // call ThirdPersonAnimator methods

        //we implemented this funcition to override the default motion.
        //this allows us modify the positional speed before it's applied
        if (isGrounded)
        {
            if (customAction && !isRolling)
                transform.rotation = anime.rootRotation;

            //strafe extra speed
            if (isStrafing)
            {
                var _speed = Mathf.Abs(strafeMagnitude);
                if (_speed <= .5f)
                    ControlSpeed(strafeSpeed.runningSpeed);
                else
                    ControlSpeed(strafeSpeed.sprintSpeed);

                if (isCrouching)
                    ControlSpeed(strafeSpeed.crouchSpeed);
            }
            else if (!isStrafing)
            {
                //free extra speed
                if (speed <= 0.5f)
                    ControlSpeed(freeSpeed.walkSpeed);
                else if (speed > 0.5 && speed <= 1f)
                    ControlSpeed(freeSpeed.runningSpeed);
                else
                    ControlSpeed(freeSpeed.sprintSpeed);

                if (isCrouching)
                    ControlSpeed(freeSpeed.crouchSpeed);
            }
        }
    }

    protected virtual void UpdateAnimator()
    {
        //update main animations
        if (anime == null || !anime.enabled) return;

        LocomotionAnimation();
    }

    public void LocomotionAnimation()
    {
        anime.SetBool("IsStrafing", isStrafing);
        anime.SetBool("IsCrouching", isCrouching);
        anime.SetBool("IsGrounded", isGrounded);
       // anime.SetBool("isDead", isDead);
        anime.SetFloat("GroundDistance", groundDistance);

        if (!isGrounded)
            anime.SetFloat("VerticalVelocity", verticalVelocity);

        if (isStrafing)
        {
            // strafe movement get the input 1 or -1
            anime.SetFloat("InputHorizontal", !stopMove && !lockMovement ? direction : 0f, 0.25f, Time.deltaTime);
            anime.SetFloat("InputVertical", !stopMove && !lockMovement ? speed : 0f, 0.25f, Time.deltaTime);
        }
        else
        {
            var dir = transform.InverseTransformDirection(targetDirection);
            dir.z *= speed;
            anime.SetFloat("InputVertical", !stopMove && !lockMovement ? dir.z : 0f, 0.25f, Time.deltaTime);
            anime.SetFloat("InputHorizontal", !stopMove && !lockMovement ? dir.x : 0f, 0.25f, Time.deltaTime);
        }

     //   if (turnOnSpotAnim)
       // {
        //    GetTurnOnSpotDirection(transform, Camera.main.transform, ref _speed, ref _direction, input);
         //   FreeTurnOnSpot(_direction * 180);
        //}
    }
}
