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

        this.CHECKPOINT_ANGLE = 20f;
        this.CHECKPOINT_ANGLE_MAX = 120f;

        this.CHECKPOINT_DIST = 5f;
        this.CHECKPOINT_DIST_MAX = 6.5f;
        //--
        this.VELOCITY_MIN = 0.01f;
        this.STEERING_ANGLE = 30f;
        this.STEERING_ANGLE_MIN = 15f;
    }
}

public class ConfigReward_6_1 : ConfigReward
{
    public ConfigReward_6_1() : base()
    {
        this.TIME = -0.001f;
        this.FORWARD = 0.005f;
        this.BREAK = -0.05f;
        this.GOAL = 0;
        //--
        this.WALL_ENTER = -5f;
        this.WALL_STAY = 0;
        //--
        this.CHECKPOINT_RANGE = 0.5f;
        this.CHECKPOINT_PASS = 0;

        this.CHECKPOINT_DIST = 0.1f;
        this.CHECKPOINT_DIST_NEG = -0.05f;
        this.CHECKPOINT_DIST_MAX = -10f;

        this.CHECKPOINT_ANGLE = 0.05f; //0.01f;
        this.CHECKPOINT_ANGLE_NEG = 0.05f;//-0.01f;
        this.CHECKPOINT_ANGLE_MAX = -10f;
        //--
        this.VELOCITY_MIN = 1f;
        this.VELOCITY_FORWARD = -0.3f;
        this.STEERING_ANGLE = 0.01f;
    }
}

//-------------------------------------------------------------------


public class ConfigAgent_6_2 : ConfigAgent
{
    public ConfigAgent_6_2() : base()
    {
        this.INDEX_START = 1;
        //--
        this.CHECKPOINT_OFFSET = 0;
        this.CHECKPOINT_RANGE = 2.3f;

        this.CHECKPOINT_ANGLE = 20f;
        this.CHECKPOINT_ANGLE_MAX = 120f;

        this.CHECKPOINT_DIST = 5f;
        this.CHECKPOINT_DIST_MAX = 6.5f;
        //--
        this.VELOCITY_MIN = 0.01f;
        this.STEERING_ANGLE = 30f;
        this.STEERING_ANGLE_MIN = 15f;
    }
}

public class ConfigReward_6_2 : ConfigReward
{
    public ConfigReward_6_2() : base()
    {
        this.TIME = -0.01f;
        this.FORWARD = 0.005f;
        this.BREAK = -0.05f;
        this.GOAL = 0;
        //--
        this.WALL_ENTER = -5f;
        this.WALL_STAY = 0;
        //--
        this.CHECKPOINT_RANGE = 0.5f;
        this.CHECKPOINT_PASS = 0;

        this.CHECKPOINT_DIST = 0.1f;
        this.CHECKPOINT_DIST_NEG = -0.05f;
        this.CHECKPOINT_DIST_MAX = -10f;

        this.CHECKPOINT_ANGLE = 0.05f; //0.01f;
        this.CHECKPOINT_ANGLE_NEG = 0.05f;//-0.01f;
        this.CHECKPOINT_ANGLE_MAX = -10f;
        //--
        this.VELOCITY_MIN = 1f;
        this.VELOCITY_FORWARD = -0.3f;
        this.STEERING_ANGLE = 0.01f;
    }
}
