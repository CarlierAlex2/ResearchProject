using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalController : MonoBehaviour
{
    [SerializeField] private float [] rangeX = {7, 36};
    [SerializeField] private float [] rangeZ = {7, 36};

    public void ResetGoal()
    {
        float randomX = Random.Range(rangeX[0], rangeX[1] + 1);
        float randomZ = Random.Range(rangeZ[0], rangeZ[1] + 1);
        this.transform.localPosition = new Vector3(randomX, 1, randomZ);
    }
}
