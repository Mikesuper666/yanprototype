using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputs : MonoBehaviour
{
    #region Variables
    [Header("Default Input")]

    public GenericInput horizontalInput = new GenericInput("Horizontal", "LeftAnalogHorizontal", "Horizontal");
    public GenericInput verticallInput = new GenericInput("Vertical", "LeftAnalogVertical", "Vertical");

    [HideInInspector]
    public PlayerController cc;

    #endregion
    protected virtual void Start()
    {
        cc = GetComponent<PlayerController>();

        if (cc != null)
            cc.Init();

        ShowCursor(false);
        LockCursor(false);
    }

    protected virtual void LateUpdate()
    {
        cc.ControlLocomotion();
    }

    protected virtual void Update()
    {
        if (cc == null || Time.timeScale == 0) return;

        InputHandle();      //update input Methods
        cc.UpdateMotor();   //call update methods
    }

    #region Basic Imputs

    protected virtual void InputHandle()
    {
        MoveCharacter();
        SprintInput();
    }//use this to lock the inputs

    protected virtual void MoveCharacter()
    {
        //get inputs from mobile
        cc.input.x = horizontalInput.GetAxis();
        cc.input.y = verticallInput.GetAxis();
    }//set values from to playercontroller

    protected virtual void SprintInput()
    {

    }

    #endregion

    #region Generic Methods

    public virtual void ShowCursor(bool value)
    {
        Cursor.visible = value;
    }

    public virtual void LockCursor(bool value)
    {
        if (value)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;
    }

    #endregion
}
