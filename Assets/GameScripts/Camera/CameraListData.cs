using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CameraListData : ScriptableObject
{
    [SerializeField] public string Name;
    [SerializeField] public List<CameraState> tpCameraStates;

    public CameraListData()
    {
        tpCameraStates = new List<CameraState>();
        tpCameraStates.Add(new CameraState("Default"));
    }
}
