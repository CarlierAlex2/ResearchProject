using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 
/// Component that helps with the environment setup for the goal.
/// </summary>
public class GoalController : MonoBehaviour
{
    // --- Minimum & maximum ---
    [SerializeField] private float [] rangeX = {7, 36};
    [SerializeField] private float [] rangeZ = {7, 36};

    //--- RESET ---------------------------------------------------------------------
    /// <summary> 
    /// This method resets the goal to a random position within the defined X and Z limits. 
    /// </summary>
    public void ResetGoal()
    {
        //generate random X and Z values
        float randomX = Random.Range(rangeX[0], rangeX[1] + 1);
        float randomZ = Random.Range(rangeZ[0], rangeZ[1] + 1);

        //place agent at position
        this.transform.localPosition = new Vector3(randomX, 1, randomZ);
    }
}
