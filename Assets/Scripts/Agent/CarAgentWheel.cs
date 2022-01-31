using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 
/// Component that controls the car agent's wheels for thrust and steering.
/// <para>Unity's  WheelCollider component is used for the rotation and torque of the wheels.</para> 
/// <para>Documentation: https://docs.unity3d.com/Manual/class-WheelCollider.html</para> 
/// </summary>
public class CarAgentWheel : MonoBehaviour
{
    // --- Wheel colliders ---
    [SerializeField] WheelCollider frontRight;
    [SerializeField] WheelCollider frontLeft;
    [SerializeField] WheelCollider backRight;
    [SerializeField] WheelCollider backLeft;

    // --- Wheel transforms ---
    [SerializeField] Transform frontRightTransform;
    [SerializeField] Transform frontLeftTransform;
    [SerializeField] Transform backRightTransform;
    [SerializeField] Transform backLeftTransform;


    // --- Component ---
    private Rigidbody rigid;


    // --- Steering ---
    public float ACCELERATION = 35f;
    public float BREAKING_FORCE = 200f;
    public float TURN_ANGLE = 25f;
    //--
    private float currentAcceleration = 0f;
    private float currentBreakForce = 0f;
    private float currentTurnAngle = 0f;


    // --- Actions ---
    private float move = 0f;
    private float rotate = 0f;
    private float isBraking = 0f;

    /// <summary> 
    ///  Start is called before the first frame update.
    /// </summary>
    private void Start()
    {
        rigid = GetComponent<Rigidbody>();
        rigid.centerOfMass = new Vector3(0, 0, 0);
    }

    /// <summary> 
    ///  Set action values for the controller.
    /// </summary>
    public void SetActions(float actionMove, float actionRotate, bool actionBrake)
    {
        isBraking = (actionBrake) ? 1f : 0f;
        SetActions(actionMove, actionRotate, isBraking);
    }

    /// <summary> 
    ///  Set action values for the controller.
    /// </summary>
    public void SetActions(float actionMove, float actionRotate, float actionBrake)
    {
        move = actionMove;
        rotate = actionRotate;
        isBraking = actionBrake;
    }

    /// <summary> 
    /// FixedUpdate is called once for every physics frame.
    /// </summary>
    private void FixedUpdate()
    {
        Acceleration(move, isBraking);
        Steering(rotate);
        UpdateWheel();
        //NOTE: Update is called once per frame / FixedUpdate is called once every physics frame
    }

    /// <summary> 
    /// Reset the controller's speed.
    /// </summary>
    public void ResetVelocity()
    {
        rigid.velocity = Vector3.zero;
        ResetWheels();
    }

    /// <summary> 
    /// Resets the wheels.
    /// </summary>
    public void ResetWheels()
    {
        frontRight.motorTorque = 0;
        frontLeft.motorTorque = 0;

        frontRight.brakeTorque = Mathf.Infinity;
        frontLeft.brakeTorque = Mathf.Infinity;
    }

    /// <summary> 
    /// Calculate the torque speed for the wheels.
    /// </summary>
    private void Acceleration(float input, float isBraking)
    {
        // acceleration
        currentAcceleration = ACCELERATION * input;

        // brake
        currentBreakForce = BREAKING_FORCE * isBraking;

        // front wheel drive
        frontRight.motorTorque = currentAcceleration;
        frontLeft.motorTorque = currentAcceleration;

        frontRight.brakeTorque = currentBreakForce;
        frontLeft.brakeTorque = currentBreakForce;
    }

    /// <summary> 
    /// Calculate the wheel steering.
    /// </summary>
    private void Steering(float input)
    {
        // steering
        currentTurnAngle = TURN_ANGLE * input;

        frontLeft.steerAngle = currentTurnAngle;
        frontRight.steerAngle = currentTurnAngle;
    }

    /// <summary> 
    /// Update the wheels models position and rotation (only visuals).
    /// </summary>
    private void UpdateWheel()
    {
        // Move mesh -----------------------------------------
        UpdateWheel(frontLeft, frontLeftTransform);
        UpdateWheel(frontRight, frontRightTransform);

        UpdateWheel(backLeft, backLeftTransform);
        UpdateWheel(backRight, backRightTransform);
    }

    /// <summary> 
    /// Update the individual wheel model position and rotation (only visuals).
    /// </summary>
    private void UpdateWheel(WheelCollider col, Transform trans) 
    {
        col.GetWorldPose(out Vector3 pos, out Quaternion rot);
        trans.position = pos;
        trans.rotation = rot;
    }
}
