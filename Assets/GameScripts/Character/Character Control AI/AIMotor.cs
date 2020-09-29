using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIMotor : Character
{
    #region Variables

    protected NavMeshAgent agent;

    #region Layers

    [EditorToolbar("Layers")]

    [Tooltip("Layers that the character can walk on")]
    public LayerMask groundLayer = 1 << 0;

    [HideInInspector]
    public CapsuleCollider _capsuleCollider;

    #endregion

    #endregion

    #region Check Methods

    public void CheckAutoCrouch()
    {
        //raidius of spherecast
        float radius = _capsuleCollider.radius * .9f;
    }

    #endregion
}
