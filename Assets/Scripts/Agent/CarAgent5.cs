using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;


public class CarAgent5 : Agent
{
    //mlagents-learn config/CarAgent4.yaml --run-id=CarAgent4_1 --env=builds/CarAgent4_parking
    //mlagents-learn config/CarAgent5_1.yaml --run-id=CarAgent5_1 --env=builds/CarAgent5_center         
    [SerializeField] private Transform target;
    [SerializeField] private GPS pathfinding;
    [SerializeField] private EnvController envController;
    [SerializeField] private TextMesh debugTextMesh;

    //---
    private static ConfigAgent CONFIG = new ConfigAgent_5_2();
    private static ConfigReward REWARDS = new ConfigReward_5_2();

    //---
    private Rigidbody rigid;
    private CarAgentWheel wheelController;


    //---
    private int index = 0;
    private List<Vector3> path;
    private Vector3 direction;
    private Vector3 velocity;


    //---
    private bool isEpisodeRunning = false;
    private bool isCheckPoint = false;
    private bool isAtGoal = false;
     
    private float [] actionOutput = {0, 0, 0};


    //--- INITIALIZE ---------------------------------------------------------------------
    public override void Initialize()
    {
        Debug.Log("Initialize Agent");
        rigid = GetComponent<Rigidbody>();
        wheelController = GetComponent<CarAgentWheel>();
    }

    public override void OnEpisodeBegin()
    {
        envController.InitEnvironment(); //watch out for infinite loop, Initialize or OnEpisodeBegin first?
        envController.ResetEnvironment(this.transform);
        wheelController.ResetVelocity();
        ResetAgent();
    }

