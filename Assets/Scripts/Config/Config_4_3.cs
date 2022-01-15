using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentConfig_4_3
{
    public static bool hasInstance = false;
    private static AgentConfig_4_3 instance;
    public static AgentConfig_4_3 Instance
    {
        get 
        {
            if (!hasInstance)
            {
                hasInstance = true;
                instance = new AgentConfig_4_3();
            }
            return instance;
        }
    }

    //--
    public int INDEX_START { get { return 1; } }


    //--
    public float CHECKPOINT_OFFSET = 1f;


    //--
    public float CHECKPOINT_RANGE = 2f;
    public float CHECKPOINT_ANGLE_MAX { get { return 140f; } }


    //--
    public float VELOCITY_MIN { get { return 0.1f; } }
    public float STEERING_ANGLE { get { return 60f; } }
}

public class RewardConfig_4_3
{
    public static bool hasInstance = false;
    private static RewardConfig_4_3 instance;
    public static RewardConfig_4_3 Instance
    {
        get 
        {
            if (!hasInstance)
            {
                hasInstance = true;
                instance = new RewardConfig_4_3();
            }
            return instance;
        }
    }

    //--
    public float TIME { get { return -0.05f; } }
    public float BREAK { get { return -0.5f; } }
    public float GOAL { get { return 100f; } }


    //--
    public float WALL_ENTER { get { return -5f; } }
    public float WALL_STAY { get { return -1f; } }


    //--
    public float CHECKPOINT_RANGE { get { return 5f; } }
    public float CHECKPOINT_PASS { get { return -5f; } }


    //--
    public float VELOCITY_MIN { get { return -2f; } }
    public float VELOCITY_FORWARD { get { return 0.1f; } }
    public float STEERING_ANGLE { get { return -0.1f; } }

}
