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
    void OnRecoil(int recoilID);

    Character character { get; }
}

public interface IAttackReceiver
{
    void OnReceiveAttack(Damage damage, IMeleeFighter attacker);
}

public static class IMeeleFighterHelper
{
    public static IMeleeFighter GetMeleeFighter(this GameObject receiver)
    {
        return receiver.GetComponent<IMeleeFighter>();
    }

    public static void ApplyDamage(this GameObject receiver, Damage damage, IMeleeFighter attacker)
    {
        var attackReceiver = receiver.GetComponent<IAttackReceiver>();
        if (attackReceiver != null) attackReceiver.OnReceiveAttack(damage, attacker);
        else receiver.ApplyDamage(damage);
    }
}