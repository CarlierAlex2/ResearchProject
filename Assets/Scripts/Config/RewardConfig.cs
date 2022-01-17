using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardConfig
{
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
