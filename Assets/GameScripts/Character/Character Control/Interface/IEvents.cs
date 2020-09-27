using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageReceiver
{
    Transform transform { get; }
    GameObject gameObject { get; }
    void TakeDamage(Damage damage);
}

public interface IHealthController : IDamageReceiver
{
    float currentHealth { get; }
    bool isDead { get; set; }
    void ChangeHealth(int value);
    void ChangeMaxHealth(int value);
}

public static class DamageHelper
{
    /// <summary>
    /// Apply damage to gameObject if <see cref="CanReceiveDamage(GameObject)"/>
    /// </summary>
    /// <param name="receiver"></param>
    /// <param name="damage"></param>
    public static void ApplyDamage(this GameObject receiver, Damage damage)
    {
        var receivers = receiver.GetComponents<IDamageReceiver>();
        if (receivers != null)
            for (int i = 0; i < receivers.Length; i++)
                receivers[i].TakeDamage(damage);
    }

    /// <summary>
    /// check if gameObject can receive the damage
    /// </summary>
    /// <param name="receiver"></param>
    /// <returns>return true if gameObject contains a <see cref="vIDamageReceiver"/></returns>
    public static bool CanReceiveDamage(this GameObject receiver)
    {
        return receiver.GetComponent<IDamageReceiver>() != null;
    }

    public static bool HasHealth(this GameObject gameObject)
    {
        var healthController = gameObject.GetComponent<IHealthController>();
        return healthController != null && healthController.currentHealth > 0;
    }

    public static float HitAngle(this Transform transform, Vector3 hitpoint, bool normalized = true)
    {
        var localTarget = transform.InverseTransformPoint(hitpoint);
        var _angle = (int)(Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg);

        if (!normalized) return _angle;

        if (_angle <= 45 && _angle >= -45)
            _angle = 0;
        else if (_angle > 45 && _angle < 135)
            _angle = 90;
        else if (_angle >= 135 || _angle <= -135)
            _angle = 180;
        else if (_angle < -45 && _angle > -135)
            _angle = -90;

        return _angle;
    }
}