using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;


/// <summary> 
/// Defines the agent's behavior.
/// <para>This class inherits from the agent class from the Unity ML-Agents Toolkit</para> 
/// <para>Github: https://github.com/Unity-Technologies/ml-agents</para> 
/// </summary>
public class CarAgent6 : Agent
{
    // --- External objects ---
    [SerializeField] private Transform target;
    [SerializeField] private GPS pathfinding;
    [SerializeField] private EnvController envController;
    [SerializeField] private TextMesh debugTextMesh;


    // --- Configuration ---
    private static ConfigAgent CONFIG = new ConfigAgent_6_4();
    private static ConfigReward REWARDS = new ConfigReward_6_4();
    private static RewardFunctions REWARD_FUNCTIONS = new RewardFunctions(CONFIG, REWARDS);


    // --- Components ---
    private Rigidbody rigid;
    private CarAgentWheel wheelController;


    // --- Pathfinding ---
    private int index = 0;
    private List<Vector3> path;
    private bool isCheckPoint = false;


    // --- Observation ---
    private Vector3 positionOld;
    private Vector3 positionNew;
    private Vector3 forwardOld;
    private Vector3 forwardNew;
    private Vector3 checkpoint;
    private Vector3 velocityOld;
    private Vector3 velocityNew;
     

    // --- Actions ---
    private float [] actionOutput = {0, 0, 0};
    private int [] actionSpace = {7, 7, 3};


    // --- Episode ---
    private bool isEpisodeRunning = false;
    private bool stopEpisode = false;


    //--- INITIALIZE ---------------------------------------------------------------------
    /// <summary> 
    /// This method is executed at the start of the training. 
    // <para>Overriden from the Agent class from the Unity ML-Agents Toolkit.</para> 
    /// </summary>
    public override void Initialize()
    {
        Debug.Log("Initialize Agent");
        rigid = GetComponent<Rigidbody>();
        wheelController = GetComponent<CarAgentWheel>();
    }

    /// <summary> 
    /// This method is executed at the start of each episode. 
    // <para>Overriden from the Agent class from the Unity ML-Agents Toolkit.</para> 
    /// </summary>
    public override void OnEpisodeBegin()
    {
        // initialize & reset environment
        envController.InitEnvironment();
        envController.ResetEnvironment(this.transform);

        // reset agent
        wheelController.ResetVelocity();
        ResetAgent();
    }

    /// <summary> 
    /// This method resets the agent variables for a new episode. 
    /// </summary>
    private void ResetAgent()
    {
        //pathfinding
        index = 0;
        path = null;
        isCheckPoint = false;

        //observations
        positionOld = Vector3.zero;
        positionNew = Vector3.zero;
        forwardOld = Vector3.zero;
        forwardNew = Vector3.zero;
        velocityOld = Vector3.zero;
        velocityNew = Vector3.zero;
        checkpoint = Vector3.zero;

        //episode
        stopEpisode = false;
        isEpisodeRunning = true;
    }

    //--- OBSERVATION ---------------------------------------------------------------------
    /// <summary> 
    /// This method is used to collect observations for the agent and is executed each step. 
    // <para>Overriden from the Agent class from the Unity ML-Agents Toolkit.</para> 
    /// </summary>
    public override void CollectObservations(VectorSensor sensor)
    {
        // check agent observations and reward when episode is running
        if(isEpisodeRunning)
        {
            UpdatePath();   //Pathfinding
            GetAgentInfo(); //Data
            RewardCar();    //Rewards
        }

        // get directions to checkpoint (including vector magnitude)
        Vector3 directionOld = checkpoint - positionOld;
        Vector3 directionNew = checkpoint - positionNew;
        directionOld.y = 0;
        directionNew.y = 0;

        // add observations
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
    /// <summary> 
    /// This method is retrieves the agent's current state for observations. 
    /// </summary>
    private void GetAgentInfo()
    {
        // agent position
        positionOld = positionNew;
        positionNew = transform.position;

        // agent forward vector
        forwardOld = forwardNew;
        forwardNew = transform.forward;

        // agent velocity
        velocityOld = velocityNew;
        velocityNew = rigid.velocity;
    }


    //--- ACTIONS ---------------------------------------------------------------------
    /// <summary> 
    /// This method is used to manually control the agent using input keys. 
    // <para>Overriden from the Agent class from the Unity ML-Agents Toolkit.</para> 
    /// </summary>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // input keys
        float move = Input.GetAxisRaw("Vertical");
        float rotate = Input.GetAxisRaw("Horizontal");
        bool isBraking = Input.GetKey(KeyCode.Space);

        // convert input keys to actions (same range as output of model)               
        float actionMove = (move + 1f) * ((float)(actionSpace[0]-1) / 2f);    
        float actionRotate = (rotate + 1f) * ((float)(actionSpace[1]-1) / 2f); 
        int actionBrake = (isBraking) ? (actionSpace[2]-1) : 0;       

        // set actions
        ActionSegment<int> actionArr = actionsOut.DiscreteActions;
        actionArr[0] = (int)actionMove;
        actionArr[1] = (int)actionRotate;
        actionArr[2] = (int)actionBrake;
    }

