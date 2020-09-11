using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerMotor : Character
{
    #region Variables

    #region Stamina

    [Header("--Debug Info--")]
    public bool debugWindow;
    public float maxStamina = 200f;
    [HideInInspector]
    public float currentStamina;
    public float rollStamina = 25f;
    public float jumpStamina = 30f;

    #endregion

    #region Layers

    [Tooltip("Layers that character can walk on")]
    public LayerMask groundLayer = 1 << 0;
    [Tooltip("What objects can make the character auto crounch")]
    public LayerMask autoCrouchLayer = 1 << 0;
    [Tooltip("[SPHERECAST] ADJUST IN PLAY MODE - White spherecast to put just above the head, this will make the character Auto-crouch if something hit the shpere")]
    public float headDetect = .95f;
    [Tooltip("Select the layers the your character will stop moving close to")]
    public LayerMask stopMoveLayer;
    [Tooltip("[RAYCAST] Stopmove Raycast Height")]
    public float stopMoveHeight = .65f;
    [Tooltip("[RAYCAST] Stopmove Raycast Distance")]
    public float stopMoveDistance = .5f;

    #endregion

    #region Character Variables

    public vMovementSpeed freeSpeed;

    [Tooltip("Can control the roll direction")]
    public bool rollControl = false;

    [Tooltip("How much time the character will be jumping")]
    public float jumpTimer = .3f;
    [HideInInspector]
    public float jumpCounter;
    [Tooltip("Add Extra jump speed, based on your speed input the character will move forward")]
    public float jumpForward = 3f;
    [Tooltip("Add Extra jump height, if you want to jump only with Root Motion leave the value with 0.")]
    public float jumpHeight = 4f;

    [Tooltip("ADJUST IN PLAY MODE - Offset height limit for sters - GREY Raycast in front of the legs")]
    public float stepOffsetEnd = 0.45f;
    [Tooltip("ADJUST IN PLAY MODE - Offset height origin for sters, make sure to keep slight above the floor - GREY Raycast in front of the legs")]
    public float stepOffsetStart = 0f;
    [Tooltip("Max angle to walk")]
    [Range(30, 75)]
    public float slopeLimit = 75f;

    protected float groundDistance;
    public RaycastHit groundHit;

    #endregion

    #region Actions

    public bool isStrafing
    {
        get
        {
            return _isStrafing || lockInStrafe;
        }
        set
        {
            _isStrafing = value;
        }
    }

    //Movement bools
    [HideInInspector]
    public bool
            isGrounded,
            isCrouching,
            inCrouchArea,
            isSprinting,
            isSliding,
            stopMove,
            autoCrouch;

    // action bools
    [HideInInspector]
    public bool
            isRolling,
            isJumping,
            isGettingUp,
            inTurn,
            quickStop,
            landHigh;

    [HideInInspector]
    public bool customAction;

    // one bool to rule then all
    [HideInInspector]
    public bool actions
    {
        get
        {
            return isRolling || quickStop || landHigh || customAction;
        }
    }

    #endregion

    #region Direction Variable

    [HideInInspector]
    public Vector3 targetDirection;

    #endregion

    #region Components

    [HideInInspector]
    internal Rigidbody _rigidbody;
    [HideInInspector]
    public CapsuleCollider _capsuleCollider;

    #endregion

    #region Hidden Variables

    [HideInInspector]
    public bool lockMovement;
    [HideInInspector]
    public bool lockInStrafe;
    [HideInInspector]
    public float colliderRadius, colliderHeight;    //Storage the collider extra information
    [HideInInspector]
    public float speed, verticalVelocity;           //general variables to the locomotion
    [HideInInspector]
    public Vector2 input;                           //generate variable for the controller
    [HideInInspector]
    public float velocity;                          //velocity to apply to rigidbody
    //Get layers from the Animator controller
    public float jumpMultiplier = 1;
    private bool _isStrafing;

    #endregion

    #endregion

    public void Init()
    {
        // rigidbody info
        _rigidbody = GetComponent<Rigidbody>();

        // capsule collider info
        _capsuleCollider = GetComponent<CapsuleCollider>();

        // health info
        
        
        currentStamina = maxStamina;
        //ResetJumpMultiplier();
        isGrounded = true;
    }

    public virtual void UpdateMotor()
    {
        StopMove();
        ControlJumpBehaviour();
    }

    #region Locomotion

    public virtual void ControlLocomotion()
    {
        //if todo lock if die or cutscenes

        FreeMovement();
    }

    public virtual void FreeMovement()
    {
        //set speed to both vertical and horizontal (return positive value only)
        speed = Mathf.Abs(input.x) + Mathf.Abs(input.y);

        //Limits the character moviment by default
        if (freeSpeed.walkByDefault)
            speed = Mathf.Clamp(speed, 0, .5f);
        else
            speed = Mathf.Clamp(speed, 0, 1f);

        //add .5f sprint
        if (isSprinting) speed += .5f;

        anime.SetFloat("InputMagnitude", speed, .2f, Time.deltaTime); //THIS ANIMATION IS NEED TO KNOW IF REALLY NECESSARY*****************************

        var cooditions = (!actions || quickStop || isRolling && rollControl);

    }

    public virtual void ControlSpeed(float velocity)
    {
        if (Time.deltaTime == 0) return;

        if (!isStrafing)
        {
            StrafeVelocity(velocity);
        }
        else
        {
            FreeVelocity(velocity);
        }
    }

    protected virtual void StrafeVelocity(float velocity)
    {
        Vector3 v = (transform.TransformDirection(new Vector3(input.x, 0, input.y)) * (velocity > 0 ? velocity : 1f));
        v.y = _rigidbody.velocity.y;
        _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, v, 20f * Time.deltaTime);
    }

    protected virtual void FreeVelocity(float velocity)
    {
        var _targetVelocity = transform.forward * velocity * speed;
        _targetVelocity.y = _rigidbody.velocity.y;
        _rigidbody.velocity = _targetVelocity;
    }//move final values forward

    protected void StopMove()
    {
        if (input.sqrMagnitude < .1f) return;

        RaycastHit hitInfo;
        Ray ray = new Ray(transform.position + Vector3.up * stopMoveHeight, targetDirection.normalized);
        var hitAngle = 0f;
        if (Physics.Raycast(ray, out hitInfo, _capsuleCollider.radius + stopMoveDistance, stopMoveLayer))
        {
            stopMove = true;
            return;
        }

        if (!isGrounded || isJumping)
        {
            stopMove = isSliding;
            return;
        }

        if (Physics.Linecast(transform.position + Vector3.up * (_capsuleCollider.height * 0.5f),
            transform.position + targetDirection.normalized * (_capsuleCollider.radius + 0.2f), out hitInfo, groundLayer))
        {
            hitAngle = Vector3.Angle(Vector3.up, hitInfo.normal);
            Debug.DrawLine(transform.position + Vector3.up * (_capsuleCollider.height * 0.5f), transform.position + targetDirection.normalized * (_capsuleCollider.radius + 0.2f), (hitAngle > slopeLimit) ? Color.yellow : Color.blue, 0.01f);
            var targetPoint = hitInfo.point + targetDirection.normalized * _capsuleCollider.radius;
            if ((hitAngle > slopeLimit) && Physics.Linecast(transform.position + Vector3.up * (_capsuleCollider.height * 0.5f),
                targetPoint, out hitInfo, groundLayer))
            {
                Debug.DrawRay(hitInfo.point, hitInfo.normal);
                hitAngle = Vector3.Angle(Vector3.up, hitInfo.normal);

                if (hitAngle > slopeLimit && hitAngle < 85f)
                {
                    Debug.DrawLine(transform.position + Vector3.up * (_capsuleCollider.height * 0.5f), hitInfo.point, Color.red, 0.01f);
                    stopMove = true;
                    return;
                }
                else
                {
                    Debug.DrawLine(transform.position + Vector3.up * (_capsuleCollider.height * 0.5f), hitInfo.point, Color.green, 0.01f);
                }
            }
        }
        else Debug.DrawLine(transform.position + Vector3.up * (_capsuleCollider.height * 0.5f), transform.position + targetDirection.normalized * (_capsuleCollider.radius * 0.2f), Color.blue, 0.01f);

        stopMove = false;
    }

    #endregion

    #region Jump Methods

    protected void ControlJumpBehaviour()
    {
        if (!isJumping) return;
        //counter timer
        jumpCounter -= Time.deltaTime;
        if(jumpCounter <= 0)
        {
            jumpCounter = 0;
            isJumping = false;
        }
        //apply extra force to the jump heigh
        var vel = _rigidbody.velocity;
        vel.y = jumpHeight * jumpMultiplier;
        _rigidbody.velocity = vel;
    }

    #endregion

    #region Ground Check

    public virtual float GroundAngle()
    {
        var groundAngle = Vector3.Angle(groundHit.normal, Vector3.up);
        return groundAngle;
    }

    #endregion

    #region Debug

    public virtual string DebugInfo(string additionalText = "")
    {
        string debugInfo = string.Empty;
        if (debugWindow)
        {
            float delta = Time.smoothDeltaTime;
            float fps = 1 / delta;

            debugInfo =
                "FPS " + fps.ToString("#,##0 fps") + "\n" +
                "inputVertical = " + input.y.ToString("0.0") + "\n" +
                "inputHorizontal = " + input.x.ToString("0.0") + "\n" +
                "verticalVelocity = " + verticalVelocity.ToString("0.00") + "\n" +
                "groundDistance = " + groundDistance.ToString("0.00") + "\n" +
                "groundAngle = " + GroundAngle().ToString("0.00") + "\n" +
                "useGravity = " + _rigidbody.useGravity.ToString() + "\n" +
                "colliderIsTrigger = " + _capsuleCollider.isTrigger.ToString() + "\n" +

                "\n" + "--- Movement Bools ---" + "\n" +
                "onGround = " + isGrounded.ToString() + "\n" +
                "lockMovement = " + lockMovement.ToString() + "\n" +
                "stopMove = " + stopMove.ToString() + "\n" +
                "sliding = " + isSliding.ToString() + "\n" +
                "sprinting = " + isSprinting.ToString() + "\n" +
                "crouch = " + isCrouching.ToString() + "\n" +
                "strafe = " + isStrafing.ToString() + "\n" +
                "landHigh = " + landHigh.ToString() + "\n" +

                "\n" + "--- Actions Bools ---" + "\n" +
                "roll = " + isRolling.ToString() + "\n" +
                "isJumping = " + isJumping.ToString() + "\n" +
                "ragdoll = " + ragdolled.ToString() + "\n" +
                "actions = " + actions.ToString() + "\n" +
                "customAction = " + customAction.ToString() + "\n" + additionalText;
        }
        return debugInfo;
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying && debugWindow)
        {
            //// debug auto crouch
            Vector3 posHead = transform.position + Vector3.up * ((colliderHeight * 0.5f) - colliderRadius);
            Ray ray1 = new Ray(posHead, Vector3.up);
            Gizmos.DrawWireSphere(ray1.GetPoint((headDetect - (colliderRadius * 0.1f))), colliderRadius * 0.9f);
            //// debug stopmove            
            Ray ray3 = new Ray(transform.position + new Vector3(0, stopMoveHeight, 0), transform.forward);
            Debug.DrawRay(ray3.origin, ray3.direction * (_capsuleCollider.radius + stopMoveDistance), Color.blue);
            //// debug slopelimit            
            Ray ray4 = new Ray(transform.position + new Vector3(0, colliderHeight / 3.5f, 0), transform.forward);
            Debug.DrawRay(ray4.origin, ray4.direction * 1f, Color.cyan);
            //// debug stepOffset
            var dir = isStrafing && input.magnitude > 0 ? (transform.right * input.x + transform.forward * input.y).normalized : transform.forward;
            Ray ray5 = new Ray((transform.position + new Vector3(0, stepOffsetEnd, 0) + dir * ((_capsuleCollider).radius + 0.05f)), Vector3.down);
            Debug.DrawRay(ray5.origin, ray5.direction * (stepOffsetEnd - stepOffsetStart), Color.yellow);
        }
    }

    #endregion

    [System.Serializable]
    public class vMovementSpeed
    {
        [Tooltip("Character will walk by default and run when the sprint input is pressed. The Sprint animation will not play")]
        public bool walkByDefault = true;
        [Tooltip("Speed to Run using rigibody force or extra speed if you're using RootMotion")]
        public float walkSpeed = 2f;
    }
}
