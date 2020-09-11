using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerAnimator : PlayerMotor
{
    protected virtual void OnAnimatorMove()
    {
        if (!this.enabled) return;

        //many things todo here
        ControlSpeed(2f);
    }

    protected virtual void UpdateAnimator()
    {
        //update main animations
        if (anime == null || !anime.enabled) return;

    }
}
