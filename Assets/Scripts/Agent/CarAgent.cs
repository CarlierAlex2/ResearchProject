using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class CarAgent : Agent
{
    [SerializeField] private Transform target;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotateSpeed = 120f;

    private Rigidbody rigid;

    public override void Initialize()
    {
        rigid = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        this.transform.localPosition = new Vector3(Random.Range(-9f, 0f), 0, Random.Range(-9f, 9f));
        this.transform.rotation = Quaternion.Euler(Vector3.up * Random.Range(0f, 360f));
        rigid.velocity = Vector3.zero;

        target.localPosition = new Vector3(Random.Range(0f, 9f), 0, Random.Range(-9f, 9f));
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        int move = Mathf.RoundToInt(Input.GetAxisRaw("Vertical"));
        int rotate = Mathf.RoundToInt(Input.GetAxisRaw("Horizontal"));

        ActionSegment<int> actions = actionsOut.DiscreteActions;
        actions[0] = move + 1;
        actions[1] = rotate + 1;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float move = actions.DiscreteActions[0] - 1;
        float rotate = actions.DiscreteActions[1] - 1;

        transform.localPosition += transform.forward * move * moveSpeed * Time.deltaTime;
        transform.Rotate(0, rotate * rotateSpeed * Time.deltaTime, 0); 
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //sensor.AddObservation(target.localPosition); 
    }

    private void OnTriggerEnter(Collider other) {

        if (other.tag == "Goal")
        {
            AddReward(1f);
            EndEpisode();
        }
        if (other.tag == "Wall")
        {
            AddReward(-1f);
            EndEpisode();
        }
    }
}
