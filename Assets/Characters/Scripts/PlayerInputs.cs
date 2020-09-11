using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputs : MonoBehaviour
{
    #region Variables
    [Header("Default Input")]

    public GenericInput horizontalInput = new GenericInput("Horizontal", "LeftAnalogHorizontal", "Horizontal");
    public GenericInput verticallInput = new GenericInput("Vertical", "LeftAnalogVertical", "Vertical");
    public GenericInput sprintInput = new GenericInput("LeftShift", "LeftStickClick", "LeftStickClick");
    public GenericInput strafeInput = new GenericInput("Tab", "RightStickClick", "RightStickClick");
    public GenericInput crouchInput = new GenericInput("C", "Y", "Y");
    public GenericInput jumpInput = new GenericInput("Space", "X", "X");
    public GenericInput rollInput = new GenericInput("Q", "B", "B");

    [HideInInspector]
    public PlayerController cc;
    [HideInInspector]
    public HUDController hud;

    #endregion
    protected virtual void Start()
    {
        cc = GetComponent<PlayerController>();

        if (cc != null)
            cc.Init();

        if (PlayerController.instance == cc || PlayerController.instance == null)
            StartCoroutine(CharacterInit());

        ShowCursor(false);
        LockCursor(false);
    }

    protected virtual IEnumerator CharacterInit()
    {
        yield return new WaitForEndOfFrame();
        if (hud == null && HUDController.instance != null)
        {
            hud = HUDController.instance;
            hud.Init(cc);
        }
    }

    protected virtual void LateUpdate()
    {
    }

    protected virtual void FixedUpdate()
    {
        cc.ControlLocomotion();
    }

    protected virtual void Update()
    {
        if (cc == null || Time.timeScale == 0) return;

        InputHandle();      //update input Methods
        cc.UpdateMotor();   //call update methods
        UpdateHUD();          //Update HUD Graphics
    }

    #region Basic Imputs

    protected virtual void InputHandle()
    {
        MoveCharacter();
        SprintInput();
        CrounchInput();
        JumpInput();
        StrafeInput();
        RollInput();
    }//use this to lock the inputs

    protected virtual void MoveCharacter()
    {
        //get inputs from mobile
        cc.input.x = horizontalInput.GetAxis();
        cc.input.y = verticallInput.GetAxis();
    }//set values from to playercontroller

    protected virtual void SprintInput()
    {
        if (sprintInput.GetButton())
            cc.Sprint(true);
        else
            cc.Sprint(false);
    }//set shift buttom to sprint method

    protected virtual void StrafeInput()
    {
        if (strafeInput.GetButtonDown())
            cc.Strafe();
    }

    protected virtual void CrounchInput()
    {
        if (crouchInput.GetButtonDown())
            cc.Crounch();
    }//set c buttom to crounch method

    protected virtual void JumpInput()
    {
        if (jumpInput.GetButtonDown())
            cc.Jump(true);
    }// set space button to jump method

    protected virtual void RollInput()
    {
        if (rollInput.GetButtonDown())
            cc.Roll();
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

    #region HUD

    public virtual void UpdateHUD()
    {
        if (hud == null)
            return;

        hud.UpdateHUD(cc);
    }

    #endregion
}
