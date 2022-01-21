using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;


public class CarAgent6 : Agent
{
    //mlagents-learn config/CarAgent6.yaml --run-id=CarAgent6 --env=builds/CarAgent6      
    [SerializeField] private Transform target;
    [SerializeField] private GPS pathfinding;
    [SerializeField] private EnvController envController;
    [SerializeField] private TextMesh debugTextMesh;

    //---
    private static ConfigAgent CONFIG = new ConfigAgent_6_1();
    private static ConfigReward REWARDS = new ConfigReward_6_1();

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
    private int [] actionSpace = {7, 7, 3};


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

        var localVelocity = transform.InverseTransformDirection(velocity);
        var localDirection = transform.InverseTransformDirection(direction).normalized;

        //pass normalized
        var obsVelocity = new Vector2(localVelocity.x, localVelocity.z) / float.MaxValue;
        var obsDirection = new Vector2(localDirection.x, localDirection.z) / float.MaxValue;
        var obsAngleDirection = (Vector3.Angle(transform.forward, direction.normalized)) / 360f;

        sensor.AddObservation(obsAngleDirection); 
        sensor.AddObservation(obsDirection); 
        sensor.AddObservation(obsVelocity); 

        // Debug
        DebugObservation(direction, velocity);
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

        //distance checkpoint
        //float distance = direction.magnitude;
        //reward += (1 - (distance / CONFIG.CHECKPOINT_DIST)) * REWARDS.CHECKPOINT_DIST;

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

        float move = Input.GetAxisRaw("Vertical");
        float rotate = Input.GetAxisRaw("Horizontal");
        bool isBraking = Input.GetKey(KeyCode.Space);

        // [-1, 0, +1] => [0, 1, 2] *3 => [0, 3, 6]                 
        actionMove = (move + 1f) * ((float)(actionSpace[0]-1) / 2f);    
        actionRotate = (rotate + 1f) * ((float)(actionSpace[1]-1) / 2f); 

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

            float move = (float)actions.DiscreteActions[0];
            float rotate = (float)actions.DiscreteActions[1];
            float isBraking = (float)actions.DiscreteActions[2];

            move = move / ((float)(actionSpace[0]-1) / 2f);
            move -= 1f;         
            rotate = rotate / ((float)(actionSpace[1]-1) / 2f);    // [0, 3, 6] => [0, 1, 2] => [-1, 0, +1]
            rotate -= 1f;     
            isBraking = isBraking / ((float)actionSpace[2]-1f);    // [0, 2] => [0, 1]

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
    private void OnTriggerEnter(Collider other) 
    {
        if (other.tag == "Wall")
        {
            AddReward(REWARDS.WALL_ENTER);
            Debug.Log("Hit wall!");
            FinishEpisode();
        }
    }

    private void OnCollisionEnter(Collision collision) 
    {

    }

    private void OnCollisionStay(Collision collision) 
    {

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
