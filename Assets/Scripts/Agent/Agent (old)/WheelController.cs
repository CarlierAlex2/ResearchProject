using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelController : MonoBehaviour
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

    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        rigid.centerOfMass = new Vector3(0, 0, 0);
    }

    // Update is called once per frame / FixedUpdate is called once every physics frame
    private void FixedUpdate()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");
        bool isBraking = Input.GetKey(KeyCode.Space);

        Acceleration(v, isBraking);
        Steering(h);
        UpdateWheel();
    }

    private void Acceleration(float input, bool isBraking)
    {
        // Acceleration
        currentAcceleration = ACCELERATION * input;

        // Brake
        if (isBraking)
            currentBreakForce = BREAKING_FORCE;
        else
            currentBreakForce = 0f;

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
