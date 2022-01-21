using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigAgent_6_1 : ConfigAgent
{
    public ConfigAgent_6_1() : base()
    {
        this.INDEX_START = 1;
        //--
        this.CHECKPOINT_OFFSET = 0;
        this.CHECKPOINT_RANGE = 2.3f;
        this.CHECKPOINT_ANGLE_MAX = 0;
        this.CHECKPOINT_DIST = 5f;
        //--
        this.VELOCITY_MIN = 0;
        this.STEERING_ANGLE = 30f;
        this.STEERING_ANGLE_MIN = 15f;
    }
}

public class ConfigReward_6_1 : ConfigReward
{
    public ConfigReward_6_1() : base()
    {
        this.TIME = -0.0001f;
        this.FORWARD = 0.005f;
        this.BREAK = -0.05f;
        this.GOAL = 0;
        //--
        this.WALL_ENTER = -5f;
        this.WALL_STAY = 0;
        //--
        this.CHECKPOINT_RANGE = 0.5f;
        this.CHECKPOINT_PASS = 0;
        this.CHECKPOINT_DIST = 0.05f;
        //--
        this.VELOCITY_MIN = 0;
        this.VELOCITY_FORWARD = 0;
        this.STEERING_ANGLE = 0.01f;
    }
}

//-------------------------------------------------------------------


