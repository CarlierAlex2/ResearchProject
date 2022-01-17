using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigAgent_5_1 : ConfigAgent
{
    public ConfigAgent_5_1() : base()
    {
        this.INDEX_START = 1;
        //--
        this.CHECKPOINT_OFFSET = 1f;
        this.CHECKPOINT_RANGE = 2.3f;
        this.CHECKPOINT_ANGLE_MAX = 140f;
        //--
        this.VELOCITY_MIN = 0.1f;
        this.STEERING_ANGLE = 30f;
    }
}

public class ConfigReward_5_1 : ConfigReward
{
    public ConfigReward_5_1() : base()
    {
        this.TIME = -0.1f;
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
        this.STEERING_ANGLE = -0.5f;
    }
}

//-------------------------------------------------------------------

