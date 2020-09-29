using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

[CanEditMultipleObjects]
[CustomEditor(typeof(AIMotor), true)]
public class AIEditor : EditorBase
{
    Transform waiPointSelected;

    #region Unity Methods

    #endregion
}

public class PopUpLayerInfoEditor : EditorWindow
{

}