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
        float reward = 0;

        // discourage standing still & not acting
        reward += (absVelocity > CONFIG.VELOCITY_MIN) ? REWARDS.VELOCITY_MIN : 0;
        reward += (actionOutput[2] > 0) ? REWARDS.BREAK : 0;

        // discourage going the opposite direction 
        reward += ((velocityForward > 0) && (actionOutput[0] < 0)) ? REWARDS.VELOCITY_FORWARD : 0;
        reward += ((velocityForward < 0) && (actionOutput[0] > 0)) ? REWARDS.VELOCITY_FORWARD : 0;

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

        // encourage moving closer & discourage moving further
        reward += (directionOld.magnitude > directionNew.magnitude) ? REWARDS.CHECKPOINT_DIST : REWARDS.CHECKPOINT_DIST_NEG;

        // discourage MAX distance
        reward += (directionNew.magnitude > CONFIG.CHECKPOINT_DIST_MAX) ? REWARDS.CHECKPOINT_DIST_MAX : 0;

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

        // encourage smaller angles = steering towards
        reward += (angleDiff < 0) ? 0 : REWARDS.CHECKPOINT_ANGLE;

        // discourage MAX steering angle
        reward = (angleNew > CONFIG.CHECKPOINT_ANGLE_MAX) ? REWARDS.CHECKPOINT_ANGLE_MAX : 0;
        
        //--
        return reward;
    }
}
