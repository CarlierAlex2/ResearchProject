using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;



public class CarAgent1 : Agent
{
    //mlagents-learn config/CarAgent1.yaml --run-id=CarAgent2 --env=builds/CarAgent1
    [SerializeField] private Transform target;
    [SerializeField] private Transform gpsObj;

    //---
    private ConfigAgent configAgent = new ConfigAgent();
    private ConfigReward configReward = new ConfigReward();
    
    //---
    private Rigidbody rigid;
    private CarAgentWheel wheelController;
    private GPS pathfinding;

    //---
    private Vector3 startPos;
    private Quaternion startRot;

    //---
    private int index = 0;
    private List<Vector3> path;
    private Vector3 direction;

    //---
    private bool isEpisodeRunning = false;
    private bool isCheckPoint = false;


    //--- INITIALIZE ---------------------------------------------------------------------
    public override void Initialize()
    {
        Debug.Log("Initialize Agent");
        rigid = GetComponent<Rigidbody>();
        wheelController = GetComponent<CarAgentWheel>();
        pathfinding = gpsObj.GetComponent<GPS>();

        startPos = transform.position;
        startRot = transform.rotation;
    }

    public override void OnEpisodeBegin()
    {
        transform.position = startPos;
        transform.rotation = startRot;

        rigid.velocity = Vector3.zero;
        wheelController.ResetWheels();

        index = 0;
        path = null;
        direction = Vector3.zero;

        isEpisodeRunning = true;

        isCheckPoint = false;
    }

    //--- OBSERVATION ---------------------------------------------------------------------
    public override void CollectObservations(VectorSensor sensor)
    {
        if(isEpisodeRunning)
        {
            UpdatePath();
            RewardCar();
        }

        sensor.AddObservation(direction); 
        Vector3 velocity = rigid.velocity;
        velocity.y = 0;
        Vector3 localVelocity = transform.InverseTransformDirection(velocity);
        sensor.AddObservation(localVelocity); 
    }


    //--- REWARDS ---------------------------------------------------------------------
    private void RewardCar()
    {
        float reward = configReward.TIME;

        // move + velocity
        Vector3 velocity = rigid.velocity;
        velocity.y = 0;
        Vector3 localVelocity = transform.InverseTransformDirection(velocity);
        var forwardSpeed = localVelocity.z;
        if(Mathf.Abs(forwardSpeed) >= configAgent.VELOCITY_MIN)
        {
            reward += configReward.VELOCITY_MIN;
            if(forwardSpeed > 0)
                reward += configReward.VELOCITY_FORWARD;
        }

        //float velocityAngle = Mathf.Abs(Vector3.Angle(this.transform.forward, velocity.normalized));
        //if(velocityAngle < configAgent.VELOCITY_ANGLE)
        //    reward += configReward.VELOCITY_ANGLE;

        //checkpoints
        if(isCheckPoint)
        {
            reward += configReward.CHECKPOINT_RANGE;
            isCheckPoint = false;
        }
        float angle = Mathf.Abs(GetAngleDirection());
        if (angle > configAgent.CHECKPOINT_ANGLE_MAX)
        {
            reward += configReward.CHECKPOINT_PASS;
            SetPath();
        }

        //finish episode
        if (index >= path.Count - 1)
        {
            reward += configReward.GOAL;
        }
        SetReward(reward);
    }


    //--- ACTIONS ---------------------------------------------------------------------
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Input keys
        float move = Input.GetAxis("Vertical");
        float rotate = Input.GetAxis("Horizontal");
        bool isBraking = Input.GetKey(KeyCode.Space);

        // Continous
        ActionSegment<float> actionsContinu = actionsOut.ContinuousActions;
        actionsContinu[0] = move;
        actionsContinu[1] = rotate;

        // Discrete
        ActionSegment<int> actionsDiscrete = actionsOut.DiscreteActions;
        actionsDiscrete[0] = (isBraking) ? 1 : 0;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {      
        if(!isEpisodeRunning || path == null)
            return;

        if (isEpisodeRunning)
        {
            if (index >= (path.Count - 1)) //finish episode
                FinishEpisode();

            float move = actions.ContinuousActions[0];
            float rotate = actions.ContinuousActions[1];
            bool isBraking = (actions.DiscreteActions[0] == 1);

            if (isBraking)
                AddReward(configReward.BREAK);

            wheelController.SetActions(move, rotate, isBraking);
        }
    }


    //--- PATH ---------------------------------------------------------------------
    private Vector3 GetDirection(float offset = 0f)
    {
        if(index < path.Count)
        {
            Vector3 d = path[index] - (this.transform.position + this.transform.forward * offset);
            d.y = 0;
            return d;
        }
        return Vector3.zero;
    }

    private void UpdatePath()
    {
        if(path == null) // check if path exists
            SetPath();
        if(path == null) // still doesn't exist => no direction
        {
            direction = Vector3.zero;
            return;
        }
            
        Vector3 checkpointVector = GetDirection(configAgent.CHECKPOINT_OFFSET); // get current direction
        if(checkpointVector.magnitude <= configAgent.CHECKPOINT_RANGE)
        {
            index++;
            isCheckPoint = true;
            Debug.Log("Update path, index=" + index);
        }
        direction = GetDirection();
        direction = transform.InverseTransformDirection(direction).normalized;
    }

    private void SetPath()
    {
        path = pathfinding.FindPath(this.transform.position, target.position);
        index = configAgent.INDEX_START;
    }

    private float GetAngleDirection()
    {
        Vector3 newDirection = GetDirection();
        if(newDirection == Vector3.zero)
            return 0;
        newDirection.Normalize();

        Vector3 forward = transform.forward;
        forward.y = 0;
        forward.Normalize();
        return Vector3.Angle(forward, newDirection);
    }


    //--- COLLISIONS ---------------------------------------------------------------------
    private void OnCollisionEnter(Collision collision) 
    {
        if (collision.gameObject.tag == "Wall")
        {
            SetReward(configReward.WALL_ENTER);
            Debug.Log("Hit wall!");
        }
    }

    private void OnCollisionStay(Collision collision) 
    {
        if (collision.gameObject.tag == "Wall")
            SetReward(configReward.WALL_STAY);
    }


    //--- FINISH ---------------------------------------------------------------------
    private void FinishEpisode()
    {
        isEpisodeRunning = false;
        Debug.Log("End episode!");
        EndEpisode(); // calls CollectObservations => don't nest
    }
}
