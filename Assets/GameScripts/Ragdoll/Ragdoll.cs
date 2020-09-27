using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[ClassHeader("Ragdoll System", true, "ragdollIcon", true, "Every gameobject children of the character must have their tag added in the IgnoreTag List.")]
public class Ragdoll : mMonoBehaviour
{
    #region public variables

    [Button("Active Ragdoll", "ActivateRagdoll", typeof(Ragdoll))]
    public bool removePhysicsAfterDie;
    [Tooltip("SHOOTER: Keep false to use detection hit on each children collider, don't forget to change the layer to BodyPart from hips to all childrens. MELEE: Keep true to only hit the main Capsule Collider.")]
    public bool disableColliders = false;
    public AudioSource collisionSource;
    public AudioClip collisionClip;
    [Header("Add Tags for Weapons or Itens here:")]
    public List<string> ignoreTags = new List<string>() { "Weapon", "Ignore Ragdoll", "Untagged" };
    public AnimatorStateInfo stateInfo;

    #endregion

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
