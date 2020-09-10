using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerMotor : MonoBehaviour
{
    #region Components

    internal Rigidbody _rigidbody;

    #endregion

    #region Hidden Variables

    internal float speed;
    internal Vector2 input;                             //generate input for mobile
    internal float velocity;

    #endregion
    public void Init()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    public virtual void UpdateMotor()
    {
        print(input.x + " " + input.y);
    }

    protected virtual void FreeVelocity(float velocity)
    {
       
    }

    #region Locomotion

    public virtual void ControlLocomotion()
    {
        FreeMoviment();
    }

    public virtual void FreeMoviment()
    {
        //set speed to both vertical and horizontal (return positive value only)
    }

    #endregion
}
