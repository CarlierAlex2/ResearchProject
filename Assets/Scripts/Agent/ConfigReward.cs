using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigReward
{
    public static float TIME { get { return -0.1f; } }
    public static float BREAK { get { return -1f; } }
    public static float VELOCITY_MIN { get { return -0.2f; } }
    public static float VELOCITY_ANGLE { get { return  0.2f; } }

    public static float WALL_ENTER { get { return -5f; } }
    public static float WALL_STAY { get { return -1f; } }

    public static float CHECKPOINT { get { return 5f; } }
    public static float CHECKPOINT_PASS { get { return -3f; } }
    public static float GOAL { get { return 100f; } }
}
