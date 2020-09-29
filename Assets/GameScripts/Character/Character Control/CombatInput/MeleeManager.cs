using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using static MeleeWeapon;
using YanProject;

public class MeleeManager : mMonoBehaviour
{
    #region SeralizedProperties in CustomEditor
    public List<BodyMember> Members = new List<BodyMember>();
    public Damage defaultDamage = new Damage(10);
    public HitProperties hitProperties;
    public MeleeWeapon leftWeapon, rightWeapon;
    public OnHitEvent onDamageHit, onRecoilHit;
    #endregion

    [Tooltip("NPC ONLY- Ideal distance for the attack")]
    public float defaultAttackDistance = 1f;
    [Tooltip("Default cost for stamina when attack")]
    public float defaultStaminaCost = 20f;
    [Tooltip("Default recovery delay for stamina when attack")]
    public float defaultStaminaRecoveryDelay = 1f;
    [Range(0, 100)]
    public int defaultDefenseRate = 50;
    [Range(0, 180)]
    public float defaultDefenseRange = 90;

    [HideInInspector]
    public IMeleeFighter fighter;
    private int damageMultiplier;
    private int currentRecoilID;
    private int currentReactionID;
    private bool ignoreDefense;
    private bool activeRagdoll;
    private bool inRecoil;
    private string attackName;

    protected virtual void Start()
    {
        Init();
    }

    protected virtual void Init()
    {
        fighter = gameObject.GetMeleeFighter();
        ///Initialize all bodyMembers and weapons
        foreach (BodyMember member in Members)
        {
            ///damage of member will be always the defaultDamage
            //member.attackObject.damage = defaultDamage;
            member.attackObject.damage.damageValue = defaultDamage.damageValue;
            if (member.bodyPart == HumanBodyBones.LeftLowerArm.ToString())
            {
                var weapon = member.attackObject.GetComponentInChildren<MeleeWeapon>();
                leftWeapon = weapon;
            }
            if (member.bodyPart == HumanBodyBones.RightLowerArm.ToString())
            {
                var weapon = member.attackObject.GetComponentInChildren<MeleeWeapon>();
                rightWeapon = weapon;
            }
            member.attackObject.meleeManager = this;
            member.SetActiveDamage(false);
        }

        if (leftWeapon != null)
        {
            leftWeapon.SetActiveDamage(false);
            leftWeapon.meleeManager = this;
        }
        if (rightWeapon != null)
        {
            rightWeapon.meleeManager = this;
            rightWeapon.SetActiveDamage(false);
        }
    }// Init properties

    #region SETS

