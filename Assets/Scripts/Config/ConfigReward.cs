using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        //--
        this.VELOCITY_MIN = -2f;
        this.VELOCITY_FORWARD = 0.1f;
        this.STEERING_ANGLE = -0.1f;
    }

    //--
    public float TIME { get; internal set; }
    public float FORWARD { get; internal set; }
    public float BREAK { get; internal set; }
    public float GOAL { get; internal set; }
    //--
    public float WALL_ENTER { get; internal set; }
    public float WALL_STAY { get; internal set; }
    //--
    public float CHECKPOINT_RANGE { get; internal set; }
    public float CHECKPOINT_PASS { get; internal set; }
    //--
    public float VELOCITY_MIN { get; internal set; }
    public float VELOCITY_FORWARD { get; internal set; }
    public float STEERING_ANGLE { get; internal set; }
}