    private void ResetAgent()
    {
        index = 0;
        path = null;
        direction = Vector3.zero;
        velocity = Vector3.zero;

        isAtGoal = false;
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

        Vector3 localDirection = transform.InverseTransformDirection(direction);
        Vector3 localVelocity = transform.InverseTransformDirection(velocity);

        sensor.AddObservation(localDirection.x); 
        sensor.AddObservation(localDirection.z); 

        sensor.AddObservation(localVelocity.x); 
        sensor.AddObservation(localVelocity.z); 

        // Debug
        DebugObservation(direction, localVelocity);
        DebugPath();
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
            direction = path[index] - this.transform.position;
            direction.y = 0;
        }
        else
            direction = Vector3.zero;
    }

    private void GetVelocity()
    {
        velocity = rigid.velocity;
        velocity.y = 0;
    }

    //--- REWARDS ---------------------------------------------------------------------
    private void RewardCar()
    {
        //time
        float reward = 0f;
        reward += REWARDS.TIME;

        //steeering
        float angle = Vector3.Angle(this.transform.forward, direction.normalized);
        if(CONFIG.STEERING_ANGLE >= angle && angle >= -CONFIG.STEERING_ANGLE)
        {
            reward += REWARDS.STEERING_ANGLE;
        }

        //finish episode + checkpoints
        Vector3 toGoal = (target.position - this.transform.position);
        toGoal.y = 0;

        if (isCheckPoint)
        {
            reward += REWARDS.CHECKPOINT_RANGE;
            isCheckPoint = false;
        }

        //--
        AddReward(reward);
    }


    //--- ACTIONS ---------------------------------------------------------------------
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Input keys
        float move = Input.GetAxis("Vertical");
        float rotate = Input.GetAxis("Horizontal");
        bool isBraking = Input.GetKey(KeyCode.Space);

        int actionMove = Mathf.FloorToInt((move + 1f) * 3f); // [-1,+1] => [0,2] => [0,6]
        int actionRotate = Mathf.FloorToInt((rotate + 1f) * 3f); // [-1,+1] => [0,2] => [0,6]
        int actionBrake = (isBraking) ? 3 : 0;  // [-1,+1] => [0,3]

        // Continous
        ActionSegment<int> actionArr = actionsOut.DiscreteActions;
        actionArr[0] = actionMove;
        actionArr[1] = actionRotate;
        actionArr[2] = actionBrake;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {      
        float reward = 0;

        //--
        if(!isEpisodeRunning || path == null)
            return;

        //--
        if (isEpisodeRunning)
        {
            if (isAtGoal) //finish episode
                FinishEpisode();

            float move = actions.DiscreteActions[0]; //[0,8]
            float rotate = actions.DiscreteActions[1];
            float isBraking = actions.DiscreteActions[2]; //[0,3]

            move = (move / 3f) - 1f; //[0,6] > [-1,+1]
            rotate = (rotate / 3f) - 1f; //[0,6] > [-1,+1]
            isBraking = isBraking / 3f;

            // Debug
            actionOutput[0] = move;
            actionOutput[1] = rotate;
            actionOutput[2] = isBraking;
            wheelController.SetActions(move, rotate, isBraking);

            //breaking
            if (isBraking > 0f) 
                reward += REWARDS.BREAK;
        }
        else
        {
            // Debug
            actionOutput[0] = 0;
            actionOutput[1] = 0;
            actionOutput[2] = 0;
        }

        AddReward(reward);
    }


    //--- PATH ---------------------------------------------------------------------
    private void UpdatePath()
    {
        if(path == null) // check if path exists
            SetPath();
        if(path == null) // still doesn't exist => no direction
        {
            direction = Vector3.zero;
            return;
        }

        //--
        Vector3 checkpointVector = path[index] - (this.transform.position + transform.forward * CONFIG.CHECKPOINT_OFFSET);
        checkpointVector.y = 0;
        if(checkpointVector.magnitude <= CONFIG.CHECKPOINT_RANGE)
        {
            index++;
            index = (index > path.Count - 1) ? (path.Count - 1) : index;

            isCheckPoint = true;
            Debug.Log("Update path, index=" + index);
        }

        //--
        Vector3 toGoal = (path[path.Count - 1] -this.transform.position);
        if (toGoal.magnitude < CONFIG.CHECKPOINT_RANGE)
        {
            isAtGoal = true;
        }
    }

    private void SetPath()
    {
        path = pathfinding.FindPath(this.transform.position, target.position);
        index = CONFIG.INDEX_START;
        Debug.Log("Path calculated");
    }


    //--- COLLISIONS ---------------------------------------------------------------------
    private void OnCollisionEnter(Collision collision) 
    {
        if (collision.gameObject.tag == "Wall")
        {
           SetReward(REWARDS.WALL_ENTER);
            Debug.Log("Hit wall!");
        }
    }

    private void OnCollisionStay(Collision collision) 
    {
        if (collision.gameObject.tag == "Wall")
            SetReward(REWARDS.WALL_STAY);
    }


    //--- FINISH ---------------------------------------------------------------------
    private void FinishEpisode()
    {
        isEpisodeRunning = false;
        Debug.Log("End episode!");
        EndEpisode(); // calls CollectObservations => don't nest
    }

    //--- DEBUG ---------------------------------------------------------------------
    private void DebugObservation(Vector3 localDirectionGPS, Vector3 localVelocity)
    {
        Vector3 c = transform.position + Vector3.up * 1f;
        Debug.DrawLine(c, c + direction, Color.red);
        Debug.DrawLine(c, c + velocity, Color.blue);

        debugTextMesh.text = localVelocity.x + "\n" + localVelocity.z + "\n-\n" + actionOutput[0] + "\n" + actionOutput[1] + "\n" + actionOutput[2];
    }

    private void DebugPath()
    {
        Vector3 c = Vector3.up * 1f;
        for(int i = 0; i < path.Count-1; i++)
            Debug.DrawLine(c + path[i], c + path[i+1], Color.green, 1f);

        Vector3 toGoal = (path[path.Count - 1] -this.transform.position);
        toGoal.y = 0;
        c += transform.position;
        Debug.DrawLine(c, c + toGoal, Color.magenta);
    }
}
