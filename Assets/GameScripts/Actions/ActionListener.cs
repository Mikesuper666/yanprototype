using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActionListener : mMonoBehaviour
{
    public bool actionEnter;
    public bool actionStay;
    public bool actionExit;
    public OnActionHandle OnDoAction = new OnActionHandle();

    public virtual void OnActionEnter(Collider other)
    {

    }

    public virtual void OnActionStay(Collider other)
    {

    }

    public virtual void OnActionExit(Collider other)
    {

    }

    [System.Serializable]
    public class OnActionHandle : UnityEngine.Events.UnityEvent<TriggerGenericAction>
    {

    }
}

