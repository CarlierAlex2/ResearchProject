using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;


public class CarAgent6 : Agent
{
    //mlagents-learn config/PPO_CarAgent6.yaml --run-id=PPO_CarAgent6_4_128 --env=builds/PPO_CarAgent6
    //mlagents-learn config/PPO_CarAgent6_b256.yaml --run-id=PPO_CarAgent6_4_256 --env=builds/PPO_CarAgent6 
    //mlagents-learn config/PPO_CarAgent6_b512.yaml --run-id=PPO_CarAgent6_4_512 --env=builds/PPO_CarAgent6 

    //mlagents-learn config/PPO_CarAgent6_n32.yaml --run-id=PPO_CarAgent6_4_n32 --env=builds/PPO_CarAgent6
    //mlagents-learn config/PPO_CarAgent6_n64.yaml --run-id=PPO_CarAgent6_4_n64 --env=builds/PPO_CarAgent6 
    //mlagents-learn config/PPO_CarAgent6_n128.yaml --run-id=PPO_CarAgent6_4_n128 --env=builds/PPO_CarAgent6 

    //mlagents-learn config/SAC_CarAgent6.yaml --run-id=SAC_CarAgent6 --env=builds/SAC_CarAgent6 

    //mlagents-learn config/SAC_CarAgent6_test4.yaml --run-id=SAC_CarAgent6_test4 --env=builds/SAC_CarAgent6 
    //mlagents-learn config/SAC_CarAgent6_test5.yaml --run-id=SAC_CarAgent6_test5 --env=builds/SAC_CarAgent6 

    //mlagents-learn config/PPO/PPO_CarAgent6.yaml --run-id=PPO_CarAgent6_5_test1 --env=builds/PPO_CarAgent6

    //mlagents-learn config/SAC/SAC_CarAgent6_b1024_n64_s20.yaml --run-id=SAC_CarAgent6_5_b1024_n64_s20 --env=builds/SAC_CarAgent6

    //mlagents-learn config/PPO/PPO_CarAgent6_5_buff5120.yaml --run-id=PPO_CarAgent6_5_buff5120_noAngle --env=builds/PPO_CarAgent6

    //mlagents-learn config/PPO/PPO_CarAgent6_5_buffersize.yaml --run-id=PPO_6_5_angle_120_none --env=builds/PPO_CarAgent6_multi
    

    [SerializeField] private Transform target;
    [SerializeField] private GPS pathfinding;
    [SerializeField] private EnvController envController;
    [SerializeField] private TextMesh debugTextMesh;

    //---
    private static ConfigAgent CONFIG = new ConfigAgent_6_1();
    private static ConfigReward REWARDS = new ConfigReward_6_1();
    private static RewardFunctions REWARD_FUNCTIONS = new RewardFunctions(CONFIG, REWARDS);

    //---
    private Rigidbody rigid;
    private CarAgentWheel wheelController;

    //---
    private int index = 0;
    private List<Vector3> path;

    //---
    private Vector3 positionOld;
    private Vector3 positionNew;
    private Vector3 forwardOld;
    private Vector3 forwardNew;
    private Vector3 checkpoint;
    private Vector3 velocityOld;
    private Vector3 velocityNew;

    //---
    private bool isEpisodeRunning = false;
    private bool isCheckPoint = false;
    private bool stopEpisode = false;
     
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

        positionOld = Vector3.zero;
        positionNew = Vector3.zero;
        forwardOld = Vector3.zero;
        forwardNew = Vector3.zero;
        velocityOld = Vector3.zero;
        velocityNew = Vector3.zero;
        checkpoint = Vector3.zero;

