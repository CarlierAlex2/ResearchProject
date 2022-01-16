using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvController : MonoBehaviour
{
    [SerializeField] private GPS gpsController;
    [SerializeField] private StartController startController;
    [SerializeField] private GoalController goalController;
    private bool isInit = false;

    public void InitEnvironment()
    {
        if(isInit)
            return;

        isInit = true;
        gpsController.Init();
        gpsController.SetupObstacles();
    }

    public void ResetEnvironment(Transform agentTransform)
    {
        startController.ResetStart(agentTransform);

        bool isGoalSet = false;
        int t = 0;
        while(!isGoalSet && t < 10)
        {
            goalController.ResetGoal();
            isGoalSet = gpsController.IsWalkable(goalController.transform.position);
            t++;
            //if(!isGoalSet)
            //    new WaitForSeconds(1);
        }
    }
}
