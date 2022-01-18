using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;


public class CarAgent5 : Agent
{
    //mlagents-learn config/CarAgent5.yaml --run-id=CarAgent5_1 --env=builds/CarAgent5         
    [SerializeField] private Transform target;
    [SerializeField] private GPS pathfinding;
    [SerializeField] private EnvController envController;
    [SerializeField] private TextMesh debugTextMesh;

    //---
    private static ConfigAgent CONFIG = new ConfigAgent_5_1();
    private static ConfigReward REWARDS = new ConfigReward_5_1();

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
    private int [] actionSpace = {3, 3, 2};


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

        Vector3 localDirection = transform.InverseTransformDirection(direction).normalized;
        Vector3 localVelocity = transform.InverseTransformDirection(velocity);

        sensor.AddObservation(localDirection.x); 
        sensor.AddObservation(localDirection.z); 
        sensor.AddObservation(transform.localEulerAngles.y); 

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

        //speed
        float velocityRounded = Mathf.Round(velocity.magnitude * 1000f) / 1000f; //3 decimals
        if (velocityRounded > 0f) 
            reward += REWARDS.FORWARD;

        //steering
        //float angle = Mathf.Abs(Vector3.Angle(this.transform.forward, direction.normalized));
        //if(CONFIG.STEERING_ANGLE >= angle)
        //{
        //    reward += REWARDS.STEERING_ANGLE;
        //}

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
        float actionMove, actionRotate;
        int actionBrake;

        // speed
        float move = Input.GetAxisRaw("Vertical");
        if(move > 0)
            actionMove = 1f;
        else if(move < 0)
            actionMove = -1f;

        // rotate
        float rotate = Input.GetAxisRaw("Horizontal");
        if(rotate > 0)
            actionRotate = 1f;
        else if(rotate < 0)
            actionRotate = -1f;

        //break
        bool isBraking = Input.GetKey(KeyCode.Space);

        // [-1, 0, +1] => [0, 1, 2] *3 => [0, 3, 6]
        actionMove = move + 1f;                     
        actionMove = actionMove * ((float)(actionSpace[0]-1) / 2f);    

        actionRotate = rotate + 1f;
        actionRotate = actionRotate * ((float)(actionSpace[1]-1) / 2f); 

        // [0, +1] => [0, 2]
        actionBrake = (isBraking) ? (actionSpace[2]-1) : 0;       

        // Continous
        ActionSegment<int> actionArr = actionsOut.DiscreteActions;
        actionArr[0] = (int)actionMove;
        actionArr[1] = (int)actionRotate;
        actionArr[2] = (int)actionBrake;
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

            float move = (float)actions.DiscreteActions[0]; //[0,3]
            float rotate = (float)actions.DiscreteActions[1];
            float isBraking = (float)actions.DiscreteActions[2]; //[0,3]

            move = move / ((float)(actionSpace[0]-1) / 2f);
            move -= 1f;         
            rotate = rotate / ((float)(actionSpace[1]-1) / 2f);    // [0, 3, 6] => [0, 1, 2] => [-1, 0, +1]
            rotate -= 1f;     
            isBraking = isBraking / (actionSpace[2]-1f);    // [0, 2] => [0,1]

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
            AddReward(REWARDS.WALL_ENTER);
            Debug.Log("Hit wall!");
            FinishEpisode();
        }
    }

    private void OnCollisionStay(Collision collision) 
    {
        //if (collision.gameObject.tag == "Wall")
        //    SetReward(REWARDS.WALL_STAY);
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

        float vel_x = Mathf.Round(localVelocity.x * 100f) / 100f;
        float vel_z = Mathf.Round(localVelocity.z * 100f) / 100f;
        debugTextMesh.text = vel_x + "\n" + vel_z + "\n-\n" + actionOutput[0] + "\n" + actionOutput[1] + "\n" + actionOutput[2];
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
