using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridAgent : MonoBehaviour
{
    //mlagents-learn config/CarAgent1.yaml --run-id=CarAgent1

    [SerializeField] GPS gps;
    [SerializeField] private Transform goal = null;
    private List<Vector3> path = null;
    private int index = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (path == null)
            path = gps.FindPath(this.transform.position, goal.position);

        float movementSpeed = 5f;
        Vector3 direction = path[index] - this.transform.position;
        direction.y = 0;

        if(direction.magnitude <= 0.1f)
        {
            index++;
            if (index >= path.Count)
            {
                path = gps.FindPath(this.transform.position, goal.position);
                index = 0;
            }
            direction = path[index] - this.transform.position;
            direction.y = 0;
        }
        direction.Normalize();

        direction.y = 0;
        transform.position += direction * Time.deltaTime * movementSpeed;
    }
}
