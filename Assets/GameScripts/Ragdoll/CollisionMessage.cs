using System.Collections;
using System;
using UnityEngine;

public partial class CollisionMessage : MonoBehaviour, IDamageReceiver
{
    [HideInInspector]
    public Ragdoll ragdoll;
    public bool overrideReactionID;
    [mHideInInspector("overrideReactionID")]
    public int reactionID;

    void Start()
    {
        ragdoll = GetComponentInParent<Ragdoll>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision != null)
        {
            if (ragdoll && ragdoll.isActive)
            {
                ragdoll.OnRagdollCollisionEnter(new RagdollCollision(this.gameObject, collision));
                if (!inAddDamage)
                {
                    float impactForce = collision.relativeVelocity.x + collision.relativeVelocity.y + collision.relativeVelocity.z;
                    if (impactForce > 10 || impactForce < -10)
                    {
                        inAddDamage = true;
                        Damage damage = new Damage((int)Mathf.Abs(impactForce) - 10);
                        damage.ignoreDefense = true;
                        damage.sender = collision.transform;
                        damage.hitPosition = collision.contacts[0].point;

                        Invoke("ResetAddDamage", .1f);
                    }
                }
            }
        }

    }

    bool inAddDamage;

    void ResetAddDamage()
    {
        inAddDamage = false;
    }

    public void TakeDamage(Damage damage)
    {
        if (!ragdoll) return;
        if (!ragdoll.iChar.isDead)
        {
            inAddDamage = true;
            if (overrideReactionID) damage.reaction_id = reactionID;

            ragdoll.ApplyDamage(damage);
            Invoke("ResetAddDamage", 0.1f);
        }
    }
}
