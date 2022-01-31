using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 
/// Component that helps with the environment setup.
/// </summary>
public class EnvController : MonoBehaviour
{
    // --- External objects ---
    [SerializeField] private GPS gpsController;
    [SerializeField] private StartController startController;
    [SerializeField] private GoalController goalController;

    // --- Init ---
    private bool isInit = false;

    //--- INITIALIZE ---------------------------------------------------------------------
    /// <summary> 
    /// This method initializes the environment, including the pathfinding grid. 
    /// </summary>
    public void InitEnvironment()
    {
        // check if is initialised
        if(isInit)
            return;
        isInit = true;

        // initialise gps
        gpsController.Init();
        gpsController.SetupObstacles();
    }

    //--- RESET ---------------------------------------------------------------------
    /// <summary> 
    /// This method resets the environment, including the goal and agent's startposition grid. 
    /// </summary>
    public void ResetEnvironment(Transform agentTransform)
    {
        // agent start position
        startController.ResetStart(agentTransform);

        // random endgoal position
        bool isGoalSet = false;
        int t = 0;
        while(!isGoalSet && t < 10)
        {
            // reset goal again if there is no walkable tile
            goalController.ResetGoal();
            isGoalSet = gpsController.IsWalkable(goalController.transform.position);
            t++;
            
            //if(!isGoalSet)
            //    new WaitForSeconds(1);
        }
    }
}
