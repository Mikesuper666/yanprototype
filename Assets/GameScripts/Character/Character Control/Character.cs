using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using YanProject;

[System.Serializable]
public class OnActiveRagdoll : UnityEvent { }
[System.Serializable]
public class OnActionHandle : UnityEvent<Collider> { }
[System.Serializable]
public abstract class Character : HealthController
{
    #region Character Variables 

    public enum DeathBy
    {
        Animation,
        AnimationWithRagdoll,
        Ragdoll
    }

    public DeathBy deathBy = DeathBy.Animation;
    public bool removeComponentsAfterDie;
    // get the animator component of character
    [HideInInspector]
    public Animator anime { get; private set; }
    // know if the character is ragdolled or not
    // [HideInInspector]
    public bool ragdolled { get; set; }

    [Header("--- Character Events ---")]
    public OnActiveRagdoll onActiveRagdoll = new OnActiveRagdoll();
    [Header("Check if Character is in Trigger with tag Action")]
    [HideInInspector]
    public OnActionHandle onActionEnter = new OnActionHandle();
    [HideInInspector]
    public OnActionHandle onActionStay = new OnActionHandle();
    [HideInInspector]
    public OnActionHandle onActionExit = new OnActionHandle();

    protected AnimatorParameter hitDirectionHash;
    protected AnimatorParameter reactionIDHash;
    protected AnimatorParameter triggerReactionHash;
    protected AnimatorParameter triggerResetStateHash;
    protected AnimatorParameter recoilIDHash;
    protected AnimatorParameter triggerRecoilHash;
    protected bool isInit;

    #endregion

    public virtual void Init()
    {
        anime = GetComponent<Animator>();

        if (anime)
        {
            hitDirectionHash = new AnimatorParameter(anime, "HitDirection");
            reactionIDHash = new AnimatorParameter(anime, "ReactionID");
            triggerReactionHash = new AnimatorParameter(anime, "TriggerReaction");
            triggerResetStateHash = new AnimatorParameter(anime, "ResetState");
            recoilIDHash = new AnimatorParameter(anime, "RecoilID");
            triggerRecoilHash = new AnimatorParameter(anime, "TriggerRecoil");
        }

        var actionListeners = GetComponents<ActionListener>();
        for (int i = 0; i < actionListeners.Length; i++)
        {
            if (actionListeners[i].actionEnter)
                onActionEnter.AddListener(actionListeners[i].OnActionEnter);
            if (actionListeners[i].actionStay)
                onActionStay.AddListener(actionListeners[i].OnActionStay);
            if (actionListeners[i].actionExit)
                onActionExit.AddListener(actionListeners[i].OnActionExit);
        }
    }

    public virtual void ResetRagdoll()
    {

    }

    public virtual void EnableRagdoll()
    {

    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        onActionEnter.Invoke(other);
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        onActionStay.Invoke(other);
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        onActionExit.Invoke(other);
    }



}
