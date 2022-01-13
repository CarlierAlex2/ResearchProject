using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;



public class CarAgent : Agent
{
    //mlagents-learn config/CarAgent2.yaml --run-id=CarAgent2 --env=builds/CarAgent2
    [SerializeField] private Transform target;
    [SerializeField] private Transform gpsObj;
    [SerializeField] private TextMesh debugTextMesh;

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
    private Vector3 directionGPS;
    private Vector3 velocity;

    //---
    private bool isEpisodeRunning = false;
    private bool isCheckPoint = false;

    private float [] actionOutput = {0, 0, 0};


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
        ResetEnvironment();
        wheelController.ResetVelocity();
        ResetAgent();
    }

    private void ResetEnvironment()
    {
        transform.position = startPos;
        transform.rotation = startRot;
    }

    private void ResetAgent()
    {
        index = 0;
        path = null;
        directionGPS = Vector3.zero;

        isEpisodeRunning = true;
        isCheckPoint = false;
        Debug.Log("Reset Agent");
    }

    //--- OBSERVATION ---------------------------------------------------------------------
    public override void CollectObservations(VectorSensor sensor)
    {
        if(isEpisodeRunning)
        {
            UpdatePath();
            GetAgentInfo();
            RewardCar();
        }


        Vector3 localDirectionGPS = transform.InverseTransformDirection(directionGPS);
        Vector3 localVelocity = transform.InverseTransformDirection(velocity);

        sensor.AddObservation(localDirectionGPS.x); 
        sensor.AddObservation(localDirectionGPS.z); 

        sensor.AddObservation(localVelocity.x); 
        sensor.AddObservation(localVelocity.z); 

        // Debug
        Vector3 c = transform.position + Vector3.up * 1f;
        Debug.DrawLine(c, c + directionGPS, Color.red);
        Debug.DrawLine(c, c + velocity, Color.blue);

        debugTextMesh.text = localVelocity.x + "\n" + localVelocity.z + "\n-\n" + actionOutput[0] + "\n" + actionOutput[1] + "\n" + actionOutput[2];
    }

    //--- INFO ---------------------------------------------------------------------
    private void GetAgentInfo()
    {
        GetDirectionGPS();
        GetVelocity();
    }

    private void GetDirectionGPS(float offset = 0f)
    {
        if(index < path.Count)
        {
            directionGPS = path[index] - this.transform.position;
            directionGPS.y = 0;
            directionGPS.Normalize();
        }
        else
            directionGPS = Vector3.zero;
    }

    private void GetVelocity()
    {
        velocity = rigid.velocity;
        velocity.y = 0;
    }

    //--- REWARDS ---------------------------------------------------------------------
    private void RewardCar()
    {
        float reward = ConfigReward.TIME;

        // move + velocity
        Vector3 velocity = rigid.velocity;
        velocity.y = 0;
        Vector3 localVelocity = transform.InverseTransformDirection(velocity);
        var forwardSpeedDiff = (1 - localVelocity.z);
        var rewardVelocity = Mathf.Abs(forwardSpeedDiff) / ConfigAgent.VELOCITY_MIN;
        reward += ((rewardVelocity > 0) ? (rewardVelocity * ConfigReward.VELOCITY_MIN) : 0f);

        //miss turn
        //Vector3 forward = transform.forward;
        //forward.y = 0;
        //forward.Normalize();
        //float angle = Mathf.Abs(Vector3.Angle(forward, directionGPS));
        //if (angle > ConfigAgent.DIRECTION_ANGLE)
        //{
        //    reward += ConfigReward.CHECKPOINT_PASS;
        //    SetPath();
        //}

        //finish episode + checkpoints
        if (index >= path.Count - 1)
        {
            reward += ConfigReward.GOAL;
        }
        else if (isCheckPoint)
        {
            reward += ConfigReward.CHECKPOINT;
            isCheckPoint = false;
        }

        //--
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

            // Debug
            actionOutput[0] = move;
            actionOutput[1] = rotate;
            actionOutput[2] = actions.DiscreteActions[0];

            if (isBraking)
                AddReward(ConfigReward.BREAK);

            wheelController.SetActions(move, rotate, isBraking);
        }
        else
        {
            // Debug
            actionOutput[0] = 0;
            actionOutput[1] = 0;
            actionOutput[2] = 0;
        }


    }


    //--- PATH ---------------------------------------------------------------------
    private void UpdatePath()
    {
        if(path == null) // check if path exists
            SetPath();
        if(path == null) // still doesn't exist => no direction
        {
            directionGPS = Vector3.zero;
            return;
        }

        //--
        Vector3 checkpointVector = path[index] - (this.transform.position + transform.forward * ConfigAgent.CHECKPOINT_OFFSET);
        checkpointVector.y = 0;
        if(checkpointVector.magnitude <= ConfigAgent.CHECKPOINT_RANGE)
        {
            index++;
            isCheckPoint = true;
            Debug.Log("Update path, index=" + index);
        }
    }

    private void SetPath()
    {
        path = pathfinding.FindPath(this.transform.position, target.position);
        index = ConfigAgent.INDEX_START;
        Debug.Log("Path calculated");
    }


    //--- COLLISIONS ---------------------------------------------------------------------
    private void OnCollisionEnter(Collision collision) 
    {
        if (collision.gameObject.tag == "Wall")
        {
            SetReward(ConfigReward.WALL_ENTER);
            Debug.Log("Hit wall!");
        }
    }

    private void OnCollisionStay(Collision collision) 
    {
        if (collision.gameObject.tag == "Wall")
            SetReward(ConfigReward.WALL_STAY);
    }


    //--- FINISH ---------------------------------------------------------------------
    private void FinishEpisode()
    {
        isEpisodeRunning = false;
        Debug.Log("End episode!");
        EndEpisode(); // calls CollectObservations => don't nest
    }
}
