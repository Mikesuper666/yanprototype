﻿using UnityEngine;
using System.Collections;
using System;

public class HitBox : MonoBehaviour
{
    [HideInInspector]
    public MeleeAttackObject attackObject;
    // [HideInInspector]
    public Collider trigger;
    public int damagePercentage = 100;
    [EnumFlag]
    public HitBoxType triggerType = HitBoxType.Damage | HitBoxType.Recoil;
    private bool canHit;

    /// <summary>
    /// Desenha a caixa de collider no character conforme o HitBoxType
    /// </summary>
    void OnDrawGizmos()
    {
        trigger = gameObject.GetComponent<Collider>();

        if (!trigger) trigger = gameObject.AddComponent<BoxCollider>();
        Color color = (triggerType & HitBoxType.Damage) != 0 && (triggerType & HitBoxType.Recoil) == 0 ? Color.green :
                       (triggerType & HitBoxType.Damage) != 0 && (triggerType & HitBoxType.Recoil) != 0 ? Color.yellow :
                       (triggerType & HitBoxType.Recoil) != 0 && (triggerType & HitBoxType.Damage) == 0 ? Color.red : Color.black;
        color.a = 0.6f;
        Gizmos.color = color;
        if (!Application.isPlaying && trigger && !trigger.enabled) trigger.enabled = true;
        if (trigger && trigger.enabled)
        {
            if (trigger as BoxCollider)
            {
                BoxCollider box = trigger as BoxCollider;

                var sizeX = transform.lossyScale.x * box.size.x;
                var sizeY = transform.lossyScale.y * box.size.y;
                var sizeZ = transform.lossyScale.z * box.size.z;
                Matrix4x4 rotationMatrix = Matrix4x4.TRS(box.bounds.center, transform.rotation, new Vector3(sizeX, sizeY, sizeZ));
                Gizmos.matrix = rotationMatrix;
                Gizmos.DrawCube(Vector3.zero, Vector3.one);
            }
        }
    }

    void Start()
    {
        trigger = GetComponent<Collider>();
        if (!trigger) trigger = gameObject.AddComponent<BoxCollider>();
        if (trigger)
        {
            trigger.isTrigger = true;
            trigger.enabled = false;
        }

        canHit = ((triggerType & HitBoxType.Damage) != 0 || (triggerType & HitBoxType.Recoil) != 0);
    }

    void OnTriggerEnter(Collider other)
    {
        if (TriggerCondictions(other))
        {
            if (attackObject != null)
            {
                attackObject.OnHit(this, other);
            }
        }
    }

    bool TriggerCondictions(Collider other)
    {
        return (canHit && (attackObject != null && (attackObject.meleeManager == null || other.gameObject != attackObject.meleeManager.gameObject)));
    }
}

[Flags]
public enum HitBoxType
{
    Damage = 1, Recoil = 2
}

