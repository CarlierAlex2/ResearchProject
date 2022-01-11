using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class CarAgent : Agent
{
    [SerializeField] private Transform target;
    [SerializeField] private Pathfinding gps;

    private Rigidbody rigid;
    private CarAgentWheel wheelController;

    private List<Vector3> path;
    private int index = 0;
    private Vector3 direction;

    public override void Initialize()
    {
        rigid = GetComponent<Rigidbody>();
        wheelController = GetComponent<CarAgentWheel>();
    }

    public override void OnEpisodeBegin()
    {
        path = gps.FindPath(this.transform.position, target.position);
        index = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        UpdatePath();
        sensor.AddObservation(direction); 
    }

    private Vector3 GetDirection()
    {
        Vector3 d = path[index] - this.transform.position;
        d.y = 0;
        return d;
    }

    private void UpdatePath()
    {
        direction = GetDirection();
        if(direction.magnitude <= 0.1f)
        {
            index++;
            direction = GetDirection();
        }
        direction.Normalize();
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
        float move = actions.ContinuousActions[0];
        float rotate = actions.ContinuousActions[1];
        bool isBraking = (actions.DiscreteActions[0] == 1);

        wheelController.SetActions(move, rotate, isBraking);

        //force to move
        AddReward(-0.1f);

        //move in general direction
        Vector3 newDirection = GetDirection().normalized;
        Vector3 forward = transform.forward;
        forward.y = 0;
        forward.Normalize();
        float angle = Vector3.Angle(forward, newDirection);

        if(angle < 90 && angle > -90)
            AddReward(0.5f);
    }

    private void OnTriggerEnter(Collider other) {

        if (index == path.Count - 1)
        {
            AddReward(100f);
            EndEpisode();
        }
        if (other.tag == "Wall")
        {
            AddReward(-20f);
            EndEpisode();
        }
    }
}
