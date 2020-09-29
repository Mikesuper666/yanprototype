using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Handler
{
    public Transform defaultHandler;
    public List<Transform> customHandlers;
    public Handler()
    {
        customHandlers = new List<Transform>();
    }
}
