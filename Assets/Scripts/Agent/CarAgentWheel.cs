using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAgentWheel : MonoBehaviour
{
    [SerializeField] WheelCollider frontRight;
    [SerializeField] WheelCollider frontLeft;
    [SerializeField] WheelCollider backRight;
    [SerializeField] WheelCollider backLeft;

    [SerializeField] Transform frontRightTransform;
    [SerializeField] Transform frontLeftTransform;
    [SerializeField] Transform backRightTransform;
    [SerializeField] Transform backLeftTransform;

    public float ACCELERATION = 35f;
    public float BREAKING_FORCE = 200f;
    public float TURN_ANGLE = 25f;


    private float currentAcceleration = 0f;
    private float currentBreakForce = 0f;
    private float currentTurnAngle = 0f;
    private Rigidbody rigid;

    private float move = 0f;
    private float rotate = 0f;
    private float isBraking = 0f;

    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        rigid.centerOfMass = new Vector3(0, 0, 0);
    }

    public void SetActions(float actionMove, float actionRotate, bool actionBrake)
    {
        isBraking = (actionBrake) ? 1f : 0f;
        SetActions(actionMove, actionRotate, isBraking);
    }

    public void SetActions(float actionMove, float actionRotate, float actionBrake)
    {
        move = actionMove;
        rotate = actionRotate;
        isBraking = actionBrake;
    }

    // Update is called once per frame / FixedUpdate is called once every physics frame
    private void FixedUpdate()
    {
        Acceleration(move, isBraking);
        Steering(rotate);
        UpdateWheel();
    }

    public void ResetVelocity()
    {
        rigid.velocity = Vector3.zero;
        ResetWheels();
    }

    public void ResetWheels()
    {
        frontRight.motorTorque = 0;
        frontLeft.motorTorque = 0;

        frontRight.brakeTorque = Mathf.Infinity;
        frontLeft.brakeTorque = Mathf.Infinity;
    }

    private void Acceleration(float input, float isBraking)
    {
        // Acceleration
        currentAcceleration = ACCELERATION * input;

        // Brake
        currentBreakForce = BREAKING_FORCE * isBraking;

        // Front 
        frontRight.motorTorque = currentAcceleration;
        frontLeft.motorTorque = currentAcceleration;

        frontRight.brakeTorque = currentBreakForce;
        frontLeft.brakeTorque = currentBreakForce;
    }

    private void Steering(float input)
    {
        // Steering -----------------------------------------
        currentTurnAngle = TURN_ANGLE * input;
        //currentTurnAngle = (currentTurnAngle > MAX_TURN_ANGLE) ? MAX_TURN_ANGLE : currentTurnAngle;

        frontLeft.steerAngle = currentTurnAngle;
        frontRight.steerAngle = currentTurnAngle;
    }

    private void UpdateWheel()
    {
        // Move mesh -----------------------------------------
        UpdateWheel(frontLeft, frontLeftTransform);
        UpdateWheel(frontRight, frontRightTransform);

        UpdateWheel(backLeft, backLeftTransform);
        UpdateWheel(backRight, backRightTransform);
    }

    private void UpdateWheel(WheelCollider col, Transform trans) 
    {
        col.GetWorldPose(out Vector3 pos, out Quaternion rot);
        trans.position = pos;
        trans.rotation = rot;
    }
}
