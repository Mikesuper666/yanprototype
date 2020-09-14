﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using YanProject;

[CustomEditor(typeof(MainCamera))]
[CanEditMultipleObjects]
public class CameraEditor : Editor
{
    MainCamera tpCamera;
    bool hasPointCopy;
    Vector3 pointCopy;
    int indexSelected;

    void OnSceneGUI()
    {
        if (Application.isPlaying)
            return;
        tpCamera = (MainCamera)target;

        if (tpCamera.gameObject == Selection.activeGameObject)
            if (tpCamera.CameraStateList != null && tpCamera.CameraStateList.tpCameraStates != null && tpCamera.CameraStateList.tpCameraStates.Count > 0)
            {
                if (tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].cameraMode != TPCameraMode.FixedPoint) return;
                try
                {
                    for (int i = 0; i < tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].lookPoints.Count; i++)
                    {
                        if (indexSelected == i)
                        {
                            Handles.color = Color.blue;
                            tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].lookPoints[i].positionPoint = tpCamera.transform.position;
                            tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].lookPoints[i].eulerAngle = tpCamera.transform.eulerAngles;
                            if (tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].lookPoints[indexSelected].freeRotation)
                            {
                                Handles.SphereHandleCap(0, tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].lookPoints[i].eulerAngle, Quaternion.identity, 0.5f, EventType.Ignore);
                            }
                            else
                            {
                                Handles.DrawLine(tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].lookPoints[i].positionPoint,
                                tpCamera.target.position);
                            }
                        }
                        else if (Handles.Button(tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].lookPoints[i].positionPoint, Quaternion.identity, 0.5f, 0.3f, Handles.SphereHandleCap))
                        {
                            indexSelected = i;
                            tpCamera.indexLookPoint = i;
                            tpCamera.transform.position = tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].lookPoints[i].positionPoint;
                            tpCamera.transform.rotation = Quaternion.Euler(tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].lookPoints[i].eulerAngle);
                        }
                        Handles.color = Color.white;
                        Handles.Label(tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].lookPoints[i].positionPoint, tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].lookPoints[i].pointName);
                    }
                }
                catch { if (tpCamera.indexList > tpCamera.CameraStateList.tpCameraStates.Count - 1) tpCamera.indexList = tpCamera.CameraStateList.tpCameraStates.Count - 1; }
            }
    }

    void OnEnable()
    {
        indexSelected = 0;
        tpCamera = (MainCamera)target;
        tpCamera.indexLookPoint = 0;
        if (tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].cameraMode != TPCameraMode.FixedPoint) return;
        if (tpCamera.CameraStateList != null && (tpCamera.indexList < tpCamera.CameraStateList.tpCameraStates.Count) && tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].lookPoints.Count > 0)
        {
            tpCamera.transform.position = tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].lookPoints[0].positionPoint;
            tpCamera.transform.rotation = Quaternion.Euler(tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList].lookPoints[0].eulerAngle);
        }
    }

    public override void OnInspectorGUI()
    {
        tpCamera = (MainCamera)target;

        EditorGUILayout.Space();
        GUILayout.BeginVertical("CAMERA CONFIFURATIONS", "window");
        GUILayout.Space(5);

        if (tpCamera.cullingLayer == 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Please assign the Culling Layer to 'Default' ", MessageType.Warning);
            EditorGUILayout.Space();
        }

        EditorGUILayout.HelpBox("The target will be assign automatically to the current Character when start, check the InitialSetup method on the Motor.", MessageType.Info);

        base.OnInspectorGUI();
        GUILayout.EndVertical();

        GUILayout.BeginVertical("CAMERA STATES", "window");

        GUILayout.Space(5);

        EditorGUILayout.HelpBox("This settings will always load in this List, you can create more List's with different settings for another characters or scenes", MessageType.Info);

        tpCamera.CameraStateList = (CameraListData)EditorGUILayout.ObjectField("CameraState List", tpCamera.CameraStateList, typeof(CameraListData), false);
        if (tpCamera.CameraStateList == null)
        {
            GUILayout.EndVertical();
            return;
        }
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(new GUIContent("NEW CAMERA STATE")))
        {
            if (tpCamera.CameraStateList.tpCameraStates == null)
                tpCamera.CameraStateList.tpCameraStates = new List<CameraState>();

            tpCamera.CameraStateList.tpCameraStates.Add(new CameraState("New State" + tpCamera.CameraStateList.tpCameraStates.Count));
            tpCamera.indexList = tpCamera.CameraStateList.tpCameraStates.Count - 1;
        }

        if (GUILayout.Button(new GUIContent("DELETE STATE")) && tpCamera.CameraStateList.tpCameraStates.Count > 1 && tpCamera.indexList != 0)
        {
            tpCamera.CameraStateList.tpCameraStates.RemoveAt(tpCamera.indexList);
            if (tpCamera.indexList - 1 >= 0)
                tpCamera.indexList--;
        }

        GUILayout.EndHorizontal();

        if (tpCamera.CameraStateList.tpCameraStates.Count > 0)
        {
            if (tpCamera.indexList > tpCamera.CameraStateList.tpCameraStates.Count - 1) tpCamera.indexList = 0;
            tpCamera.indexList = EditorGUILayout.Popup("State", tpCamera.indexList, getListName(tpCamera.CameraStateList.tpCameraStates));
            StateData(tpCamera.CameraStateList.tpCameraStates[tpCamera.indexList]);
        }

        GUILayout.EndVertical();
        EditorGUILayout.Space();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(tpCamera);
            EditorUtility.SetDirty(tpCamera.CameraStateList);
        }
    }

    void StateData(CameraState camState)
    {
        EditorGUILayout.Space();
        camState.cameraMode = (TPCameraMode)EditorGUILayout.EnumPopup("Camera Mode", camState.cameraMode);
        camState.Name = EditorGUILayout.TextField("State Name", camState.Name);
        if (CheckName(camState.Name, tpCamera.indexList))
        {
            EditorGUILayout.HelpBox("This name already exist, choose another one", MessageType.Error);
        }

        switch (camState.cameraMode)
        {
            case TPCameraMode.FreeDirectional:
                FreeDirectionalMode(camState);
                break;
            case TPCameraMode.FixedAngle:
                FixedAngleMode(camState);
                break;
            case TPCameraMode.FixedPoint:
                FixedPointMode(camState);
                break;
        }
    }

    void DrawLookPoint(CameraState camState)
    {
        if (camState.lookPoints == null) camState.lookPoints = new List<LookPoint>();
        if (camState.lookPoints.Count > 0)
        {
            EditorGUILayout.HelpBox("You can create multiple camera points and change them using the TriggerChangeCameraState script.", MessageType.Info);

            if (tpCamera.indexLookPoint > camState.lookPoints.Count - 1)
                tpCamera.indexLookPoint = 0;
            if (tpCamera.indexLookPoint < 0)
                tpCamera.indexLookPoint = camState.lookPoints.Count - 1;
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("Fixed Points");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("<", GUILayout.Width(20)))
            {
                if (tpCamera.indexLookPoint - 1 < 0)
                    tpCamera.indexLookPoint = camState.lookPoints.Count - 1;
                else
                    tpCamera.indexLookPoint--;
                tpCamera.transform.position = camState.lookPoints[tpCamera.indexLookPoint].positionPoint;
                tpCamera.transform.rotation = Quaternion.Euler(camState.lookPoints[tpCamera.indexLookPoint].eulerAngle);

                indexSelected = tpCamera.indexLookPoint;
            }
            GUILayout.Box((tpCamera.indexLookPoint + 1).ToString("00") + "/" + camState.lookPoints.Count.ToString("00"));
            if (GUILayout.Button(">", GUILayout.Width(20)))
            {
                if (tpCamera.indexLookPoint + 1 > camState.lookPoints.Count - 1)
                    tpCamera.indexLookPoint = 0;
                else
                    tpCamera.indexLookPoint++;
                tpCamera.transform.position = camState.lookPoints[tpCamera.indexLookPoint].positionPoint;
                tpCamera.transform.rotation = Quaternion.Euler(camState.lookPoints[tpCamera.indexLookPoint].eulerAngle);
                indexSelected = tpCamera.indexLookPoint;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal("box");
            GUILayout.Label("Point Name");
            camState.lookPoints[tpCamera.indexLookPoint].pointName = GUILayout.TextField(camState.lookPoints[tpCamera.indexLookPoint].pointName, 100);
            GUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("Check 'Static Camera' to create a static point and leave uncheck to look at the Player.", MessageType.Info);
            camState.lookPoints[tpCamera.indexLookPoint].freeRotation = EditorGUILayout.Toggle("Static Camera", camState.lookPoints[tpCamera.indexLookPoint].freeRotation);

            EditorGUILayout.Space();
        }

        GUILayout.BeginHorizontal("box");
        if (GUILayout.Button("New Point"))
        {
            LookPoint p = new LookPoint();
            p.pointName = "point_" + (camState.lookPoints.Count + 1).ToString("00");
            p.positionPoint = tpCamera.transform.position;
            p.eulerAngle = (tpCamera.target) ? tpCamera.target.position : (tpCamera.transform.position + tpCamera.transform.forward);
            camState.lookPoints.Add(p);
            tpCamera.indexLookPoint = camState.lookPoints.Count - 1;

            tpCamera.transform.position = camState.lookPoints[tpCamera.indexLookPoint].positionPoint;
            indexSelected = tpCamera.indexLookPoint;
        }

        if (GUILayout.Button("Remove current point "))
        {
            if (camState.lookPoints.Count > 0)
            {
                camState.lookPoints.RemoveAt(tpCamera.indexLookPoint);
                tpCamera.indexLookPoint--;
                if (tpCamera.indexLookPoint > camState.lookPoints.Count - 1)
                    tpCamera.indexLookPoint = 0;
                if (tpCamera.indexLookPoint < 0)
                    tpCamera.indexLookPoint = camState.lookPoints.Count - 1;
                if (camState.lookPoints.Count > 0)
                    tpCamera.transform.position = camState.lookPoints[tpCamera.indexLookPoint].positionPoint;
                indexSelected = tpCamera.indexLookPoint;
            }
        }

        GUILayout.EndHorizontal();
        EditorGUILayout.Space();
    }

    void FreeDirectionalMode(CameraState camState)
    {
        camState.forward = (float)((int)EditorGUILayout.Slider("Forward", camState.forward, -1f, 1f));
        camState.right = EditorGUILayout.Slider("Right", camState.right, -3f, 3f);
        camState.defaultDistance = EditorGUILayout.FloatField("Distance", camState.defaultDistance);
        camState.useZoom = EditorGUILayout.Toggle("Use Zoom", camState.useZoom);
        if (camState.useZoom)
        {
            camState.maxDistance = EditorGUILayout.FloatField("Max Distance", camState.maxDistance);
            camState.minDistance = EditorGUILayout.FloatField("Min Distance", camState.minDistance);
        }
        camState.height = EditorGUILayout.FloatField("Height", camState.height);
        camState.fov = EditorGUILayout.Slider("Field of View", camState.fov, 1, 179);
        camState.smoothFollow = EditorGUILayout.FloatField("Smooth Follow", camState.smoothFollow);
        camState.cullingHeight = EditorGUILayout.FloatField("Culling Height", camState.cullingHeight);
        camState.rotationOffSet = EditorGUILayout.Vector3Field("Rotation OffSet", camState.rotationOffSet);
        camState.xMouseSensitivity = EditorGUILayout.FloatField("MouseSensitivity X", camState.xMouseSensitivity);
        camState.yMouseSensitivity = EditorGUILayout.FloatField("MouseSensitivity Y", camState.yMouseSensitivity);
        MinMaxSlider("Limit Angle X", ref camState.xMinLimit, ref camState.xMaxLimit, -360, 360);
        MinMaxSlider("Limit Angle Y", ref camState.yMinLimit, ref camState.yMaxLimit, -180, 180);
    }

    void FixedAngleMode(CameraState camState)
    {
        camState.defaultDistance = EditorGUILayout.FloatField("Distance", camState.defaultDistance);
        camState.useZoom = EditorGUILayout.Toggle("Use Zoom", camState.useZoom);
        if (camState.useZoom)
        {
            camState.maxDistance = EditorGUILayout.FloatField("Max Distance", camState.maxDistance);
            camState.minDistance = EditorGUILayout.FloatField("Min Distance", camState.minDistance);
        }
        camState.height = EditorGUILayout.FloatField("Height", camState.height);
        camState.fov = EditorGUILayout.Slider("Field of View", camState.fov, 1, 179);
        camState.smoothFollow = EditorGUILayout.FloatField("Smooth Follow", camState.smoothFollow);
        camState.cullingHeight = EditorGUILayout.FloatField("Culling Height", camState.cullingHeight);
        camState.right = EditorGUILayout.Slider("Right", camState.right, -3f, 3f);
        camState.fixedAngle.x = EditorGUILayout.Slider("Angle X", camState.fixedAngle.x, -360, 360);
        camState.fixedAngle.y = EditorGUILayout.Slider("Angle Y", camState.fixedAngle.y, -360, 360);
        //if (camState.cameraMode.Equals(TPCameraMode.FixedAngle))
        //    tpCamera.RotateCamera(camState.fixedAngle.x, camState.fixedAngle.y);
        Debug.Log("Debugar aqui depois");
    }

    void FixedPointMode(CameraState camState)
    {
        camState.smoothFollow = EditorGUILayout.FloatField("Smooth Follow", camState.smoothFollow);
        camState.fov = EditorGUILayout.Slider("Field of View", camState.fov, 1, 179);
        camState.fixedAngle.x = 0;
        camState.fixedAngle.y = 0;

        DrawLookPoint(camState);
    }

    void MinMaxSlider(string name, ref float minVal, ref float maxVal, float minLimit, float maxLimit)
    {
        GUILayout.BeginVertical();
        GUILayout.Label(name);
        GUILayout.BeginHorizontal("box");
        minVal = EditorGUILayout.FloatField(minVal, GUILayout.MaxWidth(60));
        EditorGUILayout.MinMaxSlider(ref minVal, ref maxVal, minLimit, maxLimit);
        maxVal = EditorGUILayout.FloatField(maxVal, GUILayout.MaxWidth(60));
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    bool CheckName(string Name, int _index)
    {
        foreach (CameraState state in tpCamera.CameraStateList.tpCameraStates)
            if (state.Name.Equals(Name) && tpCamera.CameraStateList.tpCameraStates.IndexOf(state) != _index)
                return true;

        return false;
    }

    [MenuItem("Invector/Resources/New CameraState List Data")]
    static void NewCameraStateData()
    {
        vScriptableObjectUtility.CreateAsset<CameraListData>();
    }

    private string[] getListName(List<CameraState> list)
    {
        string[] names = new string[list.Count];
        for (int i = 0; i < list.Count; i++)
        {
            names[i] = list[i].Name;
        }
        return names;
    }
}
