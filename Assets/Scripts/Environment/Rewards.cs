using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardFunctions
{
    private ConfigAgent CONFIG;
    private ConfigReward REWARDS;

    public RewardFunctions(ConfigAgent config, ConfigReward rewards)
    {
        this.CONFIG = config;
        this.REWARDS = rewards;
    }

    public float Time()
    {
        return REWARDS.TIME;
    }

    public float Movement(float velocityForward, float[] actionOutput)
    {
        float absVelocity = Mathf.Abs(velocityForward);
        float minVelocity = 0.01f;
        float reward = 0;

        // discourage standing still & not acting
        reward += (absVelocity > minVelocity) ? 1f : 0;
        reward += (actionOutput[2] > 0) ? REWARDS.BREAK : 0;

        // discourage going the opposite direction 
        reward += ((velocityForward > 0) && (actionOutput[0] < 0)) ? -0.3f : 0;
        reward += ((velocityForward < 0) && (actionOutput[0] > 0)) ? -0.3f : 0;

        //--
        return reward;
    }

    public float CheckpointDist(Vector3 oldPos, Vector3 newPos, Vector3 checkpoint)
    {
        oldPos.y = 0;
        newPos.y = 0;
        checkpoint.y = 0;

        Vector3 directionOld = checkpoint - oldPos;
        Vector3 directionNew = checkpoint - newPos;

        float reward = 0;

        float MAXIMUM_DISTANCE = 6.5f;

        // encourage moving closer & discourage moving further
        reward += (directionOld.magnitude > directionNew.magnitude) ? 0.1f : -0.05f;

        // discourage MAX distance
        reward += (directionNew.magnitude > MAXIMUM_DISTANCE) ? -10f : 0;

        //--
        return reward;
    }

    public float CheckpointAngle(Vector3 oldPos, Vector3 oldForward, Vector3 newPos, Vector3 newForward, Vector3 checkpoint)
    {
        oldPos.y = 0;
        newPos.y = 0;
        checkpoint.y = 0;

        Vector3 directionOld = checkpoint - oldPos;
        Vector3 directionNew = checkpoint - newPos;

        float angleOld = Mathf.Abs(Vector3.Angle(oldForward, directionOld.normalized));
        float angleNew = Mathf.Abs(Vector3.Angle(newForward, directionNew.normalized));
        float angleDiff = angleNew - angleOld;

        float reward = 0;
        float maxAngle = 90f;

        // encourage smaller angles = steering towards
        reward += (angleDiff < 0) ? 0f : -0.02f;

        // discourage MAX steering angle
        reward = (angleNew > maxAngle) ? -10f : 0;
        
        //--
        return reward;
    }
}