    public virtual void SetActiveAttack(List<string> bodyParts, AttackType type, bool active = true, int damageMultiplier = 0, int recoilID = 0, int reactionID = 0, bool ignoreDefense = false, bool activeRagdoll = false, string attackName = "")
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            var bodyPart = bodyParts[i];
            SetActiveAttack(bodyPart, type, active, damageMultiplier, recoilID, reactionID, ignoreDefense, activeRagdoll, attackName);
        }
    }/// Set active Multiple Parts to attack

    public virtual void SetActiveAttack(string bodyPart, AttackType type, bool active = true, int damageMultiplier = 0, int recoilID = 0, int reactionID = 0, bool ignoreDefense = false, bool activeRagdoll = false, string attackName = "")
    {
        this.damageMultiplier = damageMultiplier;
        currentRecoilID = recoilID;
        currentReactionID = reactionID;
        this.ignoreDefense = ignoreDefense;
        this.activeRagdoll = activeRagdoll;
        this.attackName = attackName;

        if (type == AttackType.Unarmed)
        {
            /// find attackObject by bodyPart
            var attackObject = Members.Find(member => member.bodyPart == bodyPart);
            if (attackObject != null)
            {
                attackObject.SetActiveDamage(active);
            }
        }
        else
        {   ///if AttackType == MeleeWeapon
            ///this work just for Right and Left Lower Arm         
            if (bodyPart == "RightLowerArm" && rightWeapon != null)
            {
                rightWeapon.meleeManager = this;
                rightWeapon.SetActiveDamage(active);
            }
            else if (bodyPart == "LeftLowerArm" && leftWeapon != null)
            {
                leftWeapon.meleeManager = this;
                leftWeapon.SetActiveDamage(active);
            }
        }
    }/// Set active Single Part to attack

    public virtual void OnDamageHit(HitInfo hitInfo)
    {
        Damage damage = new Damage(hitInfo.attackObject.damage);
        damage.sender = transform;
        damage.reaction_id = currentReactionID;
        damage.recoil_id = currentRecoilID;
        if (this.activeRagdoll) damage.activeRagdoll = this.activeRagdoll;
        if (this.attackName != string.Empty) damage.attackName = this.attackName;
        if (this.ignoreDefense) damage.ignoreDefense = this.ignoreDefense;
        /// Calc damage with multiplier 
        /// and Call ApplyDamage of attackObject 
        damage.damageValue *= damageMultiplier > 1 ? damageMultiplier : 1;
        hitInfo.attackObject.ApplyDamage(hitInfo.hitBox, hitInfo.targetCollider, damage);
        onDamageHit.Invoke(hitInfo);
    }// Listener of Damage Event

    /// <summary>
    /// Listerner do Recoil
    /// </summary>
    /// <param name="hitInfo"></param>
    public virtual void OnRecoilHit(HitInfo hitInfo)
    {
        if (hitProperties.useRecoil && InRecoilRange(hitInfo) && !inRecoil)
        {
            inRecoil = true;
            var id = currentRecoilID;
            if (fighter != null) fighter.OnRecoil(id);
            onRecoilHit.Invoke(hitInfo);
            Invoke("ResetRecoil", 1f);
        }
    }// Listener of Recoil Event

    public virtual void OnDefense()
    {
        if (leftWeapon != null && leftWeapon.meleeType != MeleeType.OnlyAttack && leftWeapon.gameObject.activeSelf)
        {
            leftWeapon.OnDefense();
        }
        if (rightWeapon != null && rightWeapon.meleeType != MeleeType.OnlyAttack && rightWeapon.gameObject.activeSelf)
        {
            rightWeapon.OnDefense();
        }
    }// Call Weapon Defense Events.

    #endregion

    #region GETS

    public virtual int GetAttackID()
    {
        if (rightWeapon != null && rightWeapon.meleeType != MeleeType.OnlyDefense && rightWeapon.gameObject.activeSelf) return rightWeapon.attackID;
        return 0;
    }// Get Current Attack ID

    public virtual float GetAttackStaminaCost()
    {
        if (rightWeapon != null && rightWeapon.meleeType != MeleeType.OnlyDefense && rightWeapon.gameObject.activeSelf) return rightWeapon.staminaCost;
        return defaultStaminaCost;
    }// Get StaminaCost

    public virtual float GetAttackStaminaRecoveryDelay()
    {
        if (rightWeapon != null && rightWeapon.meleeType != MeleeType.OnlyDefense && rightWeapon.gameObject.activeSelf) return rightWeapon.staminaRecoveryDelay;
        return defaultStaminaRecoveryDelay;
    }// Get StaminaCost

    /// <summary>
    /// Pega a distancia ideal para Defesa
    /// </summary>
    /// <returns></returns>
    public virtual float GetAttackDistance()
    {
        if (rightWeapon != null && rightWeapon.meleeType != MeleeType.OnlyDefense && rightWeapon.gameObject.activeSelf) return rightWeapon.distanceToAttack;
        return defaultAttackDistance;
    }// Get ideal distance for the attack

    public virtual int GetDefenseID()
    {
        if (leftWeapon != null && leftWeapon.meleeType != MeleeType.OnlyAttack && leftWeapon.gameObject.activeSelf)
        {
            GetComponent<Animator>().SetBool("FlipAnimation", false);
            return leftWeapon.defenseID;
        }
        if (rightWeapon != null && rightWeapon.meleeType != MeleeType.OnlyAttack && rightWeapon.gameObject.activeSelf)
        {
            GetComponent<Animator>().SetBool("FlipAnimation", true);
            return rightWeapon.defenseID;
        }
        return 0;
    }// Get Current Defense ID

    public int GetDefenseRate()
    {
        if (leftWeapon != null && leftWeapon.meleeType != MeleeType.OnlyAttack && leftWeapon.gameObject.activeSelf)
        {
            return leftWeapon.defenseRate;
        }
        if (rightWeapon != null && rightWeapon.meleeType != MeleeType.OnlyAttack && rightWeapon.gameObject.activeSelf)
        {
            return rightWeapon.defenseRate;
        }
        return defaultDefenseRate;
    }// Get Defense Rate of Melee Defense 

    public virtual int GetMoveSetID()
    {
        if (rightWeapon != null && rightWeapon.gameObject.activeSelf) return rightWeapon.movesetID;
        // if (leftWeapon != null && leftWeapon.gameObject.activeSelf) return leftWeapon.MovesetID;
        return 0;
    }// Get Current MoveSet ID

    public virtual bool CanBreakAttack()
    {
        if (leftWeapon != null && leftWeapon.meleeType != MeleeType.OnlyAttack && leftWeapon.gameObject.activeSelf)
        {
            return leftWeapon.breakAttack;
        }
        if (rightWeapon != null && rightWeapon.meleeType != MeleeType.OnlyAttack && rightWeapon.gameObject.activeSelf)
        {
            return rightWeapon.breakAttack;
        }
        return false;
    }// Check if defence can break Attack

    public virtual int GetDefenseRecoilID()
    {
        if (leftWeapon != null && leftWeapon.meleeType != MeleeType.OnlyAttack && leftWeapon.gameObject.activeSelf)
        {
            return leftWeapon.recoilID;
        }
        if (rightWeapon != null && rightWeapon.meleeType != MeleeType.OnlyAttack && rightWeapon.gameObject.activeSelf)
        {
            return rightWeapon.recoilID;
        }
        return 0;
    }// Get defense recoilID for break attack

    #endregion

    public virtual bool CanBlockAttack(Vector3 attackPoint)
    {
        if (leftWeapon != null && leftWeapon.meleeType != MeleeType.OnlyAttack && leftWeapon.gameObject.activeSelf)
        {
            return Math.Abs(transform.HitAngle(attackPoint)) <= leftWeapon.defenseRange;
        }
        if (rightWeapon != null && rightWeapon.meleeType != MeleeType.OnlyAttack && rightWeapon.gameObject.activeSelf)
        {
            return Math.Abs(transform.HitAngle(attackPoint)) <= rightWeapon.defenseRange;
        }
        return Math.Abs(transform.HitAngle(attackPoint)) <= defaultDefenseRange;
    }/// Check if attack can be blocked

    protected virtual bool InRecoilRange(HitInfo hitInfo)
    {
        var center = new Vector3(transform.position.x, hitInfo.hitPoint.y, transform.position.z);
        var euler = (Quaternion.LookRotation(hitInfo.hitPoint - center).eulerAngles - transform.eulerAngles).NormalizeAngle();

        return euler.y <= hitProperties.recoilRange;
    }/// Check if angle of hit point is in range of recoil

    public void SetLeftWeapon(GameObject weaponObject)
    {
        if (weaponObject)
        {
            leftWeapon = weaponObject.GetComponent<MeleeWeapon>();
            if (leftWeapon)
            {
                leftWeapon.SetActiveDamage(false);
                leftWeapon.meleeManager = this;
            }
        }
    }

    public void SetRightWeapon(GameObject weaponObject)
    {
        if (weaponObject)
        {
            rightWeapon = weaponObject.GetComponent<MeleeWeapon>();
            if (rightWeapon)
            {
                rightWeapon.meleeManager = this;
                rightWeapon.SetActiveDamage(false);
            }
        }
    }

    public void SetLeftWeapon(MeleeWeapon weapon)
    {
        if (weapon)
        {
            leftWeapon = weapon;
            leftWeapon.SetActiveDamage(false);
            leftWeapon.meleeManager = this;
        }
    }

    public void SetRightWeapon(MeleeWeapon weapon)
    {
        if (weapon)
        {
            rightWeapon = weapon;
            rightWeapon.meleeManager = this;
            rightWeapon.SetActiveDamage(false);
        }
    }

    void ResetRecoil()
    {
        inRecoil = false;
    }
}


#region Secundary Classes
[Serializable]
public class OnHitEvent : UnityEngine.Events.UnityEvent<HitInfo> { }

[Serializable]
public class BodyMember
{
    public Transform transform;
    public string bodyPart;

    public MeleeAttackObject attackObject;
    public bool isHuman;
    public void SetActiveDamage(bool active)
    {
        attackObject.SetActiveDamage(active);
    }
}

public enum HumanBones
{
    RightHand, RightLowerArm, RightUpperArm, RightShoulder,
    LeftHand, LeftLowerArm, LetfUpperArm, LeftShoulder,
    RightFoot, RightLowerLeg, RightUpperLeg,
    LeftFoot, LeftLowerLeg, LeftUpperLeg,
    Chest,
    Head
}

public enum AttackType
{
    Unarmed, MeleeWeapon
}
#endregion

