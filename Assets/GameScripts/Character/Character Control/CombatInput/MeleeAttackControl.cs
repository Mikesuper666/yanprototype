using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttackControl : StateMachineBehaviour
{
    [Tooltip("normalizedTime of Active Damage")]
    public float startDamage = .05f;
    [Tooltip("normalizedTime of Disable Damage")]
    public float endDamage = 0.9f;
    private bool isActive;

    public AttackType meleeAttackType = AttackType.Unarmed;
    [Tooltip("You can use a name as reference to trigger a custom HitDamageParticle")]
    public string attackName;

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
    }
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    { 
    }
    void ActiveDamage(Animator animator, bool value)
    { }

    }

