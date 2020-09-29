using System.Collections;
using System.Collections.Generic;
using UnityEngine;



// here you can modify the Melee Combat inputs
// if you want to modify the Basic Locomotion inputs, go to the PlayerInput
using YanProject;
using System.Linq;

[ClassHeader("MELEE INPUT MANAGER", iconName = "inputIcon")]
public class MeleeCombatInput : PlayerInputs, IMeleeFighter
{
    [System.Serializable]
    public class OnUpdateEvent : UnityEngine.Events.UnityEvent<MeleeCombatInput> { }

    #region Variables             

    [EditorToolbar("Inputs")]
    [Header("Melee Inputs")]
    public GenericInput weakAttackInput = new GenericInput("Mouse0", "RB", "RB");
    public GenericInput strongAttackInput = new GenericInput("Alpha1", false, "RT", true, "RT", false);
    public GenericInput blockInput = new GenericInput("Mouse1", "LB", "LB");

    protected MeleeManager meleeManager;
    public bool isAttacking { get; protected set; }
    public bool isBlocking { get; protected set; }
    public bool isArmed { get { return meleeManager != null && (meleeManager.rightWeapon != null || (meleeManager.leftWeapon != null && meleeManager.leftWeapon.meleeType != MeleeWeapon.MeleeType.OnlyDefense)); } }

    [HideInInspector]
    public OnUpdateEvent onUpdateInput;
    [HideInInspector]
    public bool lockMeleeInput;

    public void SetLockMeleeInput(bool value)
    {
        lockMeleeInput = value;

        if (value)
        {
            isAttacking = false;
            isBlocking = false;
            cc.isStrafing = false;
        }
    }

    #endregion

    public virtual bool lockInventory
    {
        get
        {
            return isAttacking || cc.isDead;
        }
    }

    protected override void FixedUpdate()
    {
        UpdateMeleeAnimations();
        UpdateAttackBehaviour();
        base.FixedUpdate();
    }

    protected override void InputHandle()
    {
        if (cc == null)
            return;

        if (MeleeAttackConditions && !lockMeleeInput)
        if (!lockMeleeInput)
        {
            MeleeWeakAttackInput();
            MeleeStrongAttackInput();
            BlockingInput();
        }
        else
        {
            isBlocking = false;
        }

        if (!isAttacking)
        {
            base.InputHandle();
        }
        onUpdateInput.Invoke(this);
    }

    #region MeleeCombat Input Methods

    protected virtual void MeleeWeakAttackInput()
    {
        if (cc.anime == null) return;

        if (weakAttackInput.GetButtonDown() && MeleeAttackStaminaConditions())
        {
           cc.anime.SetInteger("AttackID", meleeManager.GetAttackID());
           cc.anime.SetTrigger("WeakAttack");
        }
    }/// WEAK ATK INPUT

    protected virtual void MeleeStrongAttackInput()
    {
        if (cc.anime == null) return;

        if (strongAttackInput.GetButtonDown() && MeleeAttackStaminaConditions())
        {
             cc.anime.SetInteger("AttackID", meleeManager.GetAttackID());
             cc.anime.SetTrigger("StrongAttack");
        }
    }/// STRONG ATK INPUT

    protected virtual void BlockingInput()
    {
        if (cc.anime == null) return;

        isBlocking = blockInput.GetButton() && cc.currentStamina > 0;
    }/// BLOCK INPUT

    #endregion

    #region Conditions

    protected virtual bool MeleeAttackStaminaConditions()
    {
        var result = cc.currentStamina - meleeManager.GetAttackStaminaCost();
        return result >= 0;
    }

    protected virtual bool MeleeAttackConditions
    {
        get
        {
            if (meleeManager == null) meleeManager = GetComponent<MeleeManager>();
            return meleeManager != null && !cc.customAction && !cc.lockMovement && !cc.isCrouching;
        }
    }

    #endregion

    #region Update Animations

    protected virtual void UpdateMeleeAnimations()
    {
        if (cc.anime == null || meleeManager == null) return;
        cc.anime.SetInteger("AttackID", meleeManager.GetAttackID());
        cc.anime.SetInteger("DefenseID", meleeManager.GetDefenseID());
        cc.anime.SetBool("IsBlocking", isBlocking);
        cc.anime.SetFloat("MoveSet_ID", meleeManager.GetMoveSetID(), .2f, Time.deltaTime);
    }

    protected virtual void UpdateAttackBehaviour()
    {
        if (cc.IsAnimatorTag("Attack")) return;
        // lock the speed to stop the character from moving while attacking
        cc.lockSpeed = cc.IsAnimatorTag("Attack") || isAttacking;
        // force root motion animation while attacking
        cc.forceRootMotion = cc.IsAnimatorTag("Attack") || isAttacking;
    }

    #endregion

    #region Melee Methods

    public void OnEnableAttack()
    {
        cc.currentStaminaRecoveryDelay = meleeManager.GetAttackStaminaRecoveryDelay();
        cc.currentStamina -= meleeManager.GetAttackStaminaCost();
        cc.lockRotation = true;
        isAttacking = true;
    }

    public void OnDisableAttack()
    {
        cc.lockRotation = false;
        isAttacking = false;
    }

    public void ResetAttackTriggers()
    {
         cc.anime.ResetTrigger("WeakAttack");
         cc.anime.ResetTrigger("StrongAttack");
    }

    public void BreakAttack(int breakAtkID)
    {
        ResetAttackTriggers();
        OnRecoil(breakAtkID);
    }

    public void OnRecoil(int recoilID)
    {
        cc.anime.SetInteger("RecoilID", recoilID);
        cc.anime.SetTrigger("TriggerRecoil");
        cc.anime.SetTrigger("ResetState");
        cc.anime.ResetTrigger("WeakAttack");
        cc.anime.ResetTrigger("StrongAttack");
    }

    public void OnReceiveAttack(Damage damage, IMeleeFighter attacker)
    {
        // character is blocking
        if (!damage.ignoreDefense && isBlocking && meleeManager != null && meleeManager.CanBlockAttack(attacker.character.transform.position))
        {
            var damageReduction = meleeManager.GetDefenseRate();
            if (damageReduction > 0)
                damage.ReduceDamage(damageReduction);
            if (attacker != null && meleeManager != null && meleeManager.CanBreakAttack())
                attacker.OnRecoil(meleeManager.GetDefenseRecoilID());
            meleeManager.OnDefense();
            cc.currentStaminaRecoveryDelay = damage.staminaRecoveryDelay;
            cc.currentStamina -= damage.staminaBlockCost;
        }
        // apply damage
        damage.hitReaction = !isBlocking;
        cc.TakeDamage(damage);
    }

    public Character character
    {
        get { return cc; }
    }

    #endregion
}