    /// <summary> 
    /// This method is used to execute actions when receiving output from the AI-model. 
    // <para>Overriden from the Agent class from the Unity ML-Agents Toolkit.</para> 
    /// </summary>
    public override void OnActionReceived(ActionBuffers actions)
    {      
        // check if episode is running & path is set (cannot act without path)
        if(!isEpisodeRunning || path == null)
            return;

        //--
        if (isEpisodeRunning)
        {
            // stop episode when signal is given
            if (stopEpisode) 
                FinishEpisode();

            // get action output
            float move = (float)actions.DiscreteActions[0];
            float rotate = (float)actions.DiscreteActions[1];
            float isBraking = (float)actions.DiscreteActions[2];

            // convert output to agent actions
            move = move / ((float)(actionSpace[0]-1) / 2f);
            move -= 1f;         
            rotate = rotate / ((float)(actionSpace[1]-1) / 2f);
            rotate -= 1f;     
            isBraking = isBraking / ((float)actionSpace[2]-1f);

            // store actions
            actionOutput[0] = move;
            actionOutput[1] = rotate;
            actionOutput[2] = isBraking;

            // set wheelcontroller steering
            wheelController.SetActions(move, rotate, isBraking);
        }
        else
        {
            // no actions if episode is over
            actionOutput[0] = 0;
            actionOutput[1] = 0;
            actionOutput[2] = 0;
        }
    }


    //--- REWARDS ---------------------------------------------------------------------
    /// <summary> 
    /// This method is calculates reward signals for the agent based on its state. 
    /// </summary>
    private void RewardCar()
    {
        //--
        float reward = 0f;
        Vector3 localVelocity = transform.InverseTransformDirection(velocityNew);

        // calculate rewards
        reward += REWARD_FUNCTIONS.Time();
        reward += REWARD_FUNCTIONS.Movement(localVelocity.z, actionOutput);
        reward += REWARD_FUNCTIONS.Checkpoint(isCheckPoint);
        reward += REWARD_FUNCTIONS.CheckpointDist(positionOld, positionNew, checkpoint);
        reward += REWARD_FUNCTIONS.CheckpointAngle(positionOld, forwardOld, positionNew, forwardNew, checkpoint);

        // set reward for agent's model
        AddReward(reward);

        // check for maximum negative reward to stop the episode
        float minReward = -10f;
        stopEpisode = (reward <= minReward) ? true : stopEpisode;

        // setup next step
        isCheckPoint = false;
    }


    //--- PATH ---------------------------------------------------------------------
    /// <summary> 
    /// This method is manages the agent's pathfinding. 
    /// </summary>
    private void UpdatePath()
    {
        // check if path exists
        if(path == null) 
            SetPath();

        // if path still doesn't exist => no directions
        if(path == null) 
            return;

        // update current checkpoint
        checkpoint = path[index];
        Vector3 checkpointVector = checkpoint - (this.transform.position + transform.forward * CONFIG.CHECKPOINT_OFFSET);
        checkpointVector.y = 0;

        // when close enough to checkpoint, increase index to proceed to next one
        if(checkpointVector.magnitude <= CONFIG.CHECKPOINT_RANGE)
        {
            // increment index of path
            index++;

            // check if endgoal is reached
            stopEpisode = (index >= path.Count) ? true : false;

            // update when checkpoint is reached
            index = (index > path.Count - 1) ? (path.Count - 1) : index;
            checkpoint = path[index];
            isCheckPoint = true;
            //Debug.Log("Update path, index=" + index);
        }
    }

    /// <summary> 
    /// This method is requests a path from the GPS based on the endgoal and the agent's position. 
    /// </summary>
    private void SetPath()
    {
        path = pathfinding.FindPath(this.transform.position, target.position);
        index = CONFIG.INDEX_START;
        //Debug.Log("Path calculated");
    }


    //--- COLLISIONS ---------------------------------------------------------------------
    /// <summary> 
    /// This method is executed when a trigger object is entered. 
    /// </summary>
    private void OnTriggerEnter(Collider other) 
    {
        if (other.tag == "Wall")
        {
            AddReward(REWARDS.WALL_ENTER);
            Debug.Log("Hit wall!");
            FinishEpisode();
        }
    }

    /// <summary> 
    /// This method is executed when collision started with an object. 
    /// </summary>
    private void OnCollisionEnter(Collision collision) 
    {

    }

    /// <summary> 
    /// This method is executed when collision keeps happening with an object. 
    /// </summary>
    private void OnCollisionStay(Collision collision) 
    {

    }


    //--- FINISH ---------------------------------------------------------------------
    /// <summary> 
    /// This method is executed to finish an episode and prevent other code from running. 
    /// </summary>

    private void FinishEpisode()
    {
        isEpisodeRunning = false;
        Debug.Log("End episode!");
        EndEpisode(); 
        // NOTE: EndEpisode calls CollectObservations. Avoid nesting cause this results in an infinite loop
    }

    //--- DEBUG ---------------------------------------------------------------------
    /// <summary> 
    /// This method updated debug information for the agent displayed when running. 
    /// </summary>
    private void DebugObservation()
    {
        // agent position + offset (to be able to see lines)
        Vector3 c = transform.position + Vector3.up * 1f;

        // draw lines for checkpoint
        Vector3 directionNew = checkpoint - positionNew;
        Debug.DrawLine(c, c + directionNew, Color.red);

        // draw lines for velocity
        Vector3 velocity = rigid.velocity;
        Debug.DrawLine(c, c + velocity, Color.blue);

        // update text
        debugTextMesh.text = actionOutput[0] + "\n" + actionOutput[1] + "\n" + actionOutput[2];
    }

    /// <summary> 
    /// This method is shows debug information for the pathfinding. 
    /// </summary>
    private void DebugPath()
    {
        // draw path
        Vector3 c = Vector3.up * 1f;
        for(int i = 0; i < path.Count-1; i++)
            Debug.DrawLine(c + path[i], c + path[i+1], Color.green, 1f);

        // draw line to goal
        Vector3 toGoal = (path[path.Count - 1] -this.transform.position);
        toGoal.y = 0;
        c += transform.position;
        Debug.DrawLine(c, c + toGoal, Color.magenta);
    }
}
