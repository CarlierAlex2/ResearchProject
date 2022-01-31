using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 
/// Class containing reward calculation functions.
/// </summary>
public class RewardFunctions
{
    private ConfigAgent CONFIG;
    private ConfigReward REWARDS;

    public RewardFunctions(ConfigAgent config, ConfigReward rewards)
    {
        this.CONFIG = config;
        this.REWARDS = rewards;
    }

    /// <summary> 
    /// Calculate reward for steps taken by agent's.
    /// </summary>
    public float Time()
    {
        return REWARDS.TIME;
    }

    /// <summary> 
    /// Calculate reward for agent's movement.
    /// </summary>
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

    /// <summary> 
    /// Calculate reward for agent reaching the checkpoint.
    /// </summary>
    public float Checkpoint(bool isCheckPoint)
    {
        return (isCheckPoint) ? REWARDS.CHECKPOINT_PASS : 0;
    }

    /// <summary> 
    /// Calculate reward for agent's distance to checkpoint.
    /// </summary>
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

    /// <summary> 
    /// Calculate reward for agent's angle between forward vector and direction to checkpoint.
    /// </summary>
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
        reward += ((angleOld > CONFIG.CHECKPOINT_ANGLE) && (angleDiff >= 0)) ?  REWARDS.CHECKPOINT_ANGLE_NEG : REWARDS.CHECKPOINT_ANGLE;

        // discourage MAX steering angle
        reward = (angleNew > CONFIG.CHECKPOINT_ANGLE_MAX) ? REWARDS.CHECKPOINT_ANGLE_MAX : 0;
        
        //--
        return reward;
    }
}
