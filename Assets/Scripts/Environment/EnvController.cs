using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvController : MonoBehaviour
{
    [SerializeField] private StartController startController;
    [SerializeField] private GoalController goalController;

    public void ResetEnvironment(Transform agentTransform)
    {
        startController.ResetStart(agentTransform);
        goalController.ResetGoal();
    }
}
