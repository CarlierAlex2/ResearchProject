using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigReward
{
    public static float TIME { get { return -0.05f; } }
    public static float BREAK { get { return -0.01f; } }
    public static float GOAL { get { return 100f; } }


    //--
    public static float WALL_ENTER { get { return -5f; } }
    public static float WALL_STAY { get { return -1f; } }


    //--
    public static float CHECKPOINT_RANGE { get { return 5f; } }
    public static float CHECKPOINT_PASS { get { return -5f; } }


    //--
    public static float VELOCITY_MIN { get { return -0.1f; } }
    public static float VELOCITY_FORWARD { get { return 0.1f; } }
    public static float STEERING_ANGLE { get { return -0.1f; } }

}