        stopEpisode = false;
        isEpisodeRunning = true;
        isCheckPoint = false;
        Debug.Log("Reset Agent");
    }

    //--- OBSERVATION ---------------------------------------------------------------------
    public override void CollectObservations(VectorSensor sensor)
    {
        if(isEpisodeRunning)
        {
            UpdatePath();   //Pathfinding
            GetAgentInfo(); //Data
            RewardCar();    //Rewards
        }

        Vector3 directionOld = checkpoint - positionOld;
        Vector3 directionNew = checkpoint - positionNew;
        directionOld.y = 0;
        directionNew.y = 0;

        sensor.AddObservation(directionOld); 
        sensor.AddObservation(directionNew);

        sensor.AddObservation(forwardOld); 
        sensor.AddObservation(forwardNew); 

        sensor.AddObservation(velocityOld); 
        sensor.AddObservation(velocityNew); 

        // Debug
        DebugObservation();
        DebugPath();
    }

    //--- INFO ---------------------------------------------------------------------
    private void GetAgentInfo()
    {
        positionOld = positionNew;
        positionNew = transform.position;

        forwardOld = forwardNew;
        forwardNew = transform.forward;

        velocityOld = velocityNew;
        velocityNew = rigid.velocity;
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
        //--
        if(!isEpisodeRunning || path == null)
            return;

        //--
        if (isEpisodeRunning)
        {
            //Finish
            if (stopEpisode) 
                FinishEpisode();

            //Actions
            float move = (float)actions.DiscreteActions[0];
            float rotate = (float)actions.DiscreteActions[1];
            float isBraking = (float)actions.DiscreteActions[2];

            move = move / ((float)(actionSpace[0]-1) / 2f);
            move -= 1f;         
            rotate = rotate / ((float)(actionSpace[1]-1) / 2f);    // [0, 3, 6] => [0, 1, 2] => [-1, 0, +1]
            rotate -= 1f;     
            isBraking = isBraking / ((float)actionSpace[2]-1f);    // [0, 2] => [0, 1]

            //Debug
            actionOutput[0] = move;
            actionOutput[1] = rotate;
            actionOutput[2] = isBraking;
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


    //--- REWARDS ---------------------------------------------------------------------
    private void RewardCar()
    {
        float reward = 0f;

        //--
        Vector3 localVelocity = transform.InverseTransformDirection(velocityNew);

        reward += REWARD_FUNCTIONS.Time();
        reward += REWARD_FUNCTIONS.Movement(localVelocity.z, actionOutput);
        reward += REWARD_FUNCTIONS.CheckpointDist(positionOld, positionNew, checkpoint);
        reward += REWARD_FUNCTIONS.CheckpointAngle(positionOld, forwardOld, positionNew, forwardNew, checkpoint);

        //--
        AddReward(reward);

        //--
        isCheckPoint = false;
        float minReward = -10f;
        stopEpisode = (reward <= minReward) ? true : stopEpisode;
    }


    //--- PATH ---------------------------------------------------------------------
    private void UpdatePath()
    {
        if(path == null) // check if path exists
            SetPath();
        if(path == null) // still doesn't exist => no direction
            return;
        checkpoint = path[index];

        //--
        Vector3 checkpointVector = checkpoint - (this.transform.position + transform.forward * CONFIG.CHECKPOINT_OFFSET);
        checkpointVector.y = 0;
        if(checkpointVector.magnitude <= CONFIG.CHECKPOINT_RANGE)
        {
            index++;
            stopEpisode = (index >= path.Count) ? true : false;

            index = (index > path.Count - 1) ? (path.Count - 1) : index;
            checkpoint = path[index];

            isCheckPoint = true;
            Debug.Log("Update path, index=" + index);
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
    private void DebugObservation()
    {
        Vector3 directionNew = checkpoint - positionNew;
        Vector3 velocity = rigid.velocity;

        Vector3 c = transform.position + Vector3.up * 1f;
        Debug.DrawLine(c, c + directionNew, Color.red);
        Debug.DrawLine(c, c + velocity, Color.blue);

        debugTextMesh.text = actionOutput[0] + "\n" + actionOutput[1] + "\n" + actionOutput[2];
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
