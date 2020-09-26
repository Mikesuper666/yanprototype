using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputs : mMonoBehaviour
{
    #region Variables
    [Header("Default Input")]
    public bool lockInput;                            //use to block all character inputs
    [Header("Uncheck if you need to use the cursor")]
    public bool unlockCursorOnStart = false;
    public bool showCursorOnStart = false;
    [Header("Configure the inputs if you need")]
    public GenericInput horizontalInput = new GenericInput("Horizontal", "LeftAnalogHorizontal", "Horizontal");
    public GenericInput verticallInput = new GenericInput("Vertical", "LeftAnalogVertical", "Vertical");
    public GenericInput sprintInput = new GenericInput("LeftShift", "LeftStickClick", "LeftStickClick");
    public GenericInput strafeInput = new GenericInput("Tab", "RightStickClick", "RightStickClick");
    public GenericInput crouchInput = new GenericInput("C", "Y", "Y");
    public GenericInput jumpInput = new GenericInput("Space", "X", "X");
    public GenericInput rollInput = new GenericInput("Q", "B", "B");

    [Header("Camera Settings")]
    public bool lockCameraInput;
    [Header("Player don't turn with camera if checked")]
    public bool ignoreCameraRotation;                   //rotate player with the camera if walking/running etc
    [Header("Check if you need (no turn the camera)")]
    public bool rotateUsingMouse = false;
    public bool rotateToCameraWhileStrafe = true;

    [Header("Camera Input")]
    public GenericInput rotateCameraXInput = new GenericInput("Mouse X", "RightAnalogHorizontal", "Mouse X");
    public GenericInput rotateCameraYInput = new GenericInput("Mouse Y", "RightAnalogVertical", "Mouse Y");
    public GenericInput cameraZoomInput = new GenericInput("Mouse ScrollWheel", "", "");
    public GenericInput mouseRight = new GenericInput("Fire2", "", "");
    [HideInInspector]
    public MainCamera tpCamera;                         // acess camera info
    [HideInInspector]
    public string customCameraState;                    // generic string to change the CameraState        
    [HideInInspector]
    public string customlookAtPoint;                    // generic string to change the CameraPoint of the Fixed Point Mode        
    [HideInInspector]
    public bool changeCameraState;                      // generic bool to change the CameraState        
    [HideInInspector]
    public bool smoothCameraState;                      // generic bool to know if the state will change with or without lerp  
    [HideInInspector]
    public bool keepDirection;                          // keep the current direction in case you change the cameraState
    protected Vector2 oldInput;
    public UnityEngine.Events.UnityEvent OnLateUpdate;

    [HideInInspector]
    public PlayerController cc;
    [HideInInspector]
    public HUDController hud;

    #endregion

    #region Initialize Character, Camera & HUD when LoadScene

    protected virtual void Start()
    {
        cc = GetComponent<PlayerController>();

        if (cc != null)
            cc.Init();

        if (PlayerController.instance == cc || PlayerController.instance == null)
            StartCoroutine(CharacterInit());

        ShowCursor(showCursorOnStart);
        LockCursor(unlockCursorOnStart);
    }

    protected virtual IEnumerator CharacterInit()
    {
        yield return new WaitForEndOfFrame();
        if(tpCamera == null)
        {
            tpCamera = FindObjectOfType<MainCamera>();
            if (tpCamera && tpCamera.target != transform)
                tpCamera.SetMainTarget(this.transform);
        }
        if (hud == null && HUDController.instance != null)
        {
            hud = HUDController.instance;
            hud.Init(cc);
        }
    }

    #endregion

    #region Updates Methods

    protected virtual void LateUpdate()
    {
        if (cc == null || Time.timeScale == 0) return;
        //if ((!updateIK && anime.updateMode == AnimatorUpdateMode.AnimatePhysics)) return;
        CameraInput();                      // update camera input
        UpdateCameraStates();               // update camera states
        OnLateUpdate.Invoke();
    }

    protected virtual void FixedUpdate()
    {
        cc.ControlLocomotion();
        cc.AirControl();            //update air behaviour
    }

    protected virtual void Update()
    {
        if (cc == null || Time.timeScale == 0) return;

        InputHandle();      //update input Methods
        cc.UpdateMotor();   //call update methods
        UpdateHUD();          //Update HUD Graphics
    }

    protected virtual void InputHandle()
    {
        if (lockInput || cc.lockMovement || cc.ragdolled) return;

        MoveCharacter();
        SprintInput();
        CrounchInput();
        JumpInput();
        StrafeInput();
        RollInput();
    }//use this to lock the inputs

    #endregion

    #region MoveCharacter Methods

    public virtual void MoveCharacter(Vector3 position, bool rotateToDirection = true)
    {
        var dir = position - transform.position;
        var targetDir = cc.isStrafing ? transform.InverseTransformDirection(dir).normalized : dir.normalized;
        cc.input.x = targetDir.x;
        cc.input.y = targetDir.z;

        if (!keepDirection)
            oldInput = cc.input;

        if (rotateToDirection && cc.isStrafing)
        {
            targetDir.y = 0;
            cc.RotateToDirection(dir);
            Debug.DrawRay(transform.position, dir * 10f, Color.blue);
        }
        else if (rotateToDirection)
        {
            targetDir.y = 0;
            cc.targetDirection = targetDir;
        }
    }

    public virtual void MoveCharacter(Transform _transform, bool rotateToDirection = true)
    {
        MoveCharacter(_transform.position, rotateToDirection);
    }

    #endregion

    #region Basic Imputs

    protected virtual void MoveCharacter()
    {
        //get inputs from mobile
        cc.input.x = horizontalInput.GetAxis();
        cc.input.y = verticallInput.GetAxis();
        //UPDATE old input to compare whith input if keepdirection is true
        if (!keepDirection)
            oldInput = cc.input;
    }//set values from to playercontroller

    protected virtual void SprintInput()
    {
        if (sprintInput.GetButtonDown())
            cc.Sprint(true);
        else
            cc.Sprint(false);
    }//set shift buttom to sprint method

    protected virtual void StrafeInput()
    {
        if (strafeInput.GetButtonDown())
            cc.Strafe();
    }//set TAB to strafe

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

    public void SetLockBasicInput(bool value)
    {
        lockInput = value;
        if (value)
        {
            cc.input = Vector2.zero;
            cc.isSprinting = false;
            cc.anime.SetFloat("InputHorizontal", 0, 0.25f, Time.deltaTime);
            cc.anime.SetFloat("InputVertical", 0, 0.25f, Time.deltaTime);
            cc.anime.SetFloat("InputMagnitude", 0, 0.25f, Time.deltaTime);
        }
    }// Lock all the Input from the Player

    public void ShowCursor(bool value)
    {
        Cursor.visible = value;
    }

    public void LockCursor(bool value)
    {
        if (!value)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;
    }

    public void SetLockCameraInput(bool value)
    {
        lockCameraInput = value;
    }// Lock the Camera Input

    public void IgnoreCameraRotation(bool value)
    {
        ignoreCameraRotation = value;
    }// If you're using the MoveCharacter method with a custom targetDirection, check this true to align the character with your custom targetDirection

    public void SetWalkByDefault(bool value)
    {
        cc.freeSpeed.walkByDefault = value;
        cc.strafeSpeed.walkByDefault = value;
    }// Limits the character to walk only, useful for cutscenes and 'indoor' areas

    #endregion

    #region Camera Methods

    public virtual void CameraInput()
    {
        if (!Camera.main) Debug.Log("Missing a Camera with the tag MainCamera, please add one.");
        if(!ignoreCameraRotation)
        {
            if (!keepDirection) cc.UpdateTargetDirection(Camera.main.transform);
            RotateWithCamera(Camera.main.transform);
        }

        if (tpCamera == null)
            return;

        var Y = lockCameraInput ? 0f : rotateCameraYInput.GetAxis();
        var X = lockCameraInput ? 0f : rotateCameraXInput.GetAxis();
        var zoom = cameraZoomInput.GetAxis();

        if (rotateUsingMouse)//(Input.GetMouseButton(1))
            tpCamera.RotateCamera(X, Y);//block to use mouse *****************
        tpCamera.Zoom(zoom);

        if (keepDirection && Vector2.Distance(cc.input, oldInput) > .2f) keepDirection = false;
    }//Set values from mouse to move camera

    protected virtual void UpdateCameraStates()
    {
        // CAMERA STATE - you can change the CameraState here, the bool means if you want lerp of not, make sure to use the same CameraState String that you named on TPCameraListData

        if (tpCamera == null)
        {
            tpCamera = FindObjectOfType<MainCamera>();
            if (tpCamera == null)
                return;
            if (tpCamera)
            {
                tpCamera.SetMainTarget(this.transform);
                tpCamera.Init();
            }
        }

        if (changeCameraState)
            tpCamera.ChangeState(customCameraState, customlookAtPoint, smoothCameraState);
        else if (cc.isCrouching)
            tpCamera.ChangeState("Crouch", true);
        else if (cc.isStrafing)
            tpCamera.ChangeState("Strafing", true);
        else if (cc.isTalking)
            tpCamera.ChangeState("TalkCamera", true);
        else
            tpCamera.ChangeState("Default", true);
    }//change here the angle camera by camera states

    public void ChangeCameraState(string cameraState)
    {
        changeCameraState = true;
        customCameraState = cameraState;
    }

    public void ResetCameraState()
    {
        changeCameraState = false;
        customCameraState = string.Empty;
    }

    protected virtual void RotateWithCamera(Transform cameraTransform)
    {
        if (rotateToCameraWhileStrafe && cc.isStrafing && !cc.actions && !cc.lockMovement)
        {
            // smooth align character with aim position               
            if (tpCamera != null && tpCamera.lockTarget)
            {
                cc.RotateToTarget(tpCamera.lockTarget);
            }
            // rotate the camera around the character and align with when the char move
            else if (cc.input != Vector2.zero)
            {
                cc.RotateWithAnotherTransform(cameraTransform);
            }
        }
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
