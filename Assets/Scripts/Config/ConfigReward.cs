using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary> 
/// Configuration class for reward signals.
/// </summary>
public class ConfigReward
{
    public ConfigReward()
    {
        this.TIME = -0.1f;
        this.FORWARD = 0;
        this.BREAK = -0.5f;
        this.GOAL = 100f;
        //--
        this.WALL_ENTER = -5f;
        this.WALL_STAY = -1f;
        //--
        this.CHECKPOINT_RANGE = 5f;
        this.CHECKPOINT_PASS = -5f;

        this.CHECKPOINT_DIST = 0;
        this.CHECKPOINT_DIST_NEG = 0;
        this.CHECKPOINT_DIST_MAX = 0;

        this.CHECKPOINT_ANGLE = 0;
        this.CHECKPOINT_ANGLE_NEG = 0;
        this.CHECKPOINT_ANGLE_MAX = 0;
        //--
        this.VELOCITY_MIN = -2f;
        this.VELOCITY_FORWARD = 0.1f;
        this.STEERING_ANGLE = -0.1f;
    }

    // --- Time ---
    public float TIME { get; internal set; }

    // --- Actions ---
    public float FORWARD { get; internal set; }
    public float BREAK { get; internal set; }

    // --- Collision ---
    public float WALL_ENTER { get; internal set; }
    public float WALL_STAY { get; internal set; }

    // --- Pathfinding ---
    public float GOAL { get; internal set; }

    //--
    public float CHECKPOINT_RANGE { get; internal set; }
    public float CHECKPOINT_PASS { get; internal set; }

    //--
    public float CHECKPOINT_DIST { get; internal set; }
    public float CHECKPOINT_DIST_NEG { get; internal set; }
    public float CHECKPOINT_DIST_MAX { get; internal set; }

    //--
    public float CHECKPOINT_ANGLE { get; internal set; }
    public float CHECKPOINT_ANGLE_NEG { get; internal set; }
    public float CHECKPOINT_ANGLE_MAX { get; internal set; }

    // --- Velocity ---
    public float VELOCITY_MIN { get; internal set; }
    public float VELOCITY_FORWARD { get; internal set; }

    // --- Steering ---
    public float STEERING_ANGLE { get; internal set; }
}
