using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDamage 
{

}

[System.Serializable]
public class Damage
{
    [Tooltip("Apply damage to the Character Health")]
    public int damageValue = 15;
    [Tooltip("How much stamina the target will lost when blocking this attack")]
    public float staminaBlockCost = 5;
    [Tooltip("How much time the stamina of the target will wait to recovery")]
    public float staminaRecoveryDelay = 1;
    [Tooltip("Apply damage even if the Character is blocking")]
    public bool ignoreDefense;
    [Tooltip("Activated Ragdoll when hit the Character")]
    public bool activeRagdoll;
    [HideInInspector]
    public Transform sender;
    [HideInInspector]
    public Transform receiver;
    [HideInInspector]
    public Vector3 hitPosition;
    public bool hitReaction = true;
    [HideInInspector]
    public int recoil_id = 0;
    [HideInInspector]
    public int reaction_id = 0;
    public string attackName;

    public Damage()
    {
        this.damageValue = 15;
        this.staminaBlockCost = 5;
        this.staminaRecoveryDelay = 1;
        this.hitReaction = true;
    }
    public Damage(int value)
    {
        this.damageValue = value;
        this.hitReaction = true;
    }

    public Damage(Damage damage)
    {
        this.damageValue = damage.damageValue;
        this.staminaBlockCost = damage.staminaBlockCost;
        this.staminaRecoveryDelay = damage.staminaRecoveryDelay;
        this.ignoreDefense = damage.ignoreDefense;
        this.activeRagdoll = damage.activeRagdoll;
        this.sender = damage.sender;
        this.receiver = damage.receiver;
        this.recoil_id = damage.recoil_id;
        this.reaction_id = damage.reaction_id;
        this.attackName = damage.attackName;
        this.hitPosition = damage.hitPosition;
    }
    /// <summary>
    /// Calc damage Resuction percentage
    /// </summary>
    /// <param name="damageReduction"></param>
    public void ReduceDamage(float damageReduction)
    {
        int result = (int)(this.damageValue - ((this.damageValue * damageReduction) / 100));
        this.damageValue = result;
    }
}

