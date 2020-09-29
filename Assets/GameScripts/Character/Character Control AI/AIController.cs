using PixelCrushers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[ClassHeader("Enemy AI", iconName = "aiIcon")]
public class AIController : AIAnimator//, IMeleeFighter
{
    #region Variables

    [EditorToolbar("Iterations")]
    public float stateRoutineIteration = 0.15f;
    public float destinationRoutineIteration = 0.25f;
    public float findTargetIteration = 0.25f;
    public float smoothSpeed = 5f;

    [EditorToolbar("Events")]
    [Header("--- On Change State Events ---")]
    public UnityEngine.Events.UnityEvent onIdle;
    public UnityEngine.Events.UnityEvent onChase;
    public UnityEngine.Events.UnityEvent onPatrol;
    protected AIStates oldState;

    public enum AIStates
    {
        Idle,
        PatrolSubPoints,
        PatrolWaypoints,
        Chase
    }

    #endregion

    #region Unity Methods

    protected override void Start()
    {
        base.Start();
        Init();
        StartCoroutine(StateRoutine());

    }

    protected void FixedUpdate()
    {

    } 

    #endregion

    #region AI Target

    protected void SetTarget()
    {
    }

    bool CheckTargetIsAlive()
    {
        return false;
    }

    //protected IEnumerator FindTarget()
    //{
    //}

    #endregion

    #region AI Locomotion

    void ControlLocomotion()
    {
       
    }

    //Vector3 AgentDirection()
    //{
       
    //}

    //protected virtual IEnumerator DestinationBehaviour()
    //{
       
    //}

    protected virtual void UpdateDestination(Vector3 position)
    {

    }

    /// <summary>
    /// Chaca se está em um AI Navmesh vádido, e se não morreu
    /// </summary>
    protected void CheckIsOnNavMesh()
    {
        //if active navmesh and not ragdoll
        if(!agent.isOnNavMesh && agent.enabled && !ragdolled)//AI motor variable
        {
            Debug.LogWarning("Navmesh not bake, character will die - Bake your navmesh!");
            currentHealth = 0;
        }
    }// check if the AI is on a valid Navmesh, if not he dies

    #endregion

    #region AI States

    /// <summary>
    /// Estados de comportamentos disponíveis em seu personagem AI
    /// </summary>
    /// <returns></returns>
    protected IEnumerator StateRoutine()
    {
        while (this.enabled)
        {
            CheckIsOnNavMesh();
            CheckAutoCrouch();//check if character is crounching
            yield return new WaitForSeconds(stateRoutineIteration); 
        }
    }

    //protected IEnumerator Idle()
    //{

    //}

    //protected IEnumerator Chase()
    //{

    //}

    //protected IEnumerator PatrolSubPoints()
    //{

    //}

    //protected IEnumerator PatrolWaypoints()
    //{

    //}

    #endregion

    #region AI Waypoints & PatrolPoints

    //Waypoint GetWaypoint()
    //{

    //}

    //Point GetPatrolPoint(vWaypoint waypoint)
    //{

    //}

    #endregion

    #region AI Melee Combat

    //protected IEnumerator MeleeAttackRotine()
    //{

    //    //else if (!actions && attackCount > 0) canAttack = true;
    //}

    public void FinishAttack()
    {

    }

    //IEnumerator ResetAttackCount()
    //{

    //}

    public void OnEnableAttack()
    {

    }

    public void OnDisableAttack()
    {

    }

    public void ResetAttackTriggers()
    {

    }

    public void BreakAttack(int breakAtkID)
    {
        //ResetAttackCount();
        //ResetAttackTriggers();
        //OnRecoil(breakAtkID);
    }

    public void OnRecoil(int recoilID)
    {
        
    }

    public void OnReceiveAttack(Damage damage, IMeleeFighter attacker)
    {

    }

    //public Character character
    //{

    //}

    #endregion
}
