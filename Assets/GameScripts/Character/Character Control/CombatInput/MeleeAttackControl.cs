﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttackControl : StateMachineBehaviour
{
    [Tooltip("normalizedTime of Active Damage")]
    public float startDamage = 0.05f;
    [Tooltip("normalizedTime of Disable Damage")]
    public float endDamage = 0.9f;
    [Tooltip("Allow the character to move/rotate")]
    public float allowMovementAt = 0.9f;
    public int damageMultiplier;
    public int recoilID;
    public int reactionID;

    public AttackType meleeAttackType = AttackType.Unarmed;
    [Tooltip("You can use a name as reference to trigger a custom HitDamageParticle")]
    public string attackName;
    [HideInInspector]
    [Header("This work with vMeleeManager to active vMeleeAttackObject from bodyPart and id")]
    public List<string> bodyParts = new List<string> { "RightLowerArm" };
    public bool ignoreDefense;
    public bool activeRagdoll;
    [Tooltip("Check true in the last attack of your combo to reset the triggers")]
    public bool resetAttackTrigger;
    private bool isActive;
    public bool debug;
    private IAttackListerner mFighter;
    private bool isAttacking;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        mFighter = animator.GetComponent<IAttackListerner>();
        isAttacking = true;
        if (mFighter != null)
            mFighter.OnDisableAttack();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.normalizedTime % 1 >= startDamage && stateInfo.normalizedTime % 1 <= endDamage && !isActive)
        {
            Debug.Log(animator.name + " attack " + attackName + " enable damage in " + System.Math.Round(stateInfo.normalizedTime % 1, 2));
            isActive = true;
            ActiveDamage(animator, true);
        }
        else if (stateInfo.normalizedTime % 1 > endDamage && isActive)
        {
            Debug.Log(animator.name + " attack " + attackName + " enable damage in " + System.Math.Round(stateInfo.normalizedTime % 1, 2));
            isActive = true;
            ActiveDamage(animator, false);
        }

        if (stateInfo.normalizedTime % 1 > allowMovementAt && isAttacking)
        {
            isAttacking = false;
            if (mFighter != null)
                mFighter.OnDisableAttack();
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (isActive)
        {
            isActive = false;
            ActiveDamage(animator, false);
        }

        isAttacking = false;
        if (mFighter != null)
            mFighter.OnDisableAttack();

        if (mFighter != null && resetAttackTrigger)
            mFighter.ResetAttackTriggers();

        Debug.Log(animator.name + " attack " + attackName + " stateExit");
    }

    void ActiveDamage(Animator animator, bool value)
    {
        var meleeManager = animator.GetComponent<MeleeManager>();
        if (meleeManager)
            meleeManager.SetActiveAttack(bodyParts, meleeAttackType, value, damageMultiplier, recoilID, reactionID, ignoreDefense, activeRagdoll, attackName);
    }

}

