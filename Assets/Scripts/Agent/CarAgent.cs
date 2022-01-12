using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class CarAgent : Agent
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform gpsObj;
    private GPS pathfinding;

    private Rigidbody rigid;
    private CarAgentWheel wheelController;


    private Vector3 startPos;
    private Quaternion startRot;

    private List<Vector3> path;
    private int index = 0;
    private Vector3 direction;
    private bool isEpisodeRunning = false;

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
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if(!isEpisodeRunning)
            return;

        UpdatePath();
        sensor.AddObservation(direction); 
        sensor.AddObservation(rigid.velocity.magnitude); 
    }

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
        if(!isEpisodeRunning)
            return;

        float move = actions.ContinuousActions[0];
        float rotate = actions.ContinuousActions[1];
        bool isBraking = (actions.DiscreteActions[0] == 1);

        wheelController.SetActions(move, rotate, isBraking);

        //force to move
        AddReward(-0.1f);

        //move in general direction
        float angle = GetAngleDirection();
        if(Mathf.Abs(angle) < 90)
            AddReward(0.5f);
        else
            AddReward(-1f);

        if(rigid.velocity.magnitude > 0.05f)
            AddReward(0.05f);

        if (index >= path.Count - 1)
        {
            AddReward(100f);
            FinishEpisode();
        }
    }

    private Vector3 GetDirection()
    {
        if(index < path.Count)
        {
            Vector3 d = path[index] - this.transform.position;
            d.y = 0;
            return d;
        }
        return Vector3.zero;
    }

    private void UpdatePath()
    {
        if(path == null)
            SetPath();
        float angle = GetAngleDirection();
        if (Mathf.Abs(angle) > 120)
        {
            SetPath();
            AddReward(-10f);
        }
            
        if(path == null)
            direction = Vector3.zero;
        else
        {
            direction = GetDirection();
            if(direction.magnitude <= 4f)
            {
                Debug.Log("Update path, index=" + index);
                index++;
                direction = GetDirection();
                AddReward(10f);
            }
            direction.Normalize();
        }
    }

    private void SetPath()
    {
        path = pathfinding.FindPath(this.transform.position, target.position);
        index = 0;
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

    private void OnTriggerEnter(Collider other) {

        if (other.tag == "Wall")
        {
            AddReward(-20f);
            FinishEpisode();
            Debug.Log("End episode!");
        }
    }

    private void FinishEpisode()
    {
        isEpisodeRunning = true;
        EndEpisode();
    }
}
