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
        this.CHECKPOINT_ANGLE_MAX = 0;
        //--
        this.VELOCITY_MIN = 0;
        this.STEERING_ANGLE = 0;
    }
}

public class ConfigReward_5_1 : ConfigReward
{
    public ConfigReward_5_1() : base()
    {
        this.TIME = -0.01f;
        this.FORWARD = 0.002f;
        this.BREAK = -0.005f;
        this.GOAL = 0;
        //--
        this.WALL_ENTER = -10f;
        this.WALL_STAY = -0.1f;
        //--
        this.CHECKPOINT_RANGE = 2f;
        this.CHECKPOINT_PASS = 0;
        //--
        this.VELOCITY_MIN = 0;
        this.VELOCITY_FORWARD = 0;
        this.STEERING_ANGLE = 0;
    }
}

//-------------------------------------------------------------------


