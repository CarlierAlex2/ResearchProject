using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class TestAgent : Agent
{
    [SerializeField] private Transform target;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotateSpeed = 120f;
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer floorRenderer;

    private Rigidbody rigid;

    public override void Initialize()
    {
        rigid = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        //Debug.Log("OnEpisodeBegin");
        //reset episode
        this.transform.localPosition = new Vector3(Random.Range(-9f, 0f), 0, Random.Range(-9f, 9f));
        this.transform.rotation = Quaternion.Euler(Vector3.up * Random.Range(0f, 360f));
        rigid.velocity = Vector3.zero;

        target.localPosition = new Vector3(Random.Range(0f, 9f), 0, Random.Range(-9f, 9f));
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Read input values
        int move = Mathf.RoundToInt(Input.GetAxisRaw("Vertical"));
        int rotate = Mathf.RoundToInt(Input.GetAxisRaw("Horizontal"));

        // Convert the actions to discrete
        ActionSegment<int> actions = actionsOut.DiscreteActions;
        actions[0] = move + 1; // Input -1 = action 0
        actions[1] = rotate + 1;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        //MLAgent only works with numbers, doesn't fully know the agent
        float move = actions.DiscreteActions[0] - 1;
        float rotate = actions.DiscreteActions[1] - 1;

        transform.localPosition += transform.forward * move * moveSpeed * Time.deltaTime;
        transform.Rotate(0, rotate * rotateSpeed * Time.deltaTime, 0); 
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //How agent observes it's environment
        //sensor.AddObservation(this.transform.localPosition); //vector3 holds 3 values => 3 obeservation values
        //sensor.AddObservation(target.localPosition); 
    }

    private void OnTriggerEnter(Collider other) {

        if (other.tag == "Goal")
        {
            floorRenderer.material = winMaterial;
            SetReward(1f);
            EndEpisode();
        }
        if (other.tag == "Wall")
        {
            floorRenderer.material = loseMaterial;
            SetReward(-1f);
            EndEpisode();
        }
        //AddReward();
    }
}
