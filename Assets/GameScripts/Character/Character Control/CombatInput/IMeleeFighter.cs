using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttackListerner 
{
    void OnEnableAttack();

    void OnDisableAttack();

    void ResetAttackTriggers();
}

public interface IMeleeFighter : IAttackReceiver, IAttackListerner
{
    void BreakAttack(int breakAtkID);

    void OnRecoil(int recoilID);

    bool isBlocking { get; }

    bool isAttacking { get; }

    bool isArmed { get; }

    Character character { get; }

    Transform transform { get; }

    GameObject gameObject { get; }
}

public interface IAttackReceiver
{
    void OnReceiveAttack(Damage damage, IMeleeFighter attacker);
}

public static class IMeeleFighterHelper
{
    /// <summary>
    /// check if gameObject has a <see cref="vIMeleeFighter"/> Component
    /// </summary>
    /// <param name="receiver"></param>
    /// <returns>return true if gameObject contains a <see cref="vIMeleeFighter"/></returns>
    public static bool IsAMeleeFighter(this GameObject receiver)
    {
        return receiver.GetComponent<IMeleeFighter>() != null;
    }

    /// <summary>
    /// Aplica o dano usando OnReeciver method
    /// </summary>
    /// <param name="receiver">target damage receiver</param>
    /// <param name="damage">damage</param>
    /// <param name="attacker">damage sender</param>
    public static void ApplyDamage(this GameObject receiver, Damage damage, IMeleeFighter attacker)
    {
        var attackReceiver = receiver.GetComponent<IAttackReceiver>();
        if (attackReceiver != null) attackReceiver.OnReceiveAttack(damage, attacker);
        else receiver.ApplyDamage(damage);
    }// Apply damage using OnReeiveAttack method if receiver dosent has a IAttackReceiver, the Simple ApplyDamage is called

    /// <summary>
    /// Get <see cref="IMeleeFighter"/> of gameObject
    /// </summary>
    /// <param name="receiver"></param>
    /// <returns>the <see cref="vIMeleeFighter"/> component</returns>
    public static IMeleeFighter GetMeleeFighter(this GameObject receiver)
    {
        return receiver.GetComponent<IMeleeFighter>();
    }
